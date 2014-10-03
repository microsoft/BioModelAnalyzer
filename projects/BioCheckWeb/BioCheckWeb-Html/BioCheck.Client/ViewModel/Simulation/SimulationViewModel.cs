using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using BioCheck.AnalysisService;
using BioCheck.Services;
using BioCheck.ViewModel.Cells;
using BioCheck.ViewModel.Factories;
using BioCheck.ViewModel.Proof;
using MvvmFx.Common.ViewModels.Behaviors.Observable;
using MvvmFx.Common.ViewModels.Commands;
using Microsoft.Practices.Unity;

namespace BioCheck.ViewModel.Simulation
{
    public class SimulationViewModel : ObservableViewModel
    {
        private readonly DelegateCommand runCommand;
        private readonly DelegateCommand closeCommand;
        private readonly DelegateCommand cancelProofCommand;
        private readonly DelegateCommand showGraphCommand;
        private readonly DelegateCommand randomizeCommand;
        private readonly DelegateCommand plus10StepsCommand;
        private readonly DelegateCommand minus10StepsCommand;
        private readonly DelegateCommand graphAllCommand;

        private string modelName;
        private int numberOfSteps;
        private List<VariableSimViewModel> variables;
        private VariableSimViewModel selectedSimVariable;

        public SimulationViewModel()
        {
            this.runCommand = new DelegateCommand(OnRunExecuted);
            this.closeCommand = new DelegateCommand(OnCloseExecuted);
            this.cancelProofCommand = new DelegateCommand(OnCancelProofExecuted);
            this.showGraphCommand = new DelegateCommand(OnShowGraphExecuted);
            this.graphAllCommand = new DelegateCommand(OnGraphAllExecuted);
            this.randomizeCommand = new DelegateCommand(OnRandomizedExecuted);
            this.plus10StepsCommand = new DelegateCommand(OnPlus10StepsExecuted);
            this.minus10StepsCommand = new DelegateCommand(OnMinus10StepsExecuted);

            this.variables = new List<VariableSimViewModel>();
        }

        public DelegateCommand RunCommand
        {
            get { return this.runCommand; }
        }

        private AnalysisServiceClient analyzerClient;
        private DateTime timer;

        private SimulationInputDTO simInput;
        private SimulationOutputDTO simOutput;
        private int currentStep;

        private void OnRunExecuted()
        {
            ApplicationViewModel.Instance.Container
                 .Resolve<IBusyIndicatorService>()
                 .Show("Running simulation...", CancelProofCommand);

            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            simInput = SimulationInputFactory.Create(modelVM, this);

            CurrentStep = 0;

            // Reset the current view whilst it's loading
            // Mark all the variables as being modifiable now
            this.variables.ForEach(v =>
            {
                v.ModifiableValue();
                v.Steps.Clear();
            });

            // Enable/Disable logging
            simInput.EnableLogging = ApplicationViewModel.Instance.ToolbarViewModel.EnableAnalyzerLogging;

            // Create the analyzer client
            if (analyzerClient == null)
            {
                var serviceUri = new Uri("../Services/AnalysisService.svc", UriKind.Relative);
                var endpoint = new EndpointAddress(serviceUri);
                analyzerClient = new AnalysisServiceClient("AnalysisServiceCustom", endpoint);
                analyzerClient.SimulateCompleted += OnSimulateCompleted;
            }

            // Invoke the async Analyze method on the service
            timer = DateTime.Now;
            analyzerClient.SimulateAsync(simInput);
        }

        private void OnSimulateCompleted(object sender, SimulateCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                OnSimulationError(e.Error);
            }
            else
            {
                try
                {
                    // Get the resulting dictionary of variables and their values
                    this.simOutput = e.Result;

                    // Increment the current step
                    currentStep += 1;

                    // Add a column of steps to each variable
                    foreach (var variableValue in simOutput.Variables)
                    {
                        var variable = this.variables.First(v => v.Id == variableValue.Id);

                        var stepInfo = new SimStepInfo
                        {
                            Name = currentStep.ToString(),
                            Value = variableValue.Value
                        };
                        variable.Steps.Add(stepInfo);
                    }

                    OnPropertyChanged(() => CurrentStep);

                    // If the current step is lower than the number of steps, re-call the Simulate tick 
                    // passing in this latest step
                    if (this.currentStep < this.numberOfSteps)
                    {
                        this.simInput = SimulationInputFactory.Update(this, simInput);
                        analyzerClient.SimulateAsync(simInput);
                    }
                    else
                    {
                        SimulationViewModelFactory.UpdateSteps(this);

                        // Update the graph if it's open
                        ApplicationViewModel.Instance.Container
                                 .Resolve<IGraphWindowService>()
                                 .Update(new Func<GraphViewModel>(() => GraphFactory.Create(this)));

                        // If we've reached the total number of steps, then close the busy dialog
                        ApplicationViewModel.Instance.Container
                              .Resolve<IBusyIndicatorService>()
                              .Close();

                        // Log the running of the proof to the Log web service
                        ApplicationViewModel.Instance.Log.RunSimulation();
                    }
                }
                catch (Exception ex)
                {
                    OnSimulationError(ex);
                }
            }
        }

