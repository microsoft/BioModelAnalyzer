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
            input.Model.NullifyDefaultFunction();


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
                    XmlSerializer bifsSerializer = new XmlSerializer(typeof(CounterExampleOutputXML));
                    var counterExampleOutput = (CounterExampleOutputXML)bifsSerializer.Deserialize(new StringReader("<?xml version=\"1.0\"?>" + cexBifurcatesXml.ToString()));

                    if (counterExampleOutput.Status != CounterExampleType.Bifurcation)
                    {
                        var error = cexBifurcatesXml.Descendants("Error").FirstOrDefault();
                        counterExampleOutput.Error = error != null ? error.Attribute("Msg").Value : "There was an error in the analyzer";
                    }
                    //counterExampleOutput.ZippedXml = ZipHelper.Zip(cexBifurcatesXml.ToString());
                    
                    counterExamples.Add(new BifurcationCounterExample
                    {
                        Status = CounterExampleType.Bifurcation,
                        Error = counterExampleOutput.Error,
                        Variables = counterExampleOutput.Variables[0].Variables.Zip(counterExampleOutput.Variables[1].Variables, (v1, v2) => new BifurcationCounterExample.BifurcatingVariable
                        {
                            Id = v1.Id,
                            Fix1 = v1.Value,
                            Fix2 = v2.Value
                        }).ToArray()
                    });
                }

                if (cexCyclesXml != null)
                {
                    XmlSerializer cyclesSerializer = new XmlSerializer(typeof(CounterExampleOutputXML));

                    var counterExampleOutput = (CounterExampleOutputXML)cyclesSerializer.Deserialize(new StringReader("<?xml version=\"1.0\"?>" + cexCyclesXml.ToString()));
                    if (counterExampleOutput.Status != CounterExampleType.Cycle)
                    {
                        var error = cexCyclesXml.Descendants("Error").FirstOrDefault();
                        counterExampleOutput.Error = error != null ? error.Attribute("Msg").Value : "There was an error in the analyzer";
                    }

                    //counterExampleOutput.ZippedXml = ZipHelper.Zip(cexCyclesXml.ToString());
                    counterExamples.Add(new CycleCounterExample
                    {
                        Status = CounterExampleType.Cycle,
                        Error = counterExampleOutput.Error,
                        Variables = counterExampleOutput.Variables[0].Variables.Select(v => new CycleCounterExample.CycleVariable
                        {
                            Id = v.Id,
                            Value = v.Value
                        }).ToArray()
                    });
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