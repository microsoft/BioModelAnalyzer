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
                IVMCAIAnalyzer analyzer = new VMCAIAnalyzerAdapter(new UIMain.Analyzer2());

                var analyisStartTime = DateTime.Now;

                if(!input.EnableLogging)
                    log.LogDebug("Enable Logging from the Run Proof button context menu to see more detailed logging info.");

                var cexBifurcates = analyzer.FindBifurcationCex(input.Model, input.Analysis, input.EnableLogging ? log : null);
                var cexCycles = analyzer.FindCycleCex(input.Model, input.Analysis, input.EnableLogging ? log : null);
                var cexFixPoints = analyzer.FindFixPointCex(input.Model, input.Analysis, input.EnableLogging ? log : null);

                log.LogDebug(string.Format("Finding Counter Examples took {0} seconds to run.", (DateTime.Now - analyisStartTime).TotalSeconds));

                return new FurtherTestingOutput 
                {
                    CounterExamples = cexBifurcates.Cast<CounterExampleOutput>().
                                                    Concat(cexCycles.Cast<CounterExampleOutput>()).
                                                    Concat(cexFixPoints.Cast<CounterExampleOutput>()).ToArray(),
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