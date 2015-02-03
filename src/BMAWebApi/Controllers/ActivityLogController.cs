using BioCheckAnalyzerCommon;
using BioModelAnalyzer;
using BMAWebApi;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace bma.client.Controllers
{
    public class ActivityRecord
    {
        public string SessionID { get; set; }

        public string UserID { get; set; }

        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime LogInTime { get; set; }

        [JsonConverter(typeof(IsoDateTimeConverter))]
        public DateTime LogOutTime { get; set; }

        public Int32 RunProofCount { get; set; }

        public Int32 ProofErrorCount { get; set; }

        public Int32 RunSimulationCount { get; set; }

        public Int32 SimulationErrorCount { get; set; }

        public Int32 NewModelCount { get; set; }

        public Int32 ImportModelCount { get; set; }

        public Int32 SaveModelCount { get; set; }

        public Int32 FurtherTestingCount { get; set; }

        public Int32 FurtherTestingErrorCount { get; set; }

        public string ClientVersion { get; set; }
    }

    public class ActivityLogController : ApiController
    {
        // POST api/Analyze
        public void Post([FromBody]ActivityRecord record)
        {
            var entity = new ActivityEntity(record.SessionID, record.UserID)
            {
                LogInTime = record.LogInTime,
                LogOutTime = record.LogOutTime,
                FurtherTestingCount = record.FurtherTestingCount,
                ClientVersion = record.ClientVersion,
                ImportModelCount = record.ImportModelCount,
                NewModelCount = record.NewModelCount,
                RunProofCount = record.RunProofCount,
                RunSimulationCount = record.RunSimulationCount,
                SaveModelCount = record.SaveModelCount,
                ProofErrorCount = record.ProofErrorCount,
                SimulationErrorCount = record.SimulationErrorCount,
                FurtherTestingErrorCount = record.FurtherTestingErrorCount
            };

            ActivityAzureLogger logger = new ActivityAzureLogger(
                CloudStorageAccount.Parse(
                    RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString")));
                   // CloudConfigurationManager.GetSetting("StorageConnectionString")));
            logger.Add(entity);
        }
    }
}