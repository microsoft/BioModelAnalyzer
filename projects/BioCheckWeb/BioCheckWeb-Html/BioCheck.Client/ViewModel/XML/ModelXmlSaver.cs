using System;
using System.Windows.Controls;
using System.Xml.Linq;
using BioCheck.Services;
using System.Linq;
using BioCheck.ViewModel.Models;
using Microsoft.Practices.Unity;

namespace BioCheck.ViewModel.XML
{
    /// <summary>
    /// Static ViewModelSaver for saving a ModelViewModel object to a local XML data file in Isolated Storage
    /// </summary>
    public static class ModelXmlSaver
    {
        private const string FILE_FILTER = "XML files (*.xml)|*.xml";

        public static void Save(ModelViewModel modelVM)
        {
            // TODO - take the save file dialog out of here and show it in the export toolbar command
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = FILE_FILTER;
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.DefaultFileName = modelVM.Name;

            if (saveFileDialog.ShowDialog() == true)
            {
                ApplicationViewModel.Instance.Container
                          .Resolve<IBusyIndicatorService>()
                          .Show("Exporting model...");

                // Convert the ModelViewModel to xml
                var xdoc = SaveToXml(modelVM);
                
                using (var stream = saveFileDialog.OpenFile())
                {
                    // TODO - check the xml indents
                    xdoc.Save(stream);
                    //var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true });
                }

                ApplicationViewModel.Instance.Container
                          .Resolve<IBusyIndicatorService>()
                          .Close();
            }
        }

        // TODO - remove this once removed save file dlg
        public static XDocument SaveToXml(ModelViewModel modelVM)
        {
            var xdoc = new XDocument(
                new XElement("Model",
                             new XAttribute("Id", modelVM.Id),
                             new XAttribute("Name", modelVM.Name),
                             new XAttribute("BioCheckVersion", Version.Major),
                             new XElement("Description", modelVM.Description),
                             new XElement("CreatedDate", modelVM.CreatedDate),
                             new XElement("ModifiedDate", modelVM.ModifiedDate),
                             new XElement("Layout", 
                                new XElement("Columns", modelVM.Columns),
                                new XElement("Rows", modelVM.Rows),
                                new XElement("ZoomLevel", Math.Round(modelVM.ZoomLevel, 2)),
                                new XElement("PanX",  Math.Round(modelVM.PanX)),
                                new XElement("PanY",  Math.Round(modelVM.PanY))
                                ),
                             new XElement("Containers",
                                        from cvm in modelVM.ContainerViewModels
                                        select new XElement("Container",
                                                            new XAttribute("Id", cvm.Id),
                                                            new XAttribute("Name", cvm.Name ?? ""),
                                                            new XElement("PositionX", cvm.PositionX),
                                                            new XElement("PositionY", cvm.PositionY),
                                                            new XElement("Size", (Int32) cvm.Size))
                                                            ),
                            new XElement("Variables",
                                        from v in
                                            (from extVvm in modelVM.VariableViewModels select extVvm)
                                            .Concat(
                                            (from cvm in modelVM.ContainerViewModels
                                            from intVvm in cvm.VariableViewModels select intVvm))
                                        select new XElement("Variable",
                                                            new XAttribute("Id", v.Id),
                                                            new XAttribute("Name", v.Name ?? ""),
                                                            new XElement("ContainerId", v.ContainerViewModel != null ? v.ContainerViewModel.Id : 0),
                                                            new XElement("Type", v.Type),
                                                            new XElement("RangeFrom", v.RangeFrom),
                                                            new XElement("RangeTo", v.RangeTo),
                                                            new XElement("Formula", v.Formula ?? ""),
                                                            new XElement("PositionX", v.PositionX),
                                                            new XElement("PositionY", v.PositionY),
                                                            new XElement("CellX", v.CellX),
                                                            new XElement("CellY", v.CellY),
                                                            new XElement("Angle", v.Angle.GetValueOrDefault()))),

                          new XElement("Relationships",
                                        from r in modelVM.RelationshipViewModels 
                                           select new XElement("Relationship",
                                                            new XAttribute("Id", r.Id),
                                                            new XElement("ContainerId", r.ContainerViewModel != null ? r.ContainerViewModel.Id : 0),
                                                            new XElement("FromVariableId", r.From.Id),
                                                            new XElement("ToVariableId", r.To.Id),
                                                            new XElement("Type", r.Type))))
                                                            );
            return xdoc;
        }
    }
}
