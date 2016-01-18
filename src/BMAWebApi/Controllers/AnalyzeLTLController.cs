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
    public class AnalysisOutputDTO : AnalysisResult
    {
        public int Time { get; set; }

        public string[] ErrorMessages { get; set; }

        public string[] DebugMessages { get; set; }

        public int Loop { get; set; }
    }

    public class LTLAnalysisOutputDTO : AnalysisOutputDTO
    {
        [XmlElement("Tick", Type = typeof(Tick))]
        public Tick[] NegTicks { get; set; }
    }

    public class AnalysisInputDTO : Model
    {
        [XmlIgnore]
        public bool EnableLogging { get; set; }
        public string Formula { get; set; }

        public string Number_of_steps { get; set; }

    }

    public class AnalyzeLTLController : ApiController
    {
        private readonly IFailureLogger faultLogger;

        public AnalyzeLTLController(IFailureLogger logger)
        {
            this.faultLogger = logger;
        }

        // POST api/AnalyzeLTL
        public AnalysisOutputDTO Post([FromBody]AnalysisInputDTO input)
        {

            var log = new DefaultLogService();
            // LTL Proof
            try
            {
                string formula = input.Formula;
                string num_of_steps = input.Number_of_steps;

                IAnalyzer analyzer = new UIMain.Analyzer();

                var analyisStartTime = DateTime.Now;

                // Call the Analyzer and get the Output Xml
                if (input.EnableLogging)
                {
                    analyzer.LoggingOn(log);
                }
                else
                {
                    analyzer.LoggingOff();
                    log.LogDebug("Enable Logging from the Run LTL Proof button context menu to see more detailed logging info.");
                }

                var model = (Model)input;
                //var result = analyzer.checkLTL((Model)input, formula, num_of_steps); 
                var result = Utilities.RunWithTimeLimit(() => analyzer.checkLTL((Model)input, formula, num_of_steps), TimeSpan.FromMinutes(1));//, Utilities.GetTimeLimitFromConfig());

                // Log the output XML each time it's run
                // DEBUG: Sam - to check why the output is returning is null
                //azureLogService.Debug("Analyze Output XML", outputXml.ToString());

                var time = (int)Math.Round((DateTime.Now - analyisStartTime).TotalSeconds, 1);
                log.LogDebug(string.Format("The LTL proof took {0} seconds to run.", time));

                // Convert to the Output Data
                //outputData.Status = outputXml.Descendants("Status").FirstOrDefault().Value;    // <-- Change (unless contained and of use)
                //if (outputData.Status != StatusTypes.Stabilizing && outputData.Status != StatusTypes.NotStabilizing)
                //{
                //    var error = outputXml.Descendants("Error").FirstOrDefault();
                //    outputData.Error = error != null ? error.AttributeString("Msg") : "There was an error during the LTL analysis";
                //}

                var status = result.Status;
                if (result.Status == StatusType.True && result.NegStatus == StatusType.True)
                    status = StatusType.PartiallyTrue;

                return new LTLAnalysisOutputDTO
                {
                    Error = result.Error,
                    Ticks = result.Ticks,
                    NegTicks = result.NegTicks,
                    Status = status,
                    Time = (int)time,
                    Loop = result.Loop,
                    ErrorMessages = log.ErrorMessages.Length > 0 ? log.ErrorMessages.ToArray() : null,
                    DebugMessages = log.DebugMessages.Length > 0 ? log.DebugMessages.ToArray() : null
                    //outputData.ZippedXml = ZipHelper.Zip(outputXml.ToString());
                    //outputData.ZippedLog = ZipHelper.Zip(string.Join(Environment.NewLine, log.DebugMessages));
                };
            }
            catch (Exception ex)
            {
                //  azureLogService.Debug("Analyze Exception", ex.ToString());

                log.LogError(ex.ToString());
                faultLogger.Add(DateTime.Now, "2.0", input, log);
                // Return an Unknown if fails
                return new AnalysisOutputDTO
                {
                    Status = StatusType.Error,
                    Error = ex.Message,
                    ErrorMessages = log.ErrorMessages.Length > 0 ? log.ErrorMessages.ToArray() : null,
                    DebugMessages = log.DebugMessages.Length > 0 ? log.DebugMessages.ToArray() : null
                };
            }
        }
    }

}