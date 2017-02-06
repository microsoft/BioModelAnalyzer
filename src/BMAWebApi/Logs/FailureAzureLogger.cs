// Copyright (c) Microsoft Research 2016
// License: MIT. See LICENSE
using bma.client;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMAWebApi
{
    public class FailureEntity : TableEntity
    {
        public FailureEntity(string key, string backEndVersion)
        {
            this.RowKey = key;
            this.PartitionKey = backEndVersion;
        }

        public string BackEndVersion
        {
            get { return PartitionKey;  }
        }

        public DateTime DateTime { get; set; }
    }

    public interface ILogContents
    {
        string[] ErrorMessages { get;  }
        string[] DebugMessages { get; }
    }

    public interface IFailureLogger
    {
        void Add(DateTime dateTime, string backEndVersion, object request, ILogContents contents);
    }

    public class FailureAzureLogger : IFailureLogger
    {
        private CloudTableClient tableClient;
        private CloudTable failuresTable;
        private CloudBlobClient blobClient;
        private CloudBlobContainer failuresContainer;

        public FailureAzureLogger(CloudStorageAccount account)
        {
            if (account == null) throw new ArgumentNullException("account");
            tableClient = account.CreateCloudTableClient();
            blobClient = account.CreateCloudBlobClient();
            failuresContainer = blobClient.GetContainerReference("failures");
            failuresContainer.CreateIfNotExists();            
            failuresTable = tableClient.GetTableReference("ServiceFailures");
            failuresTable.CreateIfNotExists();
        }

        public FailureAzureLogger(string connectionString) : this(CloudStorageAccount.Parse(connectionString))
        {
        }

        public void Add(DateTime dateTime, string backEndVersion, object request, ILogContents log)
        {
            var uniqueName = Guid.NewGuid().ToString();

            string inputBlobName = String.Concat(uniqueName, "_request");
            try
            {               
                var inputBlob = failuresContainer.GetBlockBlobReference(inputBlobName);
                inputBlob.UploadText(JsonConvert.SerializeObject(request, Formatting.Indented));
            }
            catch (Exception exc)
            {
                Trace.WriteLine("Error writing blob: " + exc.Message);
                inputBlobName = null;
            }

            string outputBlobName = String.Concat(uniqueName, "_result");
            try
            {
                var outputBlob = failuresContainer.GetBlockBlobReference(outputBlobName);
                using (var stream = outputBlob.OpenWrite())
                {
                    var writer = new StreamWriter(stream);
                    if (log.ErrorMessages != null && log.ErrorMessages.Length > 0)
                        writer.WriteLine("Error messages:\n{0}\n\n", String.Join("\n", log.ErrorMessages));
                    if (log.DebugMessages != null && log.DebugMessages.Length > 0)
                        writer.WriteLine("Debug messages:\n{0}\n", String.Join("\n", log.DebugMessages));
                    writer.Flush();
                }
            }
            catch (Exception exc)
            {
                Trace.WriteLine("Error writing blob: " + exc.Message);
                outputBlobName = null;
            }

            failuresTable.Execute(TableOperation.Insert(new FailureEntity(uniqueName, backEndVersion) {
                 DateTime = dateTime
            }));
        }
    }
}
 
