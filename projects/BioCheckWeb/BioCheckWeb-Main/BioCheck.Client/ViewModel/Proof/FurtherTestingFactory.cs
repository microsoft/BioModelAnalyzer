using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using BioCheck.AnalysisService;
using BioCheck.Helpers;
using MvvmFx.Common.Helpers;

namespace BioCheck.ViewModel.Proof
{
    /// <summary>
    /// Static Factory class for converting a ModelViewModel to AnalysisInput
    /// </summary>
    public static class FurtherTestingFactory
    {
        /// <summary>
        /// Creates the AnalysisInput from the specified model VM.
        /// </summary>
        /// <param name="modelVM">The model VM.</param>
        /// <returns></returns>
        public static FurtherTestingOutput Create(FurtherTestingOutputDTO analysisOutputDto)
        {
            var output = new FurtherTestingOutput(analysisOutputDto);

            foreach (var cexOutputDTO in analysisOutputDto.CounterExamples)
            {
                var xml = ZipHelper.Unzip(cexOutputDTO.ZippedXml);
                var xdoc = XDocument.Parse(xml);

                var cexOutput = FromCounterExampleXml(xdoc);

                output.CounterExamples.Add(cexOutput);
            }

         
            return output;
        }

        public static CounterExampleOutput FromCounterExampleXml(XDocument xdoc)
        {
            var output = new CounterExampleOutput();
            output.Status = xdoc.Descendants("Status").FirstOrDefault().Value;

            if (output.Status == StatusTypes.Bifurcation)
            {
                output.CounterExampleType = CounterExampleTypes.Bifurcation;

                // Bifurcation
                // Get all the variable entries
                var allVariables = (from v in xdoc.Descendants("Variable")
                                    select new
                                    {
                                        Id = ParseId(v.Attribute("Id").Value),
                                        Value = v.AttributeInt("Value"),
                                    });

                // There will be multiple entries for bifurcated variables with the same id
                var uniqueVariables = new Dictionary<int, BifurcatingVariableOutput>();
                foreach (var vo in allVariables)
                {
                    BifurcatingVariableOutput bifurcatedVariable = null;
                    if (uniqueVariables.TryGetValue(vo.Id, out bifurcatedVariable))
                    {
                        // If a variable has multiple entries, and the Values are not the same, 
                        // it is Unstable and show as 2/3
                        bifurcatedVariable.Fix2 = vo.Value;
                    }
                    else
                    {
                        bifurcatedVariable = new BifurcatingVariableOutput();
                        bifurcatedVariable.Id = vo.Id;
                        bifurcatedVariable.Fix1 = vo.Value;
                        uniqueVariables.Add(vo.Id, bifurcatedVariable);
                    }
                }

                output.BifurcatingVariables = uniqueVariables.Values.ToList();
            }
            else if (output.Status == StatusTypes.Cycle)
            {
                output.CounterExampleType = CounterExampleTypes.Oscillation;

                // Cycled
                // Get all the variable entries
                var allVariables = (from v in xdoc.Descendants("Variable")
                                    select new
                                    {
                                        Id = ParseId(v.Attribute("Id").Value),
                                        Value = v.AttributeInt("Value"),
                                    });

                // There will be multiple entries for bifurcated variables with the same id
                var uniqueVariables = new Dictionary<int, OscillatingVariableOutput>();
                foreach (var vo in allVariables)
                {
                    int id = vo.Id;
                    OscillatingVariableOutput oscillatingVariable = null;
                    if (uniqueVariables.TryGetValue(id, out oscillatingVariable))
                    {
                        // If a variable has multiple entries, and the Values are not the same, 
                        // it is Unstable and show as 2, 3
                        oscillatingVariable.Oscillation = string.Format("{0}, {1}", oscillatingVariable.Oscillation, vo.Value);
                    }
                    else
                    {
                        oscillatingVariable = new OscillatingVariableOutput();
                        oscillatingVariable.Id = vo.Id;
                        oscillatingVariable.Oscillation = vo.Value.ToString();
                        uniqueVariables.Add(id, oscillatingVariable);
                    }
                }

                output.OscillatingVariables = uniqueVariables.Values.ToList();
            }
            else if (output.Status == StatusTypes.Fixpoint)
            {
                // TODO - should Fixpoint return as Oscillation?
                output.CounterExampleType = CounterExampleTypes.Oscillation;

                // FixPoint
                // Get all the variable entries
                var allVariables = (from v in xdoc.Descendants("Variable")
                                    select new
                                    {
                                        Id = ParseId(v.Attribute("Id").Value),
                                        Value = v.AttributeInt("Value"),
                                    });

                // There will be multiple entries for bifurcated variables with the same id
                var uniqueVariables = new Dictionary<int, OscillatingVariableOutput>();
                foreach (var vo in allVariables)
                {
                    int id = vo.Id;
                    OscillatingVariableOutput oscillatingVariable = null;
                    if (uniqueVariables.TryGetValue(id, out oscillatingVariable))
                    {
                        // If a variable has multiple entries, and the Values are not the same, 
                        // it is Unstable and show as 2, 3
                        oscillatingVariable.Oscillation = string.Format("{0}, {1}", oscillatingVariable.Oscillation, vo.Value);
                    }
                    else
                    {
                        oscillatingVariable = new OscillatingVariableOutput();
                        oscillatingVariable.Id = vo.Id;
                        oscillatingVariable.Oscillation = vo.Value.ToString();
                        uniqueVariables.Add(id, oscillatingVariable);
                    }
                }

                output.OscillatingVariables = uniqueVariables.Values.ToList();
            }
            else
            //if (output.Status == StatusTypes.Error ||
            // output.Status == StatusTypes.Default ||
            // output.Status == StatusTypes.TryingStabilizing ||
            // output.Status == StatusTypes.Unknown)
            {
                var error = xdoc.Descendants("Error").FirstOrDefault();
                output.Error = error != null ? error.AttributeString("Msg") : "There was an error in the analyzer";
            }

            return output;
        }

        private static int ParseId(string value)
        {
            int timeIndex = value.IndexOf('^');
            if (timeIndex > -1)
            {
                // The output is from Z3, which is in “variable-id ^ time" format. You can ignore the time after the “^". 
                string substring = value.Substring(0, timeIndex);
                return Int32.Parse(substring);
            }

            return Int32.Parse(value);
        }
    }
}