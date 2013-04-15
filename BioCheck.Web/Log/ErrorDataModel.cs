using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace BioCheck.Web.Log
{
    public class ErrorDataModel : TableEntity
    {
        public ErrorDataModel(string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {

        }

        public ErrorDataModel()
            : this(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        {

        }

        public string UserId { get; set; }

        public DateTime Date { get; set; }

        public string Message { get; set; }

        public string Details { get; set; }

        public string Version { get; set; }
    }
}
