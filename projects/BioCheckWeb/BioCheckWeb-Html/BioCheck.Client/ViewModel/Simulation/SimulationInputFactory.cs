using System;
using System.Collections.Generic;
using System.Linq;
using BioCheck.AnalysisService;
using BioCheck.ViewModel.Models;
using BioCheck.ViewModel.Proof;
using BioCheck.Helpers;
using System.Collections.ObjectModel;

namespace BioCheck.ViewModel.Simulation
{
    public static class SimulationInputFactory
    {
        public static SimulationInputDTO Create(ModelViewModel modelVM, SimulationViewModel simulationVM)
        {
            var simInput = new SimulationInputDTO();
            simInput.ModelName = modelVM.Name;

            var analysisInputDto = AnalysisInputDTOFactory.Create(modelVM);
            simInput.ZippedXml = analysisInputDto.ZippedXml;

            var vars = (from v in simulationVM.Variables
                        select new SimVariableDTO
                        {
                            Id = v.Id,
                            Value = v.InitialValue
                        });

            simInput.Variables = new ObservableCollection<SimVariableDTO>();

            foreach (var inputVar in vars)
            {
                simInput.Variables.Add(inputVar);
            }

            return simInput;
        }

        public static SimulationInputDTO Update(SimulationViewModel simulationVM, SimulationInputDTO lastInput)
        {
            var nextInput = new SimulationInputDTO();
            nextInput.EnableLogging = lastInput.EnableLogging;
            nextInput.ModelName = lastInput.ModelName;
            nextInput.ZippedXml = lastInput.ZippedXml;

            var vars = (from v in simulationVM.Variables
                        let lastStep = v.Steps.LastOrDefault()
                        select new SimVariableDTO
                            {
                                Id = v.Id,
                                Value = lastStep == null ? 0 : lastStep.Value
                            });

            nextInput.Variables = new ObservableCollection<SimVariableDTO>();

            foreach (var inputVar in vars)
            {
                nextInput.Variables.Add(inputVar);
            }

            return nextInput;
        }
    }
}