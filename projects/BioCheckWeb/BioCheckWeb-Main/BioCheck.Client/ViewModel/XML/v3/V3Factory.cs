using System;
using System.Xml.Linq;
using BioCheck.ViewModel.Cells;
using BioCheck.ViewModel.Models;
using MvvmFx.Common.Helpers;
using System.Linq;

namespace BioCheck.ViewModel.XML.v3
{
    /// <summary>
    /// Factory for creating and populating a ModelViewModel from
    /// a version 3 XML file.
    /// </summary>
    public class V3Factory : ModelXmlFactory
    {
        protected override ModelViewModel OnCreate(XDocument xdoc)
        {
            var modelVM = new ModelViewModel();

            var xm = xdoc.Element("Model");

            modelVM.Id = xm.AttributeInt("Id");
            modelVM.Name = xm.AttributeString("Name");
            modelVM.Description = xm.ElementString("Description", false);
            modelVM.Author = xm.ElementString("Author", false);

            modelVM.CreatedDate = xm.ElementDateTime("CreatedDate");
            modelVM.ModifiedDate = xm.ElementDateTime("ModifiedDate");

            return modelVM;
        }

        protected override void OnLoad(XDocument xdoc, ModelViewModel modelVM)
        {
            var xm = xdoc.Element("Model");

            var xl = xm.Element("Layout");
            modelVM.Columns = xl.ElementInt("Columns");
            modelVM.Rows = xl.ElementInt("Rows");
            modelVM.ZoomLevel = xl.ElementDouble("ZoomLevel");
            modelVM.PanX = xl.ElementDouble("PanX");
            modelVM.PanY = xl.ElementDouble("PanY");

            modelVM.ContainerViewModels.AddRange(from xc in xm.Descendants("Container")
                                                 select new ContainerViewModel
                                                            {
                                                                Id = xc.AttributeInt("Id"),
                                                                Name = xc.AttributeString("Name", false),
                                                                PositionX = xc.ElementInt("PositionX"),
                                                                PositionY = xc.ElementInt("PositionY"),
                                                                Size = (ContainerSizeTypes)xc.ElementInt("Size", 1)
                                                            });
            (from xv in xm.Descendants("Variable")
             select new VariableViewModel
                        {
                            Id = xv.AttributeInt("Id"),
                            Name = xv.AttributeString("Name"),
                            ContainerViewModel = modelVM.ContainerViewModels.GetItemById(xv.ElementInt("ContainerId", false)),
                            Type = EnumHelper.Parse<VariableTypes>(xv.ElementString("Type")),
                            RangeFrom = xv.ElementInt("RangeFrom"),
                            RangeTo = xv.ElementInt("RangeTo"),
                            Formula = xv.ElementString("Formula", false),
                            PositionX = xv.ElementInt("PositionX"),
                            PositionY = xv.ElementInt("PositionY"),
                            CellX = xv.ElementInt("CellX", false),
                            CellY = xv.ElementInt("CellY", false),
                            Angle = xv.ElementNullableInt("Angle", false),
                        })
                       .ToList()
                       .ForEach(vvm =>
                                    {
                                        if (vvm.ContainerViewModel == null)
                                        {
                                            modelVM.VariableViewModels.Add(vvm);
                                        }
                                        else
                                        {
                                            vvm.CellX = vvm.ContainerViewModel.PositionX;
                                            vvm.CellY = vvm.ContainerViewModel.PositionY;

                                            // If the variable is a constant, we add it to the Model
                                            // and delete the 'ghost' ContainerVM.
                                            if (vvm.Type == VariableTypes.Constant)
                                            {
                                                modelVM.VariableViewModels.Add(vvm);
                                                modelVM.ContainerViewModels.Remove(vvm.ContainerViewModel);
                                                vvm.ContainerViewModel = null;
                                            }
                                            else
                                            {
                                                vvm.ContainerViewModel.VariableViewModels.Add(vvm);
                                            }
                                        }
                                    });

            (from xr in xm.Descendants("Relationship")
             let allVariables = (from v in
                                     (from extVvm in modelVM.VariableViewModels select extVvm)
                                         .Concat(
                                         (from cvm in modelVM.ContainerViewModels
                                          from intVvm in cvm.VariableViewModels
                                          select intVvm))
                                 select v)
             select new RelationshipViewModel
             {
                 Id = xr.AttributeInt("Id"),
                 ContainerViewModel = modelVM.ContainerViewModels.GetItemById(xr.ElementInt("ContainerId")),
                 From = (from vvm in allVariables
                         where vvm.Id == xr.ElementInt("FromVariableId")
                         select vvm).FirstOrDefault(),
                 To = (from vm in allVariables
                       where vm.Id == xr.ElementInt("ToVariableId")
                       select vm).FirstOrDefault(),
                 Type = EnumHelper.Parse<RelationshipTypes>(xr.ElementString("Type")),
             })
                .ToList()
                .ForEach(rvm =>
                {
                    if (rvm.Type != RelationshipTypes.Activator && rvm.Type != RelationshipTypes.Inhibitor)
                        rvm.Type = RelationshipTypes.Activator;

                    modelVM.RelationshipViewModels.Add(rvm);

                    //if (rvm.From == null || rvm.To == null)
                    //{
                    //    throw new Exception("Missing Variable");
                    //}
                });
        }
    }
}
