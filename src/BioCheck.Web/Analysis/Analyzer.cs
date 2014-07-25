using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using BioCheck.Web.Analysis.Xml;
using BioCheck.Web.Helpers;
using BioCheck.Web.Services;
using BioCheckAnalyzerCommon;

namespace BioCheck.Web.Analysis
{
    /// <summary>
    /// Static class to launch the analysis of the model
    /// </summary>
    public class Analyzer
    {
        public ValidationOutput IsValid(string formula)
        {
            //UIExpr.parse_result result = UIExpr.is_well_formed(formula);
            //bool isValid = result.IsParseOK;

            var output = new ValidationOutput();

            // Debug
            //output.IsValid = true;
            // return output;

            var result = UIExpr.check_syntax(formula);
            if (result.IsParseOK)
            {
                output.IsValid = true;
            }
            else if (result.IsParseErr)
            {
                UIExpr.perr perr = (result as UIExpr.parse_result.ParseErr).Item;

                output.IsValid = false;
                output.Line = perr.line;
                output.Column = perr.col;
                output.Details = perr.msg;

                // TODO - Samin: sometimes the lines break mid-way through a line, e.g. the var is put on another line.

                var msgs = new List<string>(perr.msg.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));

                string message = string.Empty;
                if (msgs.Count > 3)
                {
                    for (int i = 3; i < msgs.Count; i++)
                    {
                        var line = msgs[i];
                        if (!string.IsNullOrEmpty(line) && !line.StartsWith("Note"))
                        {
                            if (message != string.Empty)
                            {
                                message += " ";
                            }
                            message += line;
                        }
                    }
                    output.Message = message;
                }
                else if (msgs.Count > 0)
                {
                    output.Message = msgs[0];
                }
                else
                {
                    output.Message = "Unspecified validation error";
                }
            }

            return output;
        }

        /// <summary>
        /// Analyzes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns></returns>       
        public AnalysisOutputDTO Analyze(AnalysisInputDTO input)
        {
            
            // Convert to Input Xml
            var inputXml = XDocument.Parse(ZipHelper.Unzip(input.ZippedXml));

            // Check if the XML contains the LTL engine           
            string engineName = inputXml.Descendants("Engine").Elements("Name").First().Value;
            
            var log = new DefaultLogService();

            var azureLogService = new LogService();
            
            // SI: Refactor if-clauses into separate methods. 
            if (engineName == "VMCAI")
            {
                // Standard Proof
                try
                {
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
                        log.LogDebug("Enable Logging from the Run Proof button context menu to see more detailed logging info.");
                    }

                    var outputXml = analyzer.checkStability(inputXml);

                    // Log the output XML each time it's run
                    // DEBUG: Sam - to check why the output is returning is null
                    //azureLogService.Debug("Analyze Output XML", outputXml.ToString());

                    var time = Math.Round((DateTime.Now - analyisStartTime).TotalSeconds, 1);
                    log.LogDebug(string.Format("Analyzer took {0} seconds to run.", time));

                    // Convert to the Output Data
                    var outputData = new AnalysisOutputDTO();
                    outputData.Status = outputXml.Descendants("Status").FirstOrDefault().Value;
                    if (outputData.Status != StatusTypes.Stabilizing && outputData.Status != StatusTypes.NotStabilizing)
                    {
                        var error = outputXml.Descendants("Error").FirstOrDefault();
                        outputData.Error = error != null ? error.AttributeString("Msg") : "There was an error in the analyzer";
                    }

                    outputData.Time = time;
                    outputData.ErrorMessages = log.ErrorMessages;
                    outputData.ZippedXml = ZipHelper.Zip(outputXml.ToString());
                    outputData.ZippedLog = ZipHelper.Zip(string.Join(Environment.NewLine, log.DebugMessages));

                    return outputData;
                }
                catch (Exception ex)
                {
                    azureLogService.Debug("Analyze Exception", ex.ToString());

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

                    var outputXml = analyzer.checkLTL(inputXml,formula,num_of_steps);  

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
            // Normal proof, LTL or SYN
        }

        public SimulationOutputDTO Simulate(SimulationInputDTO input)
        {
            // Convert to Input Xml
            var inputXml = XDocument.Parse(ZipHelper.Unzip(input.ZippedXml));

            var log = new DefaultLogService();

            try
            {
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
                    log.LogDebug("Enable Logging from the Run Proof button context menu to see more detailed logging info.");
                }

                var inputDictionary = new Dictionary<int, int>();

                foreach (var variable in input.Variables)
                {
                    inputDictionary.Add(variable.Id, variable.Value);
                }

                var outputDictionary = analyzer.simulate_tick(inputXml, inputDictionary);

                // Convert to the Output Data
                var outputData = new SimulationOutputDTO();
                outputData.ErrorMessages = log.ErrorMessages;
                outputData.ZippedLog = ZipHelper.Zip(string.Join(Environment.NewLine, log.DebugMessages));

                outputData.Variables = new List<SimVariableDTO>();
                foreach (var item in outputDictionary)
                {
                    outputData.Variables.Add(new SimVariableDTO { Id = item.Key, Value = item.Value });
                }

                return outputData;
            }
            catch (Exception ex)
            {
                // Return an error message if fails
                var outputData = new SimulationOutputDTO
                                     {
                                         Error = ex.ToString(),
                                         ErrorMessages = log.ErrorMessages,
                                         ZippedLog = ZipHelper.Zip(string.Join(Environment.NewLine, log.DebugMessages))
                                     };
                return outputData;
            }
        }

