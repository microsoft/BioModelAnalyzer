using BioCheckAnalyzerCommon;
using bma.client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Linq;
using BioModelAnalyzer;

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
    }

    public class FurtherTestingController : ApiController
    {
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
                model.Preprocess();

                // SI: these all return a single CEx, not an array of them. 
                var cexBifurcates = analyzer.findCExBifurcates(model, input.Analysis);
                var cexCycles = analyzer.findCExCycles(model, input.Analysis);
                var cexFixPoints = analyzer.findCExFixpoint(model, input.Analysis);

                log.LogDebug(string.Format("Finding Counter Examples took {0} seconds to run.", (DateTime.Now - analyisStartTime).TotalSeconds));

                return new FurtherTestingOutput 
                {
                    CounterExamples = new CounterExampleOutput[] { cexBifurcates, cexCycles, cexFixPoints }, 
                    ErrorMessages = log.ErrorMessages.Count > 0 ? log.ErrorMessages.ToArray() : null,
                    DebugMessages = log.DebugMessages.Count > 0 ? log.DebugMessages.ToArray() : null
                };
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                return new FurtherTestingOutput
                {
                    ErrorMessages = log.ErrorMessages.Count > 0 ? log.ErrorMessages.ToArray() : null,
                    DebugMessages = log.DebugMessages.Count > 0 ? log.DebugMessages.ToArray() : null
                };
            }
        }
    }
}