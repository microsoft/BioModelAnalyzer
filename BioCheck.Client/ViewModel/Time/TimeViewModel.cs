using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using BioCheck.AnalysisService;
using BioCheck.Services;
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
        //private ProofViewState state;           // Of namespace BioCheck.ViewModel.Proof

        private readonly DelegateCommand runCommand;
        private readonly DelegateCommand closeCommand;
        private readonly DelegateCommand cancelTimeCommand;
        private readonly DelegateCommand consoleCommand;
        private readonly DelegateCommand runSimulation;
        private readonly DelegateCommand runProve;
   

        private AnalysisServiceClient analyzerClient;

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
            this.runSimulation = new DelegateCommand(OnRunSimulationExecuted);
            this.runProve = new DelegateCommand(OnRunProveExecuted);
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


        private AnalysisInputDTO timeInputDto;

        private void OnRunExecuted()
        {
            // Sanity check: Check that the model is active (early on at startup, it's not active)
            if (!ApplicationViewModel.Instance.HasActiveModel)
            {
                return;
            }

            // Counter lack of input
            if (this.LTLInput == null || this.LTLInput == "")
            {
                this.LTLOutput = "\nYou wrote no formula.\nPlease write a formula to test your loaded model against, and try again.";
            }
            else
            {
                string currFormula = this.LTLInput;
                string noWhite = currFormula.Trim();    // Strip whitespace

                if (noWhite == "")
                {
                    this.LTLOutput = "\nYou wrote no formula.\nPlease write a formula to test your loaded model against, and try again.";
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

                        // Invoke the async Analyze method on the service
                        timer = DateTime.Now;
                        analyzerClient.AnalyzeAsync(timeInputDto);                // Result in an AnalysisService.svc.cs input with a Proof signature.
                    }
                    else 
                    {
                        this.LTLOutput = "\nYour input formula contains an unbalanced number of brackets.\nPlease correct this and try again.";
                    }
                }
            }
        }

        private TimeOutput timeOutput;
        private void OnTimeCompleted(object sender, AnalyzeCompletedEventArgs e)           // This is an object in Reference.cs!
        {
            
            if (e.Error != null)
            {
                OnTimeError(e.Error);
            }
            else
            {
                try
                { 
                     
                    // Get the resulting dictionary of variables and their values
                    this.timeOutput = TimeOutputFactory.Create(e.Result);

                    string finalOutput;
                    if (timeOutput.Status != "Error")
                    {
                        bool correct = bool.Parse(timeOutput.Status);
                        if (correct)
                        {
                            if (this.ltlProof)
                            {
                                //finalOutput = "\nTRUE: All possible simulations of maximum length ";
                                //finalOutput += this.ltlPath;
                                //finalOutput += " satisifies the above formula for the current model, ";
                                //finalOutput += this.ModelName;
                                //finalOutput += ".";

                                finalOutput = "\nFALSE: In all possible simulations of maximum length ";
                                finalOutput += this.ltlPath;
                                finalOutput += " , there are states where the above formula is not satisfied for the current model, ";
                                finalOutput += this.ModelName;
                                finalOutput += ".";
                            }
                            else { 
                                finalOutput = "\nTRUE: There is a simulation that satisifies the above formula for the current model, ";
                                finalOutput += this.ModelName;
                                finalOutput += ".\n\nThe simulation output:\n";
                                finalOutput += timeOutput.Model;
                            }
                        }
                        else 
                        {
                            if (this.ltlProof)
                            {
                                //finalOutput = "\nFALSE: No possible simulation exists of maximum length ";
                                //finalOutput += this.ltlPath;
                                //finalOutput += " that satisifies the above formula for the current model, ";
                                //finalOutput += this.ModelName;
                                //finalOutput += ".";

                                finalOutput = "\nTRUE: All possible simulations of maximum length ";
                                finalOutput += this.ltlPath;
                                finalOutput += " satisifies the above formula for the current model, ";
                                finalOutput += this.ModelName;
                                finalOutput += ".";
                            }
                            else
                            {
                                finalOutput = "\nFALSE: No possible simulation exists of maximum length ";
                                finalOutput += this.ltlPath;
                                finalOutput += " that satisifies the above formula for the current model, ";
                                finalOutput += this.ModelName;
                                finalOutput += ".";
                            }
                        }
                    } 
                    else 
                    {
                        finalOutput = "\nSomething went wrong. Are you sure that the formula above is correct?\nPlease note that variable names are case-sensitive.";
                    }
                    
                    this.LTLOutput = finalOutput;

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

        private void OnRunSimulationExecuted()
        {
            this.ltlProof = false;
        }

        private void OnRunProveExecuted()
        {
            this.ltlProof = true;
        }
    }
}
