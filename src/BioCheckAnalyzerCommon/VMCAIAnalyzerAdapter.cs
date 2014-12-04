using BioCheckAnalyzerCommon;
using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Linq;
using System.Collections.Generic;

namespace BioModelAnalyzer
{
    public class VMCAIAnalyzerAdapter : IVMCAIAnalyzer
    {
        private readonly IAnalyzer2 analyzer2;

        public VMCAIAnalyzerAdapter(IAnalyzer2 analyzer2)
        {
            this.analyzer2 = analyzer2;
        }

        public AnalysisResult CheckStability(Model model, ILogService logger)
        {
            // Convert to input data
            model.Preprocess();

            var overrides = new XmlAttributeOverrides();
            overrides.Add(model.GetType(), new XmlAttributes { XmlRoot = new XmlRootAttribute("AnalysisInput") });
            var xmlSerializer = new XmlSerializer(model.GetType(), overrides);
            var stream = new MemoryStream();
            xmlSerializer.Serialize(stream, (Model)model);
            stream.Position = 0;
            var inputXml = XDocument.Load(stream);

            if (logger != null)
            {
                analyzer2.LoggingOn(logger);
            }
            else
            {
                analyzer2.LoggingOff();
            }

            // Check stability
            var outputXml = analyzer2.checkStability(inputXml);

            // Convert to the Output Data
            var outputStream = new MemoryStream();
            outputXml.Save(outputStream);
            outputStream.Position = 0;
            overrides.Add(typeof(AnalysisResult), 
                            new XmlAttributes { XmlRoot = new XmlRootAttribute("AnalysisOutput") });
            XmlSerializer outputSerializer = new XmlSerializer(typeof(AnalysisResult), overrides);
            var output = (AnalysisResult)outputSerializer.Deserialize(outputStream);
            if (output.Status != StatusType.Stabilizing && output.Status != StatusType.NotStabilizing)
            {
                var error = outputXml.Descendants("Error").FirstOrDefault();
                var errorMsg = error != null ? error.Attribute(XName.Get("Msg")).Value : "There was an error in the analyzer";
                if(logger != null)
                    logger.LogError(errorMsg);
            }
            return output;
        }

        public CycleCounterExample[] FindCycleCex(Model model, AnalysisResult result, ILogService logger)
        {
            model.Preprocess();

            var overrides = new XmlAttributeOverrides();
            overrides.Add(model.GetType(), new XmlAttributes { XmlRoot = new XmlRootAttribute("AnalysisInput") });
            overrides.Add(result.GetType(), new XmlAttributes { XmlRoot = new XmlRootAttribute("AnalysisOutput") });

            var xmlSerializer = new XmlSerializer(model.GetType(), overrides);
            var stream = new MemoryStream();
            xmlSerializer.Serialize(stream, model);
            stream.Position = 0;
            var inputXml = XDocument.Load(stream);

            var xmlSerializer2 = new XmlSerializer(result.GetType(), overrides);
            var stream2 = new MemoryStream();
            xmlSerializer2.Serialize(stream2, result);
            stream2.Position = 0;
            var outputXml = XDocument.Load(stream2);

            if (logger != null)
                analyzer2.LoggingOn(logger);
            else
                analyzer2.LoggingOff();

            var cexCyclesXml = analyzer2.findCExCycles(inputXml, outputXml);

            List<CycleCounterExample> counterExamples = new List<CycleCounterExample>();

            if (cexCyclesXml != null)
            {
                XmlSerializer cyclesSerializer = new XmlSerializer(typeof(CounterExampleOutputXML));

                var counterExampleOutput = (CounterExampleOutputXML)cyclesSerializer.Deserialize(new StringReader("<?xml version=\"1.0\"?>" + cexCyclesXml.ToString()));
                if (counterExampleOutput.Status != CounterExampleType.Cycle)
                {
                    var error = cexCyclesXml.Descendants("Error").FirstOrDefault();
                    counterExampleOutput.Error = error != null ? error.Attribute("Msg").Value : "There was an error in the analyzer";
                }

                counterExamples.Add(new CycleCounterExample
                {
                    Status = CounterExampleType.Cycle,
                    Error = counterExampleOutput.Error,
                    Variables = counterExampleOutput.Variables[0].Variables.Select(v => new CycleCounterExample.Variable
                    {
                        Id = v.Id,
                        Value = v.Value
                    }).ToArray()
                });
            }

            return counterExamples.ToArray();
        }

