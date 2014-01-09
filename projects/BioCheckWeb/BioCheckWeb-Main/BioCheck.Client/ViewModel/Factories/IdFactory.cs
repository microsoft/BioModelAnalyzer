using System.Linq;
using BioCheck.ViewModel.Cells;
using BioCheck.ViewModel.Models;

namespace BioCheck.ViewModel.Factories
{
    public static class IdFactory 
    {
        public static int NewContainerId(ModelViewModel modelVM)
        {
            // Get the current largest container id
            var containerIds = (from cvm in modelVM.ContainerViewModels
                                where cvm.Id > 0
                                select cvm.Id)
                           .ToList();
            int maxId = containerIds.Count > 0 ? containerIds.Max() : 0;

            int newId = maxId + 1;
            return newId;
        }

        public static int NewVariableId(ModelViewModel modelVM)
        {
            // TODO - use any gaps

            // Get the current largest variable id
            // to use as the new seed number for variable id's that haven't been set yet
            var variableIds = (from vvm in
                                   (from extVvm in modelVM.VariableViewModels select extVvm)
                                   .Union(
                                   (from cvm in modelVM.ContainerViewModels
                                    from intVvm in cvm.VariableViewModels
                                    select intVvm))
                               where vvm.Id > 0
                               select vvm.Id)
                                 .ToList();
            int maxId = variableIds.Count > 0 ? variableIds.Max() : 0;

            int newId = maxId + 1;
            return newId;
        }

        public static int NewRelationshipId(ModelViewModel modelVM)
        {
            // Get the current largest relationship id
            var relationshipIds = (from rvm in modelVM.RelationshipViewModels
                                   where rvm.Id > 0
                                   select rvm.Id)
                                  .ToList();
            int relationshipId = relationshipIds.Count > 0 ? relationshipIds.Max() : 0;

            int newId = relationshipId + 1;
            return newId;
        }
    }
}
