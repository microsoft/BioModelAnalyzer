using System;
using System.Collections.Generic;
using System.Linq;
using BioCheck.AnalysisService;
using BioCheck.ViewModel.Models;
using BioCheck.ViewModel.Proof;
using BioCheck.ViewModel.Time;
using BioCheck.Helpers;

namespace BioCheck.ViewModel.Time
{
    public static class TimeViewModelFactory
    {
        // Time edit

        public static TimeViewModel CreatePopUp(ModelViewModel input)
        {
            var timeVM = new TimeViewModel();
            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            //timeVM.ModelName = input.ModelName;
            return timeVM;
        }

        public static TimeViewModel Create(AnalysisInputDTO input, TimeOutput output)
        {
            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            var timeVM = new TimeViewModel(input, output);
            timeVM.ModelName = input.ModelName;

            // Changes the visual state view!
            timeVM.State = output.Status == StatusTypes.True ? LTLViewState.Simulation : LTLViewState.NoSimulation;

            // Need to capture these.
            timeVM.Time = output.Time;
            timeVM.Steps = output.Ticks.Count;

            var allVariables = (from v in
                                    (from extVvm in modelVM.VariableViewModels select extVvm)
                                    .Concat(
                                        (from cvm in modelVM.ContainerViewModels
                                         from intVvm in cvm.VariableViewModels
                                         select intVvm))
                                select v).ToList();

            var lastTick = output.Ticks.Last();
            int NCells = modelVM.ContainerViewModels.Count();

            foreach (var variableOutput in lastTick.Variables)
            {
                // Find the variable's container and variable
                var variableVM = (from vvm in allVariables
                                  where vvm.Id == variableOutput.Id
                                  select vvm).First();


                var pi = new ProgressionInfo();
                pi.Id = variableOutput.Id;

                
                var vartimeVM = new VariableProofViewModel
                {
                    Id = variableOutput.Id,
                    Name = variableVM.Name,
                    Range =
                        variableOutput.IsStable
                            ? variableOutput.Low.ToString()
                            : string.Format("{0} - {1}", variableOutput.Low, variableOutput.High)
                };

                
                // Cell name editor dependent on whether cells exist, and if names were provided
                if (NCells > 0)
                {
                    if (variableVM.ContainerViewModel != null)
                    {
                        vartimeVM.CellName = pi.CellName = variableVM.ContainerViewModel.Name;
                        pi.Name = variableVM.ContainerViewModel.Name + " " + variableVM.Name;           // 2-in-1 for easy display
                    }
                    else
                    {
                        vartimeVM.CellName = pi.CellName = "";
                        pi.Name = " " + variableVM.Name;
                        // A space for consistency with intracellular variables in cells that lack name
                    }
                }
                else
                {
                    vartimeVM.CellName = pi.CellName = "Extracellular";
                    pi.Name = variableVM.Name;
                }

                
                // Formula editor dependent on whether a formula was provided
                if (variableVM.Formula == "")
                {
                    vartimeVM.TargetFunction = "avg(pos)-avg(neg)";
                }
                else
                {
                    vartimeVM.TargetFunction = variableVM.Formula;
                }
                
                timeVM.Variables.Add(vartimeVM);
                timeVM.TimeInfos.Add(pi);
            }
            // Above's ok.
            ////_______ <-- -->
            foreach (var tick in output.Ticks)
            {
                var tickName = string.Format("T = {0}", tick.Time);

                for (int i = 0; i < tick.Variables.Count; i++)
                {
                    var tickVariable = tick.Variables[i];

                    var progressionInfo = timeVM.TimeInfos[i];

                    progressionInfo.Steps.Add(
                            new ProgressionStepInfo
                            {
                                Name = tickName,
                                Values = tickVariable.IsStable
                                             ? tickVariable.Low.ToString()
                                             : string.Format("{0} - {1}", tickVariable.Low, tickVariable.High),
                                IsStable = tickVariable.IsStable
                            }
                    );
                }
            }

            return timeVM;
        }
    }
}