        public BifurcationCounterExample[] FindBifurcationCex(Model model, AnalysisResult result, ILogService logger)
        {
            model.Preprocess();

            var overrides = new XmlAttributeOverrides();
            overrides.Add(model.GetType(), new XmlAttributes { XmlRoot = new XmlRootAttribute("AnalysisInput") });
            overrides.Add(result.GetType(), new XmlAttributes { XmlRoot = new XmlRootAttribute("AnalysisOutput") });
            
            var xmlSerializer = new XmlSerializer(model.GetType(), overrides);
            var stream = new MemoryStream();
            xmlSerializer.Serialize(stream, model);
            stream.Position = 0;
            var inputXml = XDocument.Load(stream);

            var xmlSerializer2 = new XmlSerializer(result.GetType(), overrides);
            var stream2 = new MemoryStream();
            xmlSerializer2.Serialize(stream2, result);
            stream2.Position = 0;
            var outputXml = XDocument.Load(stream2);

            if (logger != null)
                analyzer2.LoggingOn(logger);
            else
                analyzer2.LoggingOff();

            var cexBifurcatesXml = analyzer2.findCExBifurcates(inputXml, outputXml);

            List<BifurcationCounterExample> counterExamples = new List<BifurcationCounterExample>();

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

            return counterExamples.ToArray();                             
        }

        public FixPointCounterExample[] FindFixPointCex(Model model, AnalysisResult result, ILogService logger)
        {
            model.Preprocess();

            var overrides = new XmlAttributeOverrides();
            overrides.Add(model.GetType(), new XmlAttributes { XmlRoot = new XmlRootAttribute("AnalysisInput") });
            overrides.Add(result.GetType(), new XmlAttributes { XmlRoot = new XmlRootAttribute("AnalysisOutput") });

            var xmlSerializer = new XmlSerializer(model.GetType(), overrides);
            var stream = new MemoryStream();
            xmlSerializer.Serialize(stream, model);
            stream.Position = 0;
            var inputXml = XDocument.Load(stream);

            var xmlSerializer2 = new XmlSerializer(result.GetType(), overrides);
            var stream2 = new MemoryStream();
            xmlSerializer2.Serialize(stream2, result);
            stream2.Position = 0;
            var outputXml = XDocument.Load(stream2);

            if (logger != null)
                analyzer2.LoggingOn(logger);
            else
                analyzer2.LoggingOff();

            var cexFixPointsXml = analyzer2.findCExFixpoint(inputXml, outputXml);

            List<FixPointCounterExample> counterExamples = new List<FixPointCounterExample>();

            if (cexFixPointsXml != null)
            {
                XmlSerializer cyclesSerializer = new XmlSerializer(typeof(CounterExampleOutputXML));

                var counterExampleOutput = (CounterExampleOutputXML)cyclesSerializer.Deserialize(new StringReader("<?xml version=\"1.0\"?>" + cexFixPointsXml.ToString()));
                if (counterExampleOutput.Status != CounterExampleType.Fixpoint)
                {
                    var error = cexFixPointsXml.Descendants("Error").FirstOrDefault();
                    counterExampleOutput.Error = error != null ? error.Attribute("Msg").Value : "There was an error in the analyzer";
                }

                //counterExampleOutput.ZippedXml = ZipHelper.Zip(cexCyclesXml.ToString());
                counterExamples.Add(new FixPointCounterExample
                {
                    Status = CounterExampleType.Fixpoint,
                    Error = counterExampleOutput.Error,
                    Variables = counterExampleOutput.Variables[0].Variables.Select(v => new FixPointCounterExample.Variable
                    {
                        Id = v.Id,
                        Value = v.Value
                    }).ToArray()
                });
            }

            return counterExamples.ToArray();
        }

        public SimulationVariable[] Simulate(Model model, SimulationVariable[] state, ILogService logger)
        {
            model.Preprocess();

            var overrides = new XmlAttributeOverrides();
            overrides.Add(model.GetType(), new XmlAttributes { XmlRoot = new XmlRootAttribute("AnalysisInput") });

            var xmlSerializer = new XmlSerializer(model.GetType(), overrides);
            var stream = new MemoryStream();
            xmlSerializer.Serialize(stream, model);
            stream.Position = 0;
            var inputXml = XDocument.Load(stream);

            if (logger != null)
                analyzer2.LoggingOn(logger);
            else
                analyzer2.LoggingOff();

            var inputDictionary = new Dictionary<int, int>();
            foreach (var variable in state)
            {
                inputDictionary.Add(variable.Id, (int)variable.Value);
            }

            var outputDictionary = analyzer2.simulate_tick(inputXml, inputDictionary);

            return outputDictionary.Select(pair => new SimulationVariable
            {
                Id = pair.Key,
                Value = pair.Value
            }).ToArray();
        }
    }

    [XmlRoot(ElementName = "AnalysisOutput")]
    public class CounterExampleOutputXML : CounterExampleOutput
    {
        [XmlElement("Variables", Type = typeof(CounterExampleVariables))]
        public CounterExampleVariables[] Variables { get; set; }

    }
}