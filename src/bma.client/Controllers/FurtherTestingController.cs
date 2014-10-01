using BioCheckAnalyzerCommon;
using bmaclient;
using bma.client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Web.Http;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Linq;

namespace bma.client.Controllers
{
    public class FurtherTestingController : ApiController
    {
        // POST api/Validate
        public FurtherTestingOutput Post([FromBody]FurtherTestingInput input)
        {
            input.Model.ReplaceVariableNamesWithIDs();

            // Hack: create XDocuments to interop with old code
            var xmlSerializer = new XmlSerializer(typeof(AnalysisInput));
            var stream = new MemoryStream();
            xmlSerializer.Serialize(stream, input.Model);
            stream.Position = 0;
            var inputXml = XDocument.Load(stream);

            var xmlSerializer2 = new XmlSerializer(typeof(AnalysisOutput));
            var stream2 = new MemoryStream();
            xmlSerializer2.Serialize(stream2, input.Analysis);
            stream2.Position = 0;
            var outputXml = XDocument.Load(stream2);

            //var log = new DefaultLogService();

            try
            {
                IAnalyzer2 analyzer = new UIMain.Analyzer2();

                var analyisStartTime = DateTime.Now;

                // Call the Analyzer and get the Counter Examples Xml
                //if (input.EnableLogging)
                //{
                //    analyzer.LoggingOn(log);
                //}
                //else
                //{
                    analyzer.LoggingOff();
                //    log.LogDebug("Enable Logging from the Run Proof button context menu to see more detailed logging info.");
                //}

                var cexBifurcatesXml = analyzer.findCExBifurcates(inputXml, outputXml);
                var cexCyclesXml = analyzer.findCExCycles(inputXml, outputXml);

                //log.LogDebug(string.Format("Finding Counter Examples took {0} seconds to run.", (DateTime.Now - analyisStartTime).TotalSeconds));

                var furtherTestingOutput = new FurtherTestingOutput();
                List<CounterExampleOutput> counterExamples = new List<CounterExampleOutput>();

                // Convert to the Output Data
                if (cexBifurcatesXml != null)
                {
                    var counterExampleOutput = new CounterExampleOutput();
                    counterExampleOutput.Status = cexBifurcatesXml.Descendants("Status").FirstOrDefault().Value;
                    if (counterExampleOutput.Status != StatusType.Stabilizing.ToString() && counterExampleOutput.Status != StatusType.NotStabilizing.ToString())
                    {
                        var error = cexBifurcatesXml.Descendants("Error").FirstOrDefault();
                        counterExampleOutput.Error = error != null ? error.Attribute("Msg").Value : "There was an error in the analyzer";
                    }

                    //counterExampleOutput.ZippedXml = ZipHelper.Zip(cexBifurcatesXml.ToString());
                    counterExamples.Add(counterExampleOutput);
                }

                if (cexCyclesXml != null)
                {
                    var counterExampleOutput = new CounterExampleOutput();
                    counterExampleOutput.Status = cexCyclesXml.Descendants("Status").FirstOrDefault().Value;
                    if (counterExampleOutput.Status != StatusType.Stabilizing.ToString() && counterExampleOutput.Status != StatusType.NotStabilizing.ToString())
                    {
                        var error = cexCyclesXml.Descendants("Error").FirstOrDefault();
                        counterExampleOutput.Error = error != null ? error.Attribute("Msg").Value : "There was an error in the analyzer";
                    }

                    //counterExampleOutput.ZippedXml = ZipHelper.Zip(cexCyclesXml.ToString());
                    counterExamples.Add(counterExampleOutput);
                }

                furtherTestingOutput.CounterExamples = counterExamples.ToArray();
                furtherTestingOutput.ErrorMessages = new string[0]; //log.ErrorMessages;
                //furtherTestingOutput.ZippedLog = ZipHelper.Zip(string.Join(Environment.NewLine, log.DebugMessages));

                return furtherTestingOutput;
            }
            catch (Exception ex)
            {
                var furtherTestingOutput = new FurtherTestingOutput
                {
                    Error = ex.ToString(),
                    ErrorMessages = new string[0]//log.ErrorMessages,
                  //  ZippedLog = ZipHelper.Zip(string.Join(Environment.NewLine, log.DebugMessages))
                };

                return furtherTestingOutput;
            }
        }
    }
}