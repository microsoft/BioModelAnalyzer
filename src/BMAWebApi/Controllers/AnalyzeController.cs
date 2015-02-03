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
    public class AnalysisOutput : AnalysisResult
    {
        public int Time { get; set; }

        public string[] ErrorMessages { get; set; }

        public string[] DebugMessages { get; set; }
    }

    public class AnalysisInput : Model
    {
        [XmlIgnore]
        public bool EnableLogging { get; set; }
    }

    public class AnalyzeController : ApiController
    {
        // POST api/Analyze
        public AnalysisOutput Post([FromBody]AnalysisInput input)
        {
            var log = new DefaultLogService();

            FailureAzureLogger faultLogger = new FailureAzureLogger(
                   CloudStorageAccount.Parse(
                       RoleEnvironment.GetConfigurationSettingValue("StorageConnectionString")));

            // Standard Proof
            try
            {
                IAnalyzer analyzer = new UIMain.Analyzer();
                var analyisStartTime = DateTime.Now;

                if (input.EnableLogging)
                {
                    analyzer.LoggingOn(log);
                }
                else
                {
                    analyzer.LoggingOff();
                }

                var model = (Model)input;
                var result = analyzer.checkStability(model);

                var time = Math.Round((DateTime.Now - analyisStartTime).TotalSeconds, 1);
                log.LogDebug(string.Format("Analyzer took {0} seconds to run.", time));

                if (result.Status != StatusType.Stabilizing && result.Status != StatusType.NotStabilizing) {
                    log.LogError(result.Error);
                    faultLogger.Add(DateTime.Now, "2.0", input, log);
                }

                return new AnalysisOutput 
                {
                    Error = result.Error,
                    Ticks = result.Ticks,
                    Status = result.Status,
                    Time = (int)time,
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
                return new AnalysisOutput
                {
                    Status = StatusType.Error,
                    ErrorMessages = log.ErrorMessages.Length > 0 ? log.ErrorMessages.ToArray() : null,
                    DebugMessages = log.DebugMessages.Length > 0 ? log.DebugMessages.ToArray() : null
                };
            }
        }            
            /*
            else if (engineName == "CAV")
            {
                // LTL Proof
                try
                {
                    string formula = inputXml.Descendants("Engine").Elements("Formula").First().Value;
                    string num_of_steps = inputXml.Descendants("Engine").Elements("Number_of_steps").First().Value;

                    IAnalyzer2 analyzer = new UIMain.Analyzer2();

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

                    var outputXml = analyzer.checkLTL(inputXml, formula, num_of_steps);

                    // Log the output XML each time it's run
                    // DEBUG: Sam - to check why the output is returning is null
                    //azureLogService.Debug("Analyze Output XML", outputXml.ToString());

                    var time = Math.Round((DateTime.Now - analyisStartTime).TotalSeconds, 1);
                    log.LogDebug(string.Format("The LTL proof took {0} seconds to run.", time));

                    // Convert to the Output Data
                    var outputData = new AnalysisOutputDTO();
                    //outputData.Status = outputXml.Descendants("Status").FirstOrDefault().Value;    // <-- Change (unless contained and of use)
                    //if (outputData.Status != StatusTypes.Stabilizing && outputData.Status != StatusTypes.NotStabilizing)
                    //{
                    //    var error = outputXml.Descendants("Error").FirstOrDefault();
                    //    outputData.Error = error != null ? error.AttributeString("Msg") : "There was an error during the LTL analysis";
                    //}

                    outputData.Time = time;
                    outputData.ErrorMessages = log.ErrorMessages;
                    outputData.ZippedXml = ZipHelper.Zip(outputXml.ToString());
                    outputData.ZippedLog = ZipHelper.Zip(string.Join(Environment.NewLine, log.DebugMessages));

                    return outputData;
                }
                catch (Exception ex)
                {
                    azureLogService.Debug("LTL Exception", ex.ToString());

                    // Return an Unknown if fails
                    var outputData = new AnalysisOutputDTO
                    {
                        Status = StatusTypes.Unknown,
                        Error = ex.ToString(),
                        ErrorMessages = log.ErrorMessages,
                        ZippedLog = ZipHelper.Zip(string.Join(Environment.NewLine, log.DebugMessages))
                    };
                    return outputData;
                }
            }
            else
            {
                // if (engineName == "SYN")
                try
                {
                    IAnalyzer2 analyzer = new UIMain.Analyzer2();                   // Needs changing to the SYN engine.

                    var analyisStartTime = DateTime.Now;

                    // Call the Analyzer and get the Output Xml
                    if (input.EnableLogging)
                    {
                        analyzer.LoggingOn(log);
                    }
                    else
                    {
                        analyzer.LoggingOff();
                        log.LogDebug("Enable Logging from the context menu to see more detailed logging info.");
                    }

                    var outputXml = analyzer.checkSynth(inputXml);                        // analyzer (set above)

                    var time = Math.Round((DateTime.Now - analyisStartTime).TotalSeconds, 1);
                    log.LogDebug(string.Format("Synthesis took {0} seconds to run.", time));

                    // Convert to the Output Data
                    var outputData = new AnalysisOutputDTO();

                    outputData.Time = time;
                    outputData.ErrorMessages = log.ErrorMessages;

                    //outputData.ZippedXml = ZipHelper.Zip(inputXml.ToString());                      // Just sending back the input XML at the moment.
                    outputData.ZippedXml = ZipHelper.Zip(outputXml.ToString());
                    outputData.ZippedLog = ZipHelper.Zip(string.Join(Environment.NewLine, log.DebugMessages));  // The log!

                    return outputData;
                }
                catch (Exception ex)
                {
                    azureLogService.Debug("SYN Exception", ex.ToString());

                    // Return an Unknown if fails
                    var outputData = new AnalysisOutputDTO
                    {
                        Status = StatusTypes.Unknown,
                        Error = ex.ToString(),
                        ErrorMessages = log.ErrorMessages,
                        ZippedLog = ZipHelper.Zip(string.Join(Environment.NewLine, log.DebugMessages))
                    };
                    return outputData;
                }
            }*/
    }
}