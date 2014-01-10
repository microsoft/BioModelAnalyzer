using System;
using System.Linq;
using System.Xml.Linq;
using BioCheck.Web.Analysis.Xml;
using System.Collections.Generic;

namespace BioCheck.Web.Analysis
{
    public static class AnalysisXmlConverter
    {
        ///// <summary>
        ///// Converts the analysis output XML to an AnalysisOutput object.
        ///// </summary>
        ///// <param name="xdoc">The xdoc.</param>
        ///// <returns></returns>
        //public static AnalysisOutput FromAnalysisOutputXml(XDocument xdoc)
        //{
        //    var output = new AnalysisOutput();

        //    output.Status = xdoc.Descendants("Status").FirstOrDefault().Value;

        //    if (output.Status == StatusTypes.Stabilizing)
        //    {
        //        // Stabilizing
        //        // All the variables are stable
        //        // The value returned with each variable is the Value and should be displayed

        //        output.Ticks = (from t in xdoc.Descendants("Tick")
        //                        select new AnalysisTick
        //                        {
        //                            Time = t.ElementInt("Time"),
        //                            Variables = (from v in t.Descendants("Variable")
        //                                         let lo = v.AttributeInt("Lo")
        //                                         let hi = v.AttributeInt("Hi")
        //                                         select new VariableOutput
        //                                                    {
        //                                                        Id = ParseId(v.AttributeString("Id")),
        //                                                        Low = lo,
        //                                                        High = hi,
        //                                                        IsStable = lo == hi
        //                                                    }).ToList()
        //                        })
        //                        .OrderBy(t => t.Time).ToList();
        //    }
        //    else if (output.Status == StatusTypes.NotStabilizing)
        //    {
        //        output.Ticks = (from t in xdoc.Descendants("Tick")
        //                        select new AnalysisTick
        //                                   {
        //                                       Time = t.ElementInt("Time"),
        //                                       Variables = (from v in t.Descendants("Variable")
        //                                                    let lo = v.AttributeInt("Lo")
        //                                                    let hi = v.AttributeInt("Hi")
        //                                                    select new VariableOutput
        //                                                               {
        //                                                                   Id = ParseId(v.AttributeString("Id")),
        //                                                                   Low = lo,
        //                                                                   High = hi,
        //                                                                   IsStable = (lo == hi)
        //                                                               }).ToList()
        //                                   })
        //            .OrderBy(t => t.Time).ToList();
        //    }
        //    else
        //    //if (output.Status == StatusTypes.Error ||
        //    // output.Status == StatusTypes.Default ||
        //    // output.Status == StatusTypes.TryingStabilizing ||
        //    // output.Status == StatusTypes.Unknown)
        //    {
        //        var error = xdoc.Descendants("Error").FirstOrDefault();
        //        output.Error = error != null ? error.AttributeString("Msg") : "There was an error in the analyzer";
        //    }

        //    return output;
        //}

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

        /// <summary>
        /// Converts the analysis input to XML.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        public static XDocument ToAnalysisInputXml(AnalysisInput data)
        {
            var xdoc = new XDocument(
                new XElement("AnalysisInput",
                             new XAttribute("ModelName", data.ModelName),
                             new XElement("Variables",
                                          from v in data.Variables
                                          select new XElement("Variable",
                                                              new XAttribute("Id", v.Id),
                                                              new XElement("Name", v.Name),
                                                              new XElement("RangeFrom", v.RangeFrom),
                                                              new XElement("RangeTo", v.RangeTo),
                                                              new XElement("Function", v.Formula))),
                            new XElement("Relationships",
                                          from r in data.Relationships
                                          select new XElement("Relationship",
                                                              new XAttribute("Id", r.Id),
                                                              new XElement("FromVariableId", r.FromVariableId),
                                                              new XElement("ToVariableId", r.ToVariableId),
                                                              new XElement("Type", r.Type)))));
            // Debug...
            // Save the document
            //var path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //xdoc.Save(path + "\\AnalysisInput.xml");

            return xdoc;
        }
    }
}