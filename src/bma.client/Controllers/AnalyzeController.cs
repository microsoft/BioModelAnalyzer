using BioCheckAnalyzerCommon;
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
    public class AnalyzeController : ApiController
    {
        // POST api/Analyze
        public AnalysisOutput Post([FromBody]AnalysisInput input)
        {
            input.ReplaceVariableNamesWithIDs();

            var xmlSerializer = new XmlSerializer(typeof(AnalysisInput));
            var stream = new MemoryStream();
            xmlSerializer.Serialize(stream, input);
            stream.Position = 0;
            var inputXml = XDocument.Load(stream);

            string engineName = input.Engine;

            //var log = new DefaultLogService();

            // var azureLogService = new LogService();

            // SI: Refactor if-clauses into separate methods. 
            if (engineName == "VMCAI")
            {
                // Standard Proof
                try
                {
                    IAnalyzer2 analyzer = new UIMain.Analyzer2();

                    var analyisStartTime = DateTime.Now;

                    // Call the Analyzer and get the Output Xml
                    // if (input.EnableLogging)
                    // {
                    //     analyzer.LoggingOn(log);
                    // }
                    // else
                    // {
                    analyzer.LoggingOff();
                    //    log.LogDebug("Enable Logging from the Run Proof button context menu to see more detailed logging info.");
                    // }

                    var outputXml = analyzer.checkStability(inputXml);

                    // Log the output XML each time it's run
                    // DEBUG: Sam - to check why the output is returning is null
                    //azureLogService.Debug("Analyze Output XML", outputXml.ToString());

                    var time = Math.Round((DateTime.Now - analyisStartTime).TotalSeconds, 1);
                    //log.LogDebug(string.Format("Analyzer took {0} seconds to run.", time));

                    // Convert to the Output Data
                    var outputStream = new MemoryStream();
                    outputXml.Save(outputStream);
                    outputStream.Position = 0;
                    XmlSerializer outputSerializer = new XmlSerializer(typeof(AnalysisOutput));
                    var output = (AnalysisOutput)outputSerializer.Deserialize(outputStream);
                    //if (output.Status != StatusType.Stabilizing && output.Status != StatusType.NotStabilizing)
                    //{
                    //    var error = outputXml.Descendants("Error").FirstOrDefault();
                    //    output.Error = error != null ? error.AttributeString("Msg") : "There was an error in the analyzer";
                    //}
                    output.Time = (int)time;
                    //outputData.ErrorMessages = log.ErrorMessages;
                    //outputData.ZippedXml = ZipHelper.Zip(outputXml.ToString());
                    //outputData.ZippedLog = ZipHelper.Zip(string.Join(Environment.NewLine, log.DebugMessages));

                    return output;
                }
                catch (Exception ex)
                {
                    //  azureLogService.Debug("Analyze Exception", ex.ToString());

                    // Return an Unknown if fails
                    var outputData = new AnalysisOutput
                    {
                        Status = StatusType.Unknown
                        /* StatusTypes.Unknown,
                        Error = ex.ToString(),
                        ErrorMessages = log.ErrorMessages,
                        ZippedLog = ZipHelper.Zip(string.Join(Environment.NewLine, log.DebugMessages))*/
                    };
                    return outputData;
                }
            }/*
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
            }
            // Normal proof, LTL or SYN*/
            return new AnalysisOutput() { Status = StatusType.Unknown };
        }
    }
}