using BioCheckAnalyzerCommon;
using BioModelAnalyzer;
using BMAWebApi;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
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
    public class SimulationInput
    {
        public Model Model { get; set; }

        public SimulationVariable[] Variables { get; set; }

        public bool EnableLogging { get; set; }
    }

    public class SimulationOutput
    {
        public SimulationVariable[] Variables { get; set; }

        public string[] ErrorMessages { get; set; }

        public string[] DebugMessages { get; set; }
    }

    public class SimulateController : ApiController
    {
        // POST api/Analyze
        public SimulationOutput Post([FromBody]SimulationInput input)
        {           
            var log = new DefaultLogService();

            try 
            {         
                IAnalyzer analyzer = new UIMain.Analyzer();

                var analyisStartTime = DateTime.Now;

                if (!input.EnableLogging)
                    log.LogDebug("Enable Logging from the Run Proof button context menu to see more detailed logging info.");

                var logger = input.EnableLogging ? log : null;
                if (logger != null)
                {
                    analyzer.LoggingOn(log);
                }
                else
                {
                    analyzer.LoggingOff();
                } 
                
                // Prepare model for analysis
                var model = (Model)input.Model;

                var output = Utilities.RunWithTimeLimit(() => analyzer.simulate_tick(model, input.Variables), Utilities.GetTimeLimitFromConfig());

                return new SimulationOutput
                {
                    Variables = output,
                    ErrorMessages = log.ErrorMessages.Length > 0 ? log.ErrorMessages.ToArray() : null,
                    DebugMessages = log.DebugMessages.Length > 0 ? log.DebugMessages.ToArray() : null,
                };

                //outputData.ErrorMessages = log.ErrorMessages;
                //outputData.ZippedLog = ZipHelper.Zip(string.Join(Environment.NewLine, log.DebugMessages));
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                FailureAzureLogger logger = new FailureAzureLogger(
                    CloudStorageAccount.Parse(
                        RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString")));
                logger.Add(DateTime.Now, "2.0", input, log);

                return new SimulationOutput
                {
                    ErrorMessages = log.ErrorMessages.Length > 0 ? log.ErrorMessages.ToArray() : null,
                    DebugMessages = log.DebugMessages.Length > 0 ? log.DebugMessages.ToArray() : null
                };
            }
        }
    }
}