using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using BioCheck.AnalysisService;
using BioCheck.ViewModel.Factories;
using BioCheck.ViewModel.Models;

namespace BioCheck.ViewModel.Proof
{
    /// <summary>
    /// Static Factory class for converting a ModelViewModel to AnalysisInput
    /// </summary>
    public static class ProofOutputFactory
    {
        public static XDocument ToProofOutputXml(ProofViewModel proofVM)
        {
            var xdoc = new XDocument(
                new XElement("ProofOutput",
                             new XAttribute("ModelName", proofVM.ModelName),
                             new XElement("Status", proofVM.State),
                             new XElement("Time", proofVM.Time),
                             new XElement("Steps", proofVM.Steps),
                             new XElement("Variables",
                                          from v in proofVM.Variables
                                          select new XElement("Variable",
                                                              new XAttribute("Id", v.Id),
                                                              new XElement("Name", v.Name),
                                                              new XElement("Range", v.Range))),
                            new XElement("ProofProgressions",
                                          from pp in proofVM.ProgressionInfos
                                          select new XElement("ProofProgression",
                                                              new XAttribute("Id", pp.Id),
                                                              new XElement("Name", pp.Name),
                                                              new XElement("Steps",
                                                                   from s in pp.Steps
                                                                   select new XElement("Step",
                                                                                       new XElement("Name", s.Name),
                                                                                       new XElement("IsStable", s.IsStable),
                                                                                       new XElement("Values", s.Values)))
                                                              ))));

            return xdoc;
        }

        public static XDocument ToFurtherTestingOutputXml(ProofViewModel proofVM)
        {
            // TODO - tidy

            //var xdoc = new XDocument(
            //    new XElement("FurtherTestingOutput",
            //                 new XAttribute("ModelName", proofVM.ModelName),
            //                new XElement("CounterExamples",
            //                              from c in proofVM.CounterExampleInfos
            //                              select new XElement("CounterExample",
            //                                                  new XElement("Type", c.Type),
            //                                                  new XElement("VariableName", c.VariableName),
            //                                                  new XElement("VariableValue", c.VariableValue),
            //                                                  new XElement("VariableNames", c.VariableNames),
            //                                                  new XElement("VariableValues", c.VariableValues),
            //                                                  new XElement("CounterExampleVariables",
            //                                                       from v in c.CounterExampleVariables
            //                                                       select new XElement("CounterExampleVariable", v))
            //                                                  ))));

            //return xdoc;
            return null;
        }
    }
}