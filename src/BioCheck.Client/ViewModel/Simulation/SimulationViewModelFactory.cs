using System;
using System.Collections.Generic;
using System.Linq;
using BioCheck.AnalysisService;
using BioCheck.ViewModel.Models;
using BioCheck.ViewModel.Proof;
using BioCheck.Helpers;

namespace BioCheck.ViewModel.Simulation
{
    public static class SimulationViewModelFactory
    {
        public static SimulationViewModel Create(ModelViewModel modelVM)
        {
            var simulationVM = new SimulationViewModel();     

            simulationVM.ModelName = modelVM.Name;
            simulationVM.NumberOfSteps = 20;
            int NCells = modelVM.ContainerViewModels.Count();

            var allVariables = (from v in
                                    (from extVvm in modelVM.VariableViewModels select extVvm)
                                    .Concat(
                                        (from cvm in modelVM.ContainerViewModels
                                         from intVvm in cvm.VariableViewModels
                                         select intVvm))
                                select v).ToList();

            
            // Create the list of Variables

                foreach (var variableVM in allVariables)
                {
                    var varSimVM = new VariableSimViewModel
                    {
                        Id = variableVM.Id,
                        Name = variableVM.Name,
                        RangeFrom = variableVM.RangeFrom,
                        RangeTo = variableVM.RangeTo,
                        Range = string.Format("{0} - {1}", variableVM.RangeFrom, variableVM.RangeTo),
                    }; 


                    if (NCells > 0)
                    {                        
                        if (variableVM.ContainerViewModel != null)
                        {
                            varSimVM.CellName = variableVM.ContainerViewModel.Name;
                        }
                        else
                        {
                            // Could be either extracellular or no name provided
                            varSimVM.CellName = "";                                 
                        }
                    }
                    else 
                    {
                        // No cells present in the model
                        varSimVM.CellName = "Extracellular";                         
                    }
                    
                    varSimVM.RandomiseValue();
                    simulationVM.Variables.Add(varSimVM);
                }
            

            // Create the steps
            //    var stepValues = new List<List<int>>();

            // Hack - make mock steps
            //for (int i = 1; i <= 20; i++)
            //{
            //    var thisStepValues = new List<int>();
            //    List<int> lastStepValues = i > 1 ? stepValues[i - 2] : null;

            //    int variableIndex = 0;
            //    foreach (var varSimVM in simulationVM.Variables)
            //    {
            //        int thisValue = RandomHelper.GetRandom(varSimVM.RangeFrom, varSimVM.RangeTo);

            //        var stepInfo = new SimStepInfo
            //                {
            //                    Name = i.ToString(),
            //                    Value = thisValue,
            //                };

            //        if (lastStepValues != null)
            //        {
            //            int lastValue = lastStepValues[variableIndex];
            //            if (lastValue != thisValue)
            //            {
            //                // Hack - only change 1/4
            //                if (RandomHelper.GetRandom(0, 3) == 0)
            //                {
            //                    stepInfo.HasChanged = true;
            //                }
            //                else
            //                {
            //                    stepInfo.Value = lastValue;
            //                }
            //            }
            //        }

            //        thisStepValues.Add(stepInfo.Value);

            //        varSimVM.Steps.Add(stepInfo);
            //        variableIndex++;
            //    }

            //    stepValues.Add(thisStepValues);
            //}

            return simulationVM;
        }

        public static void UpdateSteps(SimulationViewModel simulationVM)
        {
            foreach (var varVM in simulationVM.Variables)
            {
                int lastValue = varVM.Steps[0].Value;

                for (int i = 1; i < varVM.Steps.Count; i++)
                {
                    var thisStep = varVM.Steps[i];

                    if (thisStep.Value != lastValue)
                        thisStep.HasChanged = true;

                    lastValue = thisStep.Value;
                }
            }

            var stepValues = new List<List<int>>();

            for (int i = 0; i < simulationVM.NumberOfSteps; i++)
            {
                var thisStepValues = new List<int>();

                foreach (var varVM in simulationVM.Variables)
                {
                    var thisStep = varVM.Steps[i];
                    thisStepValues.Add(thisStep.Value);
                }

                stepValues.Add(thisStepValues);
            }

            // Look for equality
            var cycledSteps = GetEqualSteps(stepValues);
            if (cycledSteps != null)
            {
                foreach (var thisStepValues in stepValues)
                {
                    if (thisStepValues.SequenceEqual(cycledSteps))
                    {
                        var indexOfCycle = stepValues.IndexOf(thisStepValues);

                        foreach (var variableVM in simulationVM.Variables)
                        {
                            var stepInfo = variableVM.Steps[indexOfCycle];
                            stepInfo.HasCycled = true;
                        }
                    }
                }
            }
        }

        private static List<int> GetEqualSteps(List<List<int>> stepValues)
        {
            for (int currentStep = 1; currentStep < stepValues.Count; currentStep++)
            {
                var thisStepValues = stepValues[currentStep];
                for (int previousStep = 0; previousStep < currentStep; previousStep++)
                {
                    var previousStepValues = stepValues[previousStep];

                    if (thisStepValues.SequenceEqual(previousStepValues))
                    {
                        return thisStepValues;
                    }
                }
            }

            return null;
        }

        public static void Plus10Steps(SimulationViewModel simulationVM)
        {
            //foreach (var varSimVM in simulationVM.Variables)
            //{
            //    int lastValue = varSimVM.Steps.Last().Value;

            //    // Hack - make mock steps

            //    for (int i = simulationVM.NumberOfSteps + 1; i <= simulationVM.NumberOfSteps + 10; i++)
            //    {
            //        int thisValue = RandomHelper.GetRandom(varSimVM.RangeFrom, varSimVM.RangeTo);

            //        var stepInfo = new SimStepInfo
            //        {
            //            Name = i.ToString(),
            //            Value = thisValue,
            //        };

            //        if (lastValue != thisValue)
            //        {
            //            // Hack - only change 1/4
            //            if (RandomHelper.GetRandom(0, 3) == 0)
            //            {
            //                stepInfo.HasChanged = true;
            //            }
            //            else
            //            {
            //                thisValue = lastValue;
            //                stepInfo.Value = lastValue;
            //            }
            //        }

            //        varSimVM.Steps.Add(stepInfo);

            //        lastValue = thisValue;
            //    }
            //}
        }

        public static void Minus10Steps(SimulationViewModel simulationVM)
        {
            foreach (var varSimVM in simulationVM.Variables)
            {
                if (varSimVM.Steps.Count > 0)
                {
                    varSimVM.Steps.RemoveRange(varSimVM.Steps.Count - 10, 10);
                }
            }
        }
    }
}