        private void OnSimulationError(Exception ex)
        {
            ApplicationViewModel.Instance.Container
                        .Resolve<IBusyIndicatorService>()
                        .Close();

            var details = ex.ToString();
            if (ex.InnerException != null)
            {
                details = ex.InnerException.ToString();
            }

            ApplicationViewModel.Instance.Container
                  .Resolve<IErrorWindowService>()
                  .Show("There was an error getting the simulation results.", details);

            // Log the error to the Log web service
            ApplicationViewModel.Instance.Log.Error("There was an error running the simulation.", details);
        }

        public DelegateCommand Plus10StepsCommand
        {
            get { return this.plus10StepsCommand; }
        }

        private void OnPlus10StepsExecuted()
        {
            SimulationViewModelFactory.Plus10Steps(this);
            this.NumberOfSteps += 10;

            if (currentStep > 0)
            {
                // Create the analyzer client
                if (analyzerClient == null)
                {
                    var serviceUri = new Uri("../Services/AnalysisService.svc", UriKind.Relative);
                    var endpoint = new EndpointAddress(serviceUri);
                    analyzerClient = new AnalysisServiceClient("AnalysisServiceCustom", endpoint);
                    analyzerClient.SimulateCompleted += OnSimulateCompleted;
                }

                ApplicationViewModel.Instance.Container
               .Resolve<IBusyIndicatorService>()
               .Show("Running simulation...", CancelProofCommand);

                // Update the SimInput for the last run
                simInput = SimulationInputFactory.Update(this, simInput);
                analyzerClient.SimulateAsync(simInput);
            }
        }

        public DelegateCommand Minus10StepsCommand
        {
            get { return this.minus10StepsCommand; }
        }

        private void OnMinus10StepsExecuted()
        {
            if (this.NumberOfSteps > 10)
            {
                if (this.currentStep > 0)
                {
                    SimulationViewModelFactory.Minus10Steps(this);
                    this.currentStep = this.numberOfSteps - 10;
                }
                this.NumberOfSteps -= 10;
            }
        }

        public int CurrentStep
        {
            get { return this.currentStep; }
            set
            {
                if (this.currentStep != value)
                {
                    this.currentStep = value;
                    OnPropertyChanged(() => CurrentStep);
                }
            }
        }

        public int NumberOfSteps
        {
            get { return this.numberOfSteps; }
            set
            {
                if (this.numberOfSteps != value)
                {
                    this.numberOfSteps = value;
                    OnPropertyChanged(() => NumberOfSteps);
                }
            }
        }


        public DelegateCommand RandomizeCommand
        {
            get { return this.randomizeCommand; }
        }

        private void OnRandomizedExecuted()
        {
            foreach (var variableVM in variables)
            {
                variableVM.RandomiseValue();
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Variables"/> property.
        /// </summary>
        public List<VariableSimViewModel> Variables
        {
            get { return this.variables; }
            set
            {
                if (this.variables != value)
                {
                    this.variables = value;
                    OnPropertyChanged(() => Variables);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="ModelName"/> property.
        /// </summary>
        public string ModelName
        {
            get { return this.modelName; }
            set
            {
                if (this.modelName != value)
                {
                    this.modelName = value;
                    OnPropertyChanged(() => ModelName);
                }
            }
        }


        /// <summary>
        /// Gets the value of the <see cref="CloseCommand"/> property.
        /// </summary>
        public DelegateCommand CloseCommand
        {
            get { return this.closeCommand; }
        }

        private void OnCloseExecuted()
        {
            ApplicationViewModel.Instance.Container
                .Resolve<IGraphWindowService>()
                .Close();

            ApplicationViewModel.Instance.Container
                .Resolve<ISimulationWindowService>()
                .Close();
        }

        public DelegateCommand ShowGraphCommand
        {
            get { return this.showGraphCommand; }
        }

        public DelegateCommand GraphAllCommand
        {
            get { return this.graphAllCommand; }
        }

        private void OnShowGraphExecuted()
        {
            // If no variables are selected to graph, first select all of them.
            if (variables.Where(v => v.IsGraphed).Count() == 0)
            {
                OnGraphAllExecuted();
            }

            var graphVM = GraphFactory.Create(this);

            ApplicationViewModel.Instance.Container
                     .Resolve<IGraphWindowService>().Show(graphVM);
        }

        private void OnGraphAllExecuted()
        {
            foreach (var variableVM in variables)
            {
                variableVM.IsGraphed = true;
                variableVM.ToggleGraphedCommand.Execute();
            }

            OnShowGraphExecuted();
        }

        public DelegateCommand CancelProofCommand
        {
            get { return this.cancelProofCommand; }
        }

        bool hasClosed;

        private void OnCancelProofExecuted()
        {
            hasClosed = true;

            if (analyzerClient != null)
            {
                analyzerClient.SimulateCompleted -= OnSimulateCompleted;
                analyzerClient = null;
            }

            CurrentStep = 0;

            // Reset the current view whilst it's loading
            // Mark all the variables as being modifiable now
            this.variables.ForEach(v =>
            {
                v.ModifiableValue();
                v.Steps.Clear();
            });

            ApplicationViewModel.Instance.Container
           .Resolve<IBusyIndicatorService>()
           .Close();
        }

    }
}