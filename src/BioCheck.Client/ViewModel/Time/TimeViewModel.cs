using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using BioCheck.AnalysisService;
using BioCheck.Services;
using BioCheck.Views; 
using BioCheck.ViewModel.Simulation; // For running the sim window.____
using BioCheck.ViewModel.Cells;
using BioCheck.ViewModel.Proof;
using BioCheck.ViewModel.Factories;
using MvvmFx.Common.ViewModels.Behaviors.Observable;
using MvvmFx.Common.ViewModels.Commands;
using Microsoft.Practices.Unity;
// using FurtherTestingOutputDTO = BioCheck.AnalysisService.FurtherTestingOutputDTO;


// Time edit
namespace BioCheck.ViewModel.Time
{
    public class TimeViewModel : ObservableViewModel
    {
        private readonly DelegateCommand runCommand;
        private readonly DelegateCommand closeCommand;
        private readonly DelegateCommand cancelTimeCommand;
        private readonly DelegateCommand consoleCommand;
        private readonly DelegateCommand runSimulation;
        private readonly DelegateCommand runSimulationCommand; // For simulation_____
        private readonly DelegateCommand runProve;

        private readonly TimebarViewModel timebarViewModel;

        private AnalysisServiceClient analyzerClient;
        private LTLViewState state = LTLViewState.None;

        private DateTime timer;
        private string modelName;
        private bool ltlProof = false;
        private int ltlPath = 100;
        private string ltlInput = "True";   
        private string ltlOutput;

        public TimeViewModel()
        {
            this.runCommand = new DelegateCommand(OnRunExecuted);
            this.closeCommand = new DelegateCommand(OnCloseExecuted);
            this.cancelTimeCommand = new DelegateCommand(OnCancelTimeExecuted);
            this.consoleCommand = new DelegateCommand(OnConsoleExecuted);
            //this.runSimulation = new DelegateCommand(OnRunSimulationExecuted);
            this.runSimulationCommand = new DelegateCommand(OnRunSimulationExecuted); // Simulation
            this.runSimulation = new DelegateCommand(OnNotProof);
            this.runProve = new DelegateCommand(OnRunProveExecuted);
            this.timebarViewModel = new TimebarViewModel();
        }             
   
        // New______

        // Running Simulation____
        public DelegateCommand RunSimulationCommand
        {
            get { return this.runSimulationCommand; }
        }
        private void OnRunSimulationExecuted()
        {
            if (!ApplicationViewModel.Instance.HasActiveModel)
            {
                return;
            }

            // Show a Cancellable Busy Indicator window
            ApplicationViewModel.Instance.Container
                    .Resolve<IBusyIndicatorService>()
                    .Show("Initialising simulation...");

            var modelVM = ApplicationViewModel.Instance.ActiveModel;

            if (timeOutput.Status == "True")
            {
                var simulationVM = SimulationViewModelFactory.Create(modelVM, timeOutput);
                ApplicationViewModel.Instance.Container
                     .Resolve<ISimulationWindowService>().Show(simulationVM);
            }
            else 
            {
                var simulationVM = SimulationViewModelFactory.Create(modelVM);
                ApplicationViewModel.Instance.Container
                     .Resolve<ISimulationWindowService>().Show(simulationVM);
            }

            ApplicationViewModel.Instance.Container
               .Resolve<IBusyIndicatorService>()
               .Close();
        }

        // Making a table: 
        private TimeViewModel timeVM;
        private List<VariableProofViewModel> variables;
        private List<ProgressionInfo> progressionInfos;
        private ProgressionInfo selectedProgressionInfo;
        private readonly AnalysisInputDTO input;
        private readonly TimeOutput output;
        private double time;
        private int steps;

