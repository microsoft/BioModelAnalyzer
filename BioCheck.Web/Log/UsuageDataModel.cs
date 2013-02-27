using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.StorageClient;

namespace BioCheck.Web.Log
{
    public class UsuageDataModel : TableServiceEntity
    {
        public UsuageDataModel(string partitionKey, string rowKey)
            : base(partitionKey, rowKey)
        {

        }

        public UsuageDataModel()
            : this(Guid.NewGuid().ToString(), Guid.NewGuid().ToString())
        {

        }

        public string UserId { get; set; }

        public DateTime LogInTime { get; set; }

        public DateTime LogOutTime { get; set; }

        public Int32 Duration { get; set; }

        public Int32 RunProof { get; set; }

        public Int32 RunSimulation { get; set; }

        public Int32 NewModel { get; set; }

        public Int32 ImportModel { get; set; }

        public Int32 SaveModel { get; set; }

        public Int32 FurtherTesting { get; set; }

        public string Version { get; set; }
    }
}
