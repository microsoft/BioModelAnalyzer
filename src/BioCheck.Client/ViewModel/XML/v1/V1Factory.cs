using System;
using System.Xml.Linq;
using BioCheck.ViewModel.Cells;
using BioCheck.ViewModel.Models;
using BioCheck.ViewModel.Proof;
using MvvmFx.Common.Helpers;
using System.Linq;

namespace BioCheck.ViewModel.XML.v1
{
    /// <summary>
    /// ViewModelFactory for creating and populating a ModelViewModel from
    /// an XML file.
    /// </summary>
    public class V1Factory : ModelXmlFactory
    {
        protected override ModelViewModel OnCreate(XDocument xdoc)
        {
            var modelVM = new ModelViewModel();

            var xlocaldata = xdoc.Element("LocalData");
            var xm = xlocaldata.Element("Model");

            modelVM.Id = xm.ElementInt("Id");
            modelVM.Name = xm.ElementString("Name");
            modelVM.Description = xm.ElementString("Description");

            modelVM.CreatedDate = xm.ElementDateTime("CreatedDate");
            modelVM.ModifiedDate = xm.ElementDateTime("ModifiedDate");

            return modelVM;
        }

        protected override void OnLoad(XDocument xdoc, ModelViewModel modelVM)
        {
            var xlocaldata = xdoc.Element("LocalData");
            var xm = xlocaldata.Element("Model");

            modelVM.Columns = xm.ElementInt("Columns");
            modelVM.Rows = xm.ElementInt("Rows");

            modelVM.ContainerViewModels.AddRange(from xc in xlocaldata.Descendants("Container")
                                                 select new ContainerViewModel
                                                 {
                                                     Id = xc.ElementInt("Id"),
                                                     PositionX = xc.ElementInt("PositionX"),
                                                     PositionY = xc.ElementInt("PositionY"),
                                                     Size = (ContainerSizeTypes)xc.ElementInt("Size", 1)
                                                 });

            (from xv in xlocaldata.Descendants("Variable")
             let containerVM = modelVM.ContainerViewModels.GetItemById(xv.ElementInt("ContainerId"))
             //let containerId = containerVM.Id
             select new VariableViewModel
             {
                 Id = xv.ElementInt("Id"),
                 Name = xv.ElementString("Name"),
                 ContainerViewModel = containerVM,
                 Type = EnumHelper.Parse<VariableTypes>(xv.ElementString("Type")),
                 RangeFrom = xv.ElementInt("RangeFrom"),
                 RangeTo = xv.ElementInt("RangeTo"),
                 Formula = xv.ElementString("Formula", false),
                 PositionX = xv.ElementInt("PositionX"),
                 PositionY = xv.ElementInt("PositionY")
             })
                       .ToList()
                       .ForEach(vvm =>
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
                       });

            (from xr in xlocaldata.Descendants("Relationship")
             select new RelationshipViewModel
             {
                 Id = xr.ElementInt("Id"),
                 ContainerViewModel = modelVM.ContainerViewModels.GetItemById(xr.ElementInt("ContainerId")),
                 From = (from cvm in modelVM.ContainerViewModels
                         from vvm in cvm.VariableViewModels
                         where vvm.Id == xr.ElementInt("FromVariableId")
                         select vvm).FirstOrDefault(),
                 To = (from cvm in modelVM.ContainerViewModels
                       from vm in cvm.VariableViewModels
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

                     if (rvm.From == null || rvm.To == null)
                     {

                     }
                 });
        }
    }
}
