using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using BioCheck.AnalysisService;
using BioCheck.Helpers;
using BioCheck.ViewModel.Factories;
using BioCheck.ViewModel.Models;

namespace BioCheck.ViewModel.Proof
{
    /// <summary>
    /// Static Factory class for converting a ModelViewModel to AnalysisInput
    /// </summary>
    public static class AnalysisInputDTOFactory
    {
        /// <summary>
        /// Creates the AnalysisInput from the specified model VM.
        /// </summary>
        /// <param name="modelVM">The model VM.</param>
        /// <returns></returns>
        public static AnalysisInputDTO Create(ModelViewModel modelVM)
        {
            var data = new AnalysisInputDTO();

            data.ModelName = modelVM.Name;

            var xdoc = new XDocument(
                new XElement("AnalysisInput",
                             new XAttribute("ModelName", data.ModelName),
                             new XElement("Variables",
                                 (from v in
                                   (from extVvm in modelVM.VariableViewModels select extVvm)
                                   .Union(
                                   (from cvm in modelVM.ContainerViewModels
                                    from intVvm in cvm.VariableViewModels
                                    select intVvm))
                                          select new XElement("Variable",
                                                              new XAttribute("Id", v.Id),
                                                              new XElement("Name", NameFactory.GetVariableName(v)),
                                                              new XElement("RangeFrom", v.RangeFrom),
                                                              new XElement("RangeTo", v.RangeTo),
                                                              new XElement("Function", FormulaFactory.Create(v))))),
                            new XElement("Relationships",
                                          from r in modelVM.RelationshipViewModels
                                          select new XElement("Relationship",
                                                              new XAttribute("Id", r.Id),
                                                              new XElement("FromVariableId", r.From.Id),
                                                              new XElement("ToVariableId", r.To.Id),
                                                              new XElement("Type", r.Type.ToString())))));

            data.ZippedXml = ZipHelper.Zip(xdoc.ToString());

            return data;
        }
    }
}