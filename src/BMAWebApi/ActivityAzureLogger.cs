using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BMAWebApi
{
    public class ActivityEntity : TableEntity
    {
        public ActivityEntity(string sessionID, string userID)
        {
            this.RowKey = sessionID;
            this.PartitionKey = userID;
        }

        public string SessionID 
        {
            get { return RowKey;  }
        }

        public string UserID
        {
            get { return PartitionKey;  }
        }

        public DateTime LogInTime { get; set; }

        public DateTime LogOutTime { get; set; }

        public Int32 RunProofCount { get; set; }

        public Int32 RunSimulationCount { get; set; }

        public Int32 NewModelCount { get; set; }

        public Int32 ImportModelCount { get; set; }

        public Int32 SaveModelCount { get; set; }

        public Int32 FurtherTestingCount { get; set; }

        public string ClientVersion { get; set; }

        public int SimulationErrorCount { get; set; }

        public int FurtherTestingErrorCount { get; set; }

        public int ProofErrorCount { get; set; }
    }

    public class ActivityAzureLogger
    {
        private CloudTableClient tableClient;
        private CloudTable activityTable;

        public ActivityAzureLogger(CloudStorageAccount account)
        {
            tableClient = account.CreateCloudTableClient();
            activityTable = tableClient.GetTableReference("ClientActivity");
            activityTable.CreateIfNotExists();
        }

        public void Add(ActivityEntity entity)
        {
            activityTable.Execute(TableOperation.Insert(entity));
        }
    }
}
