using System.Linq;
using BioCheck.ViewModel.Cells;

namespace BioCheck.ViewModel.Factories
{
    public static class NameFactory 
    {
        /// <summary>
        /// Creates a unique model name.
        /// </summary>
        /// <param name="name">The suggested name.</param>
        /// <returns></returns>
        public static string Create(string name)
        {
            string newName = name;

            int i = 0;

            var library = ApplicationViewModel.Instance.Library;

            while (library.Models.Any(mvm => mvm.Name == newName))
            {
                newName = string.Format("{0}({1})", name, ++i);
            }

            return newName;
        }

        /// <summary>
        /// Gets the fully resolved name of the variable in the "CellName.VariableName" format.
        /// </summary>
        /// <param name="variableVM">The variable VM.</param>
        /// <returns></returns>
        public static string GetVariableName(VariableViewModel variableVM)
        {
            var containerVM = variableVM.ContainerViewModel;

            if (containerVM != null)
            {
                if (!string.IsNullOrEmpty(containerVM.Name))
                {
                    return containerVM.Name + "." + variableVM.Name;
                }
            }

            return variableVM.Name;
        }

    }
}