        /// <summary>
        /// Analyzes the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="output">The output from the Analyzer.</param>
        /// <returns></returns>
        public FurtherTestingOutputDTO FindCounterExamples(AnalysisInputDTO input, AnalysisOutputDTO output)
        {
            // Get the input and output XML
            var inputXml = XDocument.Parse(ZipHelper.Unzip(input.ZippedXml));
            var outputXml = XDocument.Parse(ZipHelper.Unzip(output.ZippedXml));

            var log = new DefaultLogService();

            try
            {
                IAnalyzer2 analyzer = new UIMain.Analyzer2();

                var analyisStartTime = DateTime.Now;

                // Call the Analyzer and get the Counter Examples Xml
                if (input.EnableLogging)
                {
                    analyzer.LoggingOn(log);
                }
                else
                {
                    analyzer.LoggingOff();
                    log.LogDebug("Enable Logging from the Run Proof button context menu to see more detailed logging info.");
                }

                var cexBifurcatesXml = analyzer.findCExBifurcates(inputXml, outputXml);
                var cexCyclesXml = analyzer.findCExCycles(inputXml, outputXml);

                log.LogDebug(string.Format("Finding Counter Examples took {0} seconds to run.", (DateTime.Now - analyisStartTime).TotalSeconds));

                var furtherTestingOutput = new FurtherTestingOutputDTO();

                // Convert to the Output Data
                if (cexBifurcatesXml != null)
                {
                    var counterExampleOutput = new CounterExampleOutput();
                    counterExampleOutput.Status = cexBifurcatesXml.Descendants("Status").FirstOrDefault().Value;
                    if (counterExampleOutput.Status != StatusTypes.Stabilizing && counterExampleOutput.Status != StatusTypes.NotStabilizing)
                    {
                        var error = cexBifurcatesXml.Descendants("Error").FirstOrDefault();
                        counterExampleOutput.Error = error != null ? error.AttributeString("Msg") : "There was an error in the analyzer";
                    }

                    counterExampleOutput.ZippedXml = ZipHelper.Zip(cexBifurcatesXml.ToString());
                    furtherTestingOutput.CounterExamples.Add(counterExampleOutput);
                }

                if (cexCyclesXml != null)
                {
                    var counterExampleOutput = new CounterExampleOutput();
                    counterExampleOutput.Status = cexCyclesXml.Descendants("Status").FirstOrDefault().Value;
                    if (counterExampleOutput.Status != StatusTypes.Stabilizing && counterExampleOutput.Status != StatusTypes.NotStabilizing)
                    {
                        var error = cexCyclesXml.Descendants("Error").FirstOrDefault();
                        counterExampleOutput.Error = error != null ? error.AttributeString("Msg") : "There was an error in the analyzer";
                    }

                    counterExampleOutput.ZippedXml = ZipHelper.Zip(cexCyclesXml.ToString());
                    furtherTestingOutput.CounterExamples.Add(counterExampleOutput);
                }

                furtherTestingOutput.ErrorMessages = log.ErrorMessages;
                furtherTestingOutput.ZippedLog = ZipHelper.Zip(string.Join(Environment.NewLine, log.DebugMessages));

                return furtherTestingOutput;
            }
            catch (Exception ex)
            {
                var furtherTestingOutput = new FurtherTestingOutputDTO
                                     {
                                         Error = ex.ToString(),
                                         ErrorMessages = log.ErrorMessages,
                                         ZippedLog = ZipHelper.Zip(string.Join(Environment.NewLine, log.DebugMessages))
                                     };

                return furtherTestingOutput;
            }
        }
    }
}
