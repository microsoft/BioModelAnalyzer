using System;
using System.Collections.Generic;
using System.Linq;
using BioCheck.AnalysisService;
using BioCheck.ViewModel.Models;

namespace BioCheck.ViewModel.Proof
{
    public static class ProofViewModelFactory
    {
        public static ProofViewModel Create(AnalysisInputDTO input, AnalysisOutput output)
        {
            var modelVM = ApplicationViewModel.Instance.ActiveModel;

            var proofVM = new ProofViewModel(input, output);

            proofVM.ModelName = modelVM.Name;
            proofVM.State = output.Status == StatusTypes.Stabilizing ? ProofViewState.Stable : ProofViewState.NotStable;

            proofVM.Time = output.Time;
            proofVM.Steps = output.Ticks.Count;

            var allVariables = (from v in
                                    (from extVvm in modelVM.VariableViewModels select extVvm)
                                    .Concat(
                                        (from cvm in modelVM.ContainerViewModels
                                         from intVvm in cvm.VariableViewModels
                                         select intVvm))
                                select v).ToList();

            var lastTick = output.Ticks.Last();
            foreach (var variableOutput in lastTick.Variables)
            {
                // Find the container and variable
                var variableVM = (from vvm in allVariables
                                  where vvm.Id == variableOutput.Id
                                  select vvm).First();

                // If default target function is used, no formula is stored in variableVM.Formula, so is not cited in the ProofTable.
                // Fixed here.
                if(variableVM.Formula == "")
                {
                   var varProofVM = new VariableProofViewModel
                                     {
                                         Id = variableOutput.Id,
                                         Name = variableVM.Name,
                                         TargetFunction = "avg(pos)-avg(neg)",
                                         Range =
                                             variableOutput.IsStable
                                                 ? variableOutput.Low.ToString()
                                                 : string.Format("{0} - {1}", variableOutput.Low, variableOutput.High)
                                     };
                   proofVM.Variables.Add(varProofVM);
                }
                else
                {
                    var varProofVM = new VariableProofViewModel
                                     {
                                         Id = variableOutput.Id,
                                         Name = variableVM.Name,
                                         TargetFunction = variableVM.Formula,
                                         Range =
                                             variableOutput.IsStable
                                                 ? variableOutput.Low.ToString()
                                                 : string.Format("{0} - {1}", variableOutput.Low, variableOutput.High)
                                     };
                    proofVM.Variables.Add(varProofVM);
                }
                

                var pi = new ProgressionInfo();
                pi.Name = variableVM.Name;
                pi.Id = variableOutput.Id;

                proofVM.ProgressionInfos.Add(pi);
            }

            foreach (var tick in output.Ticks)
            {
                var tickName = string.Format("T = {0}", tick.Time);

                for (int i = 0; i < tick.Variables.Count; i++)
                {
                    var tickVariable = tick.Variables[i];

                    var progressionInfo = proofVM.ProgressionInfos[i];

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

            return proofVM;
        }

        public static List<CounterExampleInfo> CreateCounterExamples(ProofViewModel proofVM, FurtherTestingOutput output)
        {
            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            var cexs = new List<CounterExampleInfo>();

            int number = 1;

            foreach (var cex in output.CounterExamples)
            {
                if(cex.Status ==  StatusTypes.Bifurcation)
                {
                    var bcex = new BifurcationCounterExample();
                    bcex.Number = number;
                    bcex.VariableInfos = (from bv in cex.BifurcatingVariables
                                          let pv = proofVM.Variables.First(pv => pv.Id == bv.Id)
                                          select new BifurcatingVariableInfo
                                                     {
                                                         Name =  pv.Name,
                                                         CalculatedBound = pv.Range,
                                                         Fix1 = bv.Fix1,
                                                         Fix2 = bv.Fix2,
                                                     }).ToList();
                    cexs.Add(bcex);
                }
                else if(cex.Status ==  StatusTypes.Cycle)
                {
                    var ocex = new OscillationCounterExample();
                    ocex.Number = number;
                    ocex.VariableInfos = (from ov in cex.OscillatingVariables
                                          let pv = proofVM.Variables.First(pv => pv.Id == ov.Id)
                                          select new OscillatingVariableInfo
                                          {
                                              Name = pv.Name,
                                              CalculatedBound = pv.Range,
                                              Oscillation = ov.Oscillation,
                                          }).ToList();
                    cexs.Add(ocex);
                }
                else if(cex.Status ==  StatusTypes.Fixpoint)
                {
                    
                }
                else
                {
                    throw new Exception("Error with the counter examples");
                }
            }
        
            return cexs;
        }

        static bool flip = false;

        private static void CreateMock(ProofViewModel proofVM, ModelViewModel modelVM)
        {
            flip = !flip;
            proofVM.State = flip ? ProofViewState.Stable : ProofViewState.NotStable;

            proofVM.Time = 10;
            proofVM.Steps = 5;

            proofVM.Variables = new List<VariableProofViewModel>
                                {
                                    new VariableProofViewModel{Id = 1, Name="ConstA", TargetFunction = "Constant: 2", Range="2"},
                                    new VariableProofViewModel{Id = 2, Name="Rec1", TargetFunction = "avg(pos)-avg(neg)", Range="0 - 3"},
                                    new VariableProofViewModel{Id = 3, Name="Rec2", TargetFunction = "avg(pos)-avg(neg)", Range="2 - 5"},
                                    new VariableProofViewModel{Id = 4, Name="VarA", TargetFunction = "avg(pos)-avg(neg)", Range="1 - 4"},
                                    new VariableProofViewModel{Id = 5, Name="VarB", TargetFunction = "avg(pos)-avg(neg)", Range="0 - 2"},
                                    new VariableProofViewModel{Id = 6, Name="VarC", TargetFunction = "VarD - VarA", Range="0 - 3"},
                                    new VariableProofViewModel{Id = 7, Name="VarD", TargetFunction = "avg(pos)-avg(neg)", Range="1 - 4"},
                                    new VariableProofViewModel{Id = 8, Name="VarE", TargetFunction = "2-var(Rec1)", Range="0 - 3"},
                                    new VariableProofViewModel{Id = 9, Name="ProtF", TargetFunction = "2-var(Rec1)", Range="0 - 3"},
                                };

            proofVM.ProgressionInfos = new List<ProgressionInfo>
                                           {
                                               new ProgressionInfo{
                                                   Name="ConstA", 
                                                   Id = 1,
                                                   Steps = new List<ProgressionStepInfo>()
                                                                                              {
                                                                                                  new ProgressionStepInfo{Name= "T = 0", Values = "2", IsStable = true},
                                                                                                  new ProgressionStepInfo{Name= "T = 1", Values = "2", IsStable = true},
                                                                                                  new ProgressionStepInfo{Name= "T = 2", Values = "2", IsStable = true},
                                                                                                  new ProgressionStepInfo{Name= "T = 3", Values = "2", IsStable = true},
                                                                                                  new ProgressionStepInfo{Name= "T = 4", Values = "2", IsStable = true},
                                                                                              }},
                                                     new ProgressionInfo{
                                                   Name="Rec1",
                                                   Id = 2,
                                                     Steps = new List<ProgressionStepInfo>()
                                                                                              {
                                                                                                  new ProgressionStepInfo{Name= "T = 0", Values = "0-3", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 1", Values = "0-2", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 2", Values = "1", IsStable = true},
                                                                                                  new ProgressionStepInfo{Name= "T = 3", Values = "1", IsStable = true},
                                                                                                  new ProgressionStepInfo{Name= "T = 4", Values = "1", IsStable = true},
                                                                                              }},

                                                    new ProgressionInfo{
                                                   Name="Rec2", 
                                                   Id = 3,
                                               Steps = new List<ProgressionStepInfo>()
                                                                                              {
                                                                                                  new ProgressionStepInfo{Name= "T = 0", Values = "2-5", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 1", Values = "3-5", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 2", Values = "4-5", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 3", Values = "5", IsStable = true},
                                                                                                  new ProgressionStepInfo{Name= "T = 4", Values = "5", IsStable = true},
                                                                                              }},

                                                    new ProgressionInfo{
                                                   Name="VarA", 
                                                   Id = 4,
                                                 Steps = new List<ProgressionStepInfo>()
                                                                                              {
                                                                                                  new ProgressionStepInfo{Name= "T = 0", Values = "1-4", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 1", Values = "1-4", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 2", Values = "2-4", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 3", Values = "3", IsStable = true},
                                                                                                  new ProgressionStepInfo{Name= "T = 4", Values = "3", IsStable = true},
                                                                                              }},

                                                    new ProgressionInfo{
                                                   Name="VarB",
                                                   Id = 5, 
                                                  Steps = new List<ProgressionStepInfo>()
                                                                                              {
                                                                                                  new ProgressionStepInfo{Name= "T = 0", Values = "0-2", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 1", Values = "2", IsStable = true},
                                                                                                  new ProgressionStepInfo{Name= "T = 2", Values = "2", IsStable = true},
                                                                                                  new ProgressionStepInfo{Name= "T = 3", Values = "2", IsStable = true},
                                                                                                  new ProgressionStepInfo{Name= "T = 4", Values = "2", IsStable = true},
                                                                                              }},

                                                    new ProgressionInfo{
                                                   Name="VarC", 
                                                   Id = 6, 
                                                  Steps = new List<ProgressionStepInfo>()
                                                                                              {
                                                                                                  new ProgressionStepInfo{Name= "T = 0", Values = "0-3", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 1", Values = "0-2", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 2", Values = "1", IsStable = true},
                                                                                                  new ProgressionStepInfo{Name= "T = 3", Values = "1", IsStable = true},
                                                                                                  new ProgressionStepInfo{Name= "T = 4", Values = "1", IsStable = true},
                                                                                              }},

                                                    new ProgressionInfo{
                                                   Name="VarD", 
                                                   Id = 7, 
                                                   Steps = new List<ProgressionStepInfo>()
                                                                                              {
                                                                                                  new ProgressionStepInfo{Name= "T = 0", Values = "1-4", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 1", Values = "1-4", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 2", Values = "1-4", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 3", Values = "2-4", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 4", Values = "3", IsStable = true},
                                                                                              }},
                                                    new ProgressionInfo{
                                                   Name="VarE", 
                                                   Id = 8, 
                                                   Steps = new List<ProgressionStepInfo>()
                                                                                              {
                                                                                                  new ProgressionStepInfo{Name= "T = 0", Values = "0-3", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 1", Values = "0-2", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 2", Values = "1", IsStable = true},
                                                                                                  new ProgressionStepInfo{Name= "T = 3", Values = "1", IsStable = true},
                                                                                                  new ProgressionStepInfo{Name= "T = 4", Values = "1", IsStable = true},
                                                                                              }},
                                                    new ProgressionInfo{
                                                   Name="Protf", 
                                                   Id = 9, 
                                                Steps = new List<ProgressionStepInfo>()
                                                                                              {
                                                                                                  new ProgressionStepInfo{Name= "T = 0", Values = "0-3", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 1", Values = "0-2", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 2", Values = "0-2", IsStable = false},
                                                                                                  new ProgressionStepInfo{Name= "T = 3", Values = "0", IsStable = true},
                                                                                                  new ProgressionStepInfo{Name= "T = 4", Values = "0", IsStable = true},
                                                                                              }},
                                           };

            proofVM.CounterExampleInfos = new List<CounterExampleInfo>
                                              {
                                                //  new CounterExampleInfo
                                                //      {
                                                //          Type = CounterExampleTypes.Bifurcation,
                                                //          Number = 1,
                                                //          VariableName = "VarA",
                                                //          VariableValue = "2",
                                                //          CounterExampleVariables = new List<string>
                                                //                                     {
                                                //                                         "VarX: 2,4",
                                                //                                         "VarY: 3,4",
                                                //                                         "VarZ: 1,4",
                                                //                                     }
                                                //      },
                                                //new CounterExampleInfo
                                                //    {
                                                //          Type = CounterExampleTypes.Oscillation,
                                                //          Number = 2,
                                                //          VariableNames = "VarA, VarB",
                                                //          VariableValues = "2,4",
                                                //          CounterExampleVariables = new List<string>
                                                //                                     {
                                                //                                         "VarA: 2,3,4",
                                                //                                         "VarC: 1,4,7",
                                                //                                     }  
                                                //    }
                                              };

        }
    }
}