using System.Linq;
using System.Xml.Linq;
using BioCheck.AnalysisService;
using BioCheck.Helpers;

namespace BioCheck.ViewModel.Proof
{
    /// <summary>
    /// Static class for handling the Analysis Output and updating the Variable and Container ViewModels
    /// </summary>
    public static class AnalysisOutputHandler
    {
        /// <summary>
        /// Handles the specified output and process its results.
        /// </summary>
        /// <param name="output">The output.</param>
        public static void Handle(AnalysisOutput output)
        {
            // Process the output results
            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            var allVariables = (from v in
                                    (from extVvm in modelVM.VariableViewModels select extVvm)
                                    .Concat(
                                        (from cvm in modelVM.ContainerViewModels
                                         from intVvm in cvm.VariableViewModels
                                         select intVvm))
                                select v).ToList();

            if(output.Ticks != null && output.Ticks.Count > 0)
            {
                var lastTick = output.Ticks.Last();

                if(lastTick.Variables != null)
                {
                    foreach (var variableOutput in lastTick.Variables)
                    {
                        // Find the container and variable
                        var variableVM = (from vvm in allVariables
                                          where vvm.Id == variableOutput.Id
                                          select vvm).First();

                        if (!variableOutput.IsStable)
                        {
                            variableVM.IsStable = false;

                            if (variableVM.ContainerViewModel != null)
                            {
                                variableVM.ContainerViewModel.IsStable = false;
                            }

                            variableVM.StabilityValue = variableOutput.Low + " - " + variableOutput.High;
                        }
                        else
                        {
                            variableVM.IsStable = true;
                            variableVM.StabilityValue = variableOutput.Low.ToString();
                        }
                    }
                }
            }
        }
    }
}