        public TimeViewModel(AnalysisInputDTO input, TimeOutput output)
        {
            this.input = input;
            this.output = output;
            //this.time = output.Time;
            //this.steps = output.Ticks.Count;
            this.variables = new List<VariableProofViewModel>();
            this.progressionInfos = new List<ProgressionInfo>();
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Time"/> property.
        /// </summary>
        public double Time
        {
            get { return this.time; }
            set
            {
                if (this.time != value)
                {
                    this.time = value;
                    OnPropertyChanged(() => Time);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Steps"/> property.
        /// </summary>
        public int Steps
        {
            get { return this.steps; }
            set
            {
                if (this.steps != value)
                {
                    this.steps = value;
                    OnPropertyChanged(() => Steps);
                }
            }
        }
        public List<VariableProofViewModel> Variables
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

        public DelegateCommand RunCommand
        {
            get { return this.runCommand; }
        }

        public DelegateCommand LTLSimulation
        {
            get { return this.runSimulation; }
        }

        public DelegateCommand LTLProve
        {
            get { return this.runProve; }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="State"/> property.
        /// </summary>
        //_________
        public LTLViewState State
        {
            get { return this.state; }
            set
            {
                if (this.state != value)
                {
                    this.state = value;
                    OnPropertyChanged(() => State);
                    //OnPropertyChanged(() => IsStable);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="TimeInfos"/> property.
        /// </summary>
        public List<ProgressionInfo> TimeInfos
        {
            get { return this.progressionInfos; }
            set
            {
                if (this.progressionInfos != value)
                {
                    this.progressionInfos = value;
                    OnPropertyChanged(() => TimeInfos);
                }
            }
        }


        // For draggable buttons.
        public TimebarViewModel TimebarViewModel
        {
            get { return this.timebarViewModel; }              
        }

        private AnalysisInputDTO timeInputDto;

        // Analysis start: Called when user pushes "TEST"
        private void OnRunExecuted()
        {
            // Sanity check: Check that the model is active (early on at startup, it's not active)
            // And that there is a model loaded at all.
            if (!ApplicationViewModel.Instance.HasActiveModel)
            {
                // New
                this.State = LTLViewState.None;
                ApplicationViewModel.Instance.Container
                 .Resolve<IMessageWindowService>()
                 .Show("There is no active model to test your formulas on. Please load a model to continue.");
                return;
            }

            // Counter lack of formula input
            if (this.LTLInput == null || this.LTLInput == "")
            {
                this.State = LTLViewState.Error;
                this.LTLOutput = "You wrote no formula.\nPlease write a formula to test your loaded model against, and try again.";
            }
            else
            {
                string currFormula = this.LTLInput;
                string noWhite = currFormula.Trim();    // Strip whitespace

                if (noWhite == "")
                {
                    this.State = LTLViewState.Error;
                    this.LTLOutput = "You wrote no formula.\nPlease write a formula to test your loaded model against, and try again.";
                    this.LTLInput = "";                 // Remove any whitespace entered by the user in the formula input.
                }
                else 
                { 
                    // Check the formula for unbalanced N of brackets
                    int frontBracketCount = 0;
                    int backBracketCount = 0;

                    for (int i = 0; i < noWhite.Length; i++)
                    {
                        if (noWhite[i] == '(')
                        {
                            frontBracketCount++;
                        }
                        else if (noWhite[i] == ')')
                        {
                            backBracketCount++;
                        }
                    }

                    if (frontBracketCount == backBracketCount)
                    {
                        ApplicationViewModel.Instance.Container
                             .Resolve<IBusyIndicatorService>()
                             .Show("Running temporal proof...", CancelTimeCommand);

                        var modelVM = ApplicationViewModel.Instance.ActiveModel;
                        timeInputDto = TimeInputDTOFactory.Create(modelVM, this);

                        // Enable/Disable logging
                        timeInputDto.EnableLogging = ApplicationViewModel.Instance.ToolbarViewModel.EnableAnalyzerLogging;

                        // Create the analyzer client
                        if (analyzerClient == null)
                        {
                            var serviceUri = new Uri("../Services/AnalysisService.svc", UriKind.Relative);
                            var endpoint = new EndpointAddress(serviceUri);
                            analyzerClient = new AnalysisServiceClient("AnalysisServiceCustom", endpoint);
                            analyzerClient.AnalyzeCompleted += OnTimeCompleted;
                        }

                        timer = DateTime.Now;
                        // Invoke the async Analyze method on the service
                        // Result in an AnalysisService.svc.cs input with an LTL signature.
                        analyzerClient.AnalyzeAsync(timeInputDto);    
                    }
                    else 
                    {
                        this.State = LTLViewState.Error;
                        this.LTLOutput = "Your input formula contains an unbalanced number of brackets.\nPlease correct this and try again.";
                    }
                }
            }
        }

        private TimeOutput timeOutput;
        private void OnTimeCompleted(object sender, AnalyzeCompletedEventArgs e)           // This is an object in Reference.cs!
        {
            if (e.Error != null)
            {
                this.State = LTLViewState.Error;
                this.LTLOutput = "There was an error running your LTL query! Revisit your formula and try again.";
                OnTimeError(e.Error);
            }
            else
            {
                try
                {
                    // Unzip: Get the resulting dictionary of variables and their values
                    this.timeOutput = TimeOutputFactory.Create(e.Result);
                    //this.timeVM = TimeViewModelFactory.Create(timeInputDto, timeOutput);
                    var modelVM = ApplicationViewModel.Instance.ActiveModel;
                    this.ModelName = modelVM.Name;
                    if (timeOutput.Status == "Error")
                    {
                        this.State = LTLViewState.Error;
                        this.LTLOutput = "There is an error in your LTL formula! Please check it.\n\nAre all keyframes' rules complete?\nIndividual rules should be in the form:\n\"Variable - Operator - Value\"\n\"Value - Operator - Variable\"\nor\n\"Value - Operator - Variable - Operator - Value\"";
                    }
                    else
                    {                        
                        string finalOutput;

                        bool correct = bool.Parse(timeOutput.Status); // Using the fact that it's True or False
                        if (correct)
                        {
                            this.State = LTLViewState.Simulation;
                            finalOutput = "There is a simulation, of maximum length ";
                            finalOutput += this.ltlPath;

                            if (this.ltlProof)
                            {
                                finalOutput += " steps, that does not satisfy your formula for the current model, ";
                                finalOutput += this.ModelName;
                                finalOutput += ".";
                                finalOutput += "\n\nTo view an example of a simulation where the formula is not satisfied, push the button to run a simulation.\n";
                            }
                            else
                            {
                                finalOutput += " steps, that satisifies your formula for the current model, ";
                                finalOutput += this.ModelName;
                                finalOutput += ".\n\nTo view an example of a simulation where the formula is satisfied, push the button to run a simulation.\n";
                            }
                        }
                        else
                        {
                            this.State = LTLViewState.NoSimulation;
                            if (this.ltlProof)
                            {
                                finalOutput = "All possible simulations of maximum length ";
                                finalOutput += this.ltlPath;
                                finalOutput += " satisifies the above formula for the current model, ";
                                finalOutput += this.ModelName;
                                finalOutput += ".";
                            }
                            else
                            {
                                finalOutput = "No possible simulation of maximum length ";
                                finalOutput += this.ltlPath;
                                finalOutput += " steps exists that satisifies the above formula for the current model, ";
                                finalOutput += this.ModelName;
                                finalOutput += ".";
                            }
                        }
                        this.LTLOutput = finalOutput;
                    }

                    // If we've reached the total number of steps, then close the busy dialog
                    ApplicationViewModel.Instance.Container
                            .Resolve<IBusyIndicatorService>()
                            .Close();

                    // Log the running of the proof to the Log web service
                    ApplicationViewModel.Instance.Log.RunSimulation();
                }
                catch (Exception ex)
                {
                    OnTimeError(ex);
                }
            }
        }

        private void OnTimeError(Exception ex)
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
                  .Show("There was an error getting the temporal proof results.", details);

            // Log the error to the Log web service
            ApplicationViewModel.Instance.Log.Error("There was an error running the temporal proof.", details);
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

        // Function AND variable. It has get, so refers to a variable, but may be set.
        public int LTLPath
        {
            get { return this.ltlPath; }
            set
            {
                if (this.ltlPath != value)
                {
                    this.ltlPath = value;
                    OnPropertyChanged(() => LTLPath);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="LTLInput"/> property.
        /// </summary>
        public string LTLInput
        {
            get { return this.ltlInput; }
            set
            {
                if (this.ltlInput != value)
                {
                    this.ltlInput = value;
                    OnPropertyChanged(() => LTLInput);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="LTLOutput"/> property.
        /// </summary>
        public string LTLOutput
        {
            get { return this.ltlOutput; }
            set
            {
                if (this.ltlOutput != value)
                {
                    this.ltlOutput = value;
                    OnPropertyChanged(() => LTLOutput);
                }
            }
        }

        public bool LTLProof
        {
            get { return this.ltlProof; }
            set
            {
                //if (this.ltlProof != value)
                //{
                this.ltlProof = value;          // Should never be set here. Set by check-distinct methods.
                //    OnPropertyChanged(() => LTLProof);
                //}
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
                .Resolve<ITimeWindowService>()
                .Close();
        }


        public DelegateCommand CancelTimeCommand
        {
            get { return this.cancelTimeCommand; }
        }


        public DelegateCommand ConsoleCommand
        {
            get { return this.consoleCommand; }
        }

        bool hasClosed;

        private void OnCancelTimeExecuted()
        {
            hasClosed = true;

            if (analyzerClient != null)
            {
                //analyzerClient.TimeCompleted -= OnTimeCompleted;
                //analyzerClient = null;
            }

            ApplicationViewModel.Instance.Container
           .Resolve<IBusyIndicatorService>()
           .Close();
        }

        // Unfortunately, the console can't be called from Silverlight or the Azure emulator.
        // Use http://Monacotool in the HTML5 version.
        private void OnConsoleExecuted()
        {
            System.Console.WriteLine("BMA says hello world! What's your name?");
            string meName = System.Console.ReadLine();
            System.Console.WriteLine("Hello {0}. Now get modelling!", meName);

        }

        private void OnNotProof()
        {
            this.ltlProof = false;
        }

        private void OnRunProveExecuted()
        {
            this.ltlProof = true;
        }
    }
}
