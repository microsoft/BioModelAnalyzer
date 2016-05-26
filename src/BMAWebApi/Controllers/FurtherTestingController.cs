using BioCheckAnalyzerCommon;
using bma.client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Linq;
using Microsoft.FSharp.Core;
using BioModelAnalyzer;
using BMAWebApi;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace bma.client.Controllers
{
    public class FurtherTestingInput
    {
        public Model Model { get; set; }

        public AnalysisResult Analysis { get; set; }

        public bool EnableLogging { get; set; }
    }

    public class FurtherTestingOutput
    {
        public CounterExampleOutput[] CounterExamples { get; set; }

        public string[] ErrorMessages { get; set; }

        public string[] DebugMessages { get; set; }

        public string Error { get; set; }
    }

    public class FurtherTestingController : ApiController
    {
        private readonly IFailureLogger logger;

        public FurtherTestingController(IFailureLogger logger)
        {
            this.logger = logger;
        }

        public FurtherTestingOutput Post([FromBody]FurtherTestingInput input)
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

                // SI: these all return a single CEx, not an array of them. 
                var result = Utilities.RunWithTimeLimit(() => new                    
                    {
                        cexBifurcates = analyzer.findCExBifurcates(model, input.Analysis),
                        cexCycles = analyzer.findCExCycles(model, input.Analysis),
                        cexFixPoints = analyzer.findCExFixpoint(model, input.Analysis),
                    }, Utilities.GetTimeLimitFromConfig());


                var cexs = new List<CounterExampleOutput>();
                if (FSharpOption<BifurcationCounterExample>.get_IsSome(result.cexBifurcates))
                { 
                    cexs.Add(result.cexBifurcates.Value); 
                }
                if (FSharpOption<CycleCounterExample>.get_IsSome(result.cexCycles))
                {
                    cexs.Add(result.cexCycles.Value);
                }
                if (FSharpOption<FixPointCounterExample>.get_IsSome(result.cexFixPoints))
                {
                    cexs.Add(result.cexFixPoints.Value);
                }

                log.LogDebug(string.Format("Finding Counter Examples took {0} seconds to run.", (DateTime.Now - analyisStartTime).TotalSeconds));

                return new FurtherTestingOutput 
                {
                    CounterExamples = cexs.ToArray(), 
                    ErrorMessages = log.ErrorMessages.Length > 0 ? log.ErrorMessages.ToArray() : null,
                    DebugMessages = log.DebugMessages.Length > 0 ? log.DebugMessages.ToArray() : null
                };
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());

                //FailureAzureLogger logger = new FailureAzureLogger(
                //    CloudStorageAccount.Parse(
                //        RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString")));
                var version = typeof(AnalyzeController).Assembly.GetName().Version;
                logger.Add(DateTime.Now, version.ToString(), input, log);

                return new FurtherTestingOutput
                {
                    Error = ex.Message,
                    ErrorMessages = log.ErrorMessages.Length > 0 ? log.ErrorMessages.ToArray() : null,
                    DebugMessages = log.DebugMessages.Length > 0 ? log.DebugMessages.ToArray() : null
                };
            }
        }
    }
}