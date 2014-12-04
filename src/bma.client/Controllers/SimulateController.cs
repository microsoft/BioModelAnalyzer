using BioCheckAnalyzerCommon;
using BioModelAnalyzer;
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
                IVMCAIAnalyzer analyzer = new VMCAIAnalyzerAdapter(new UIMain.Analyzer2());

                var analyisStartTime = DateTime.Now;

                if (!input.EnableLogging)
                    log.LogDebug("Enable Logging from the Run Proof button context menu to see more detailed logging info.");

                var inputDictionary = new Dictionary<int, int>();
                foreach (var variable in input.Variables)
                {
                    inputDictionary.Add(variable.Id, (int)variable.Value);
                }

                var output = analyzer.Simulate(input.Model, input.Variables, input.EnableLogging ? log : null);

                return new SimulationOutput
                {
                    Variables = output,
                    ErrorMessages = log.ErrorMessages.Count > 0 ? log.ErrorMessages.ToArray() : null,
                    DebugMessages = log.DebugMessages.Count > 0 ? log.DebugMessages.ToArray() : null,
                };

                //outputData.ErrorMessages = log.ErrorMessages;
                //outputData.ZippedLog = ZipHelper.Zip(string.Join(Environment.NewLine, log.DebugMessages));
            }
            catch (Exception ex)
            {
                log.LogError(ex.ToString());
                return new SimulationOutput
                {
                    ErrorMessages = log.ErrorMessages.Count > 0 ? log.ErrorMessages.ToArray() : null,
                    DebugMessages = log.DebugMessages.Count > 0 ? log.DebugMessages.ToArray() : null
                };
            }
        }
    }
}