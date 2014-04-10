using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using BioCheck.AnalysisService;
using BioCheck.Helpers;
using BioCheck.Services;
using BioCheck.ViewModel.Cells;
using BioCheck.ViewModel.Proof;
using BioCheck.ViewModel.Factories;
using MvvmFx.Common.ViewModels.Behaviors.Observable;
using MvvmFx.Common.ViewModels.Commands;
using Microsoft.Practices.Unity;
// using FurtherTestingOutputDTO = BioCheck.AnalysisService.FurtherTestingOutputDTO;


namespace BioCheck.ViewModel.SCM
{
    public class SCMViewModel : ObservableViewModel
    {
        //private ProofViewState state;           // Of namespace BioCheck.ViewModel.Proof

        private readonly DelegateCommand runCommand;
        private readonly DelegateCommand closeCommand;
        private readonly DelegateCommand cancelSCMCommand;

        private AnalysisServiceClient analyzerClient;

        private DateTime timer;
        private string modelName;
        private string scmOutput;

        private SCMViewModel scmVM;
        private ProofViewModel proofVM;

        public SCMViewModel()
        {
            this.runCommand = new DelegateCommand(OnRunExecuted);
            this.closeCommand = new DelegateCommand(OnCloseExecuted);
            this.cancelSCMCommand = new DelegateCommand(OnCancelSCMExecuted);
        }
                
        public DelegateCommand RunCommand
        {
            get { return this.runCommand; }
        }

        private AnalysisInputDTO scmInputDto;

        // Analysis start: Called when user pushes "TEST"
        private void OnRunExecuted()
        {
            // Sanity check: Check that the model is active (early on at startup, it's not active)
            // And that there is a model loaded at all.
            if (!ApplicationViewModel.Instance.HasActiveModel)
            {
                // New
                ApplicationViewModel.Instance.Container
                 .Resolve<IMessageWindowService>()
                 .Show("There is no active model to test your formulas on. Please load a model to continue.");
                return;
            }

            ApplicationViewModel.Instance.Container
                    .Resolve<IBusyIndicatorService>()
                    .Show("Running SCM ...", CancelSCMCommand);

            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            scmInputDto = SCMInputDTOFactory.Create(modelVM); // Edits engine to SCM __ eventually

            // Enable/Disable logging
            scmInputDto.EnableLogging = ApplicationViewModel.Instance.ToolbarViewModel.EnableAnalyzerLogging;

            // Create the analyzer client
            if (analyzerClient == null)
            {
                var serviceUri = new Uri("../Services/AnalysisService.svc", UriKind.Relative);
                var endpoint = new EndpointAddress(serviceUri);
                analyzerClient = new AnalysisServiceClient("AnalysisServiceCustom", endpoint);
                analyzerClient.AnalyzeCompleted += OnSCMCompleted_VM;
            }

            // Invoke the async Analyze method on the service
            timer = DateTime.Now;
            analyzerClient.AnalyzeAsync(scmInputDto);                // Result in an AnalysisService.svc.cs input with a Proof signature.
              
        }

        //private SCMOutput scmOutput;
        private AnalysisInputDTO analysisInputDto;
        private AnalysisOutput analysisOutput;
        private void OnSCMCompleted_VM(object sender, AnalyzeCompletedEventArgs e)           // This is an object in Reference.cs!
        {

            var time = Math.Round((DateTime.Now - timer).TotalSeconds, 1);
            string timeString = (string.Format("The Shrink-Cut-Merge algorithm took {0} seconds to run.", time));
            Debug.WriteLine(string.Format("The Shrink-Cut-Merge algorithm took {0} seconds to run.", time));

            string finalOutput;
            finalOutput = timeString;    // Testing..
            string unzippedDetails = "";

            if (e.Error == null)
            {
                try
                {
                    this.analysisOutput = AnalysisOutputFactory.Create(e.Result);   // <---- Server output 

                    // Retrieve the Details
                    string[] varName = this.analysisOutput.Status.Split(' ');
                    
                    this.analysisOutput.Status = varName[0];
                    int counter = 0;
                    foreach(string detail in varName)
                    {
                        if (counter != 0)
                        {
                            unzippedDetails += detail + " ";
                        }
                        counter++;
                    }                    
                }
                catch (Exception ex)
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
                          .Show("There was an error running the analysis.", details);

                    // Log the error to the Log web service
                    ApplicationViewModel.Instance.Log.Error("There was an error running the analysis.", details);

                    return;
                }

                if (analysisOutput.Status == "UnknownResult")
                {
                    // Clear the current proof
                    ResetStability(false);

                    ApplicationViewModel.Instance.Container
                              .Resolve<IBusyIndicatorService>()
                              .Close();

                    string details = BuildAnalyisErrrorMessage(this.analysisOutput);

                    ApplicationViewModel.Instance.Container
                         .Resolve<IErrorWindowService>()
                         .ShowAnalysisError(details);

                    // Log the error to the Log web service
                    ApplicationViewModel.Instance.Log.Error("There was an error running the analysis.", details);
                }
                else
                {
                    //  WORKED! 
                    // Clear the current proof
                    ResetStability(true);

                    // Process the analysis output results
                    AnalysisOutputHandler.Handle(analysisOutput);

                    ApplicationViewModel.Instance.Container
                        .Resolve<IBusyIndicatorService>()
                        .Close();

                    // Edit the Proof view
                    this.proofVM = SCMViewModelFactory_ServerOutput.Create(scmInputDto, analysisOutput);
                    
                    //finalOutput += "\nThe model " + this.proofVM.ModelName + " did ";
                    //if (this.proofVM.IsStable)
                    //{
                    //    finalOutput += "stabilize.\n";
                    //}
                    //else
                    //{
                    //    finalOutput += "not stabilize.\n";
                    //}

                    finalOutput += "\nThe model \"" + this.proofVM.ModelName + "\" ";
                    switch (analysisOutput.Status)
                    {
                        case "SingleStablePoint":
                            finalOutput += "reached a single stable fixpoint.";
                            break;
                        case "MultiStablePoints":
                            finalOutput += "reached multiple stable fixpoints.";
                            break;
                        case "Cycle":
                            finalOutput += "reached a cycle.";
                            break;
                        case "UnknownResult":
                            finalOutput += "failed to produce a classifiable final state.\nSomething went wrong.";
                            break;
                    }

                    finalOutput += "\n\nDetails:\n" + unzippedDetails;

                    // Log the running of the proof to the Log web service
                    ApplicationViewModel.Instance.Log.RunProof();
                }
            }
            else
            {
                // Clear the current proof
                ResetStability(false);

                ApplicationViewModel.Instance.Container
                             .Resolve<IBusyIndicatorService>()
                             .Close();

                string details = e.Error.ToString();
                details = details + Environment.NewLine + e.Error.StackTrace;

                if (analysisOutput != null)
                {
                    details += Environment.NewLine;
                    details += Environment.NewLine;

                    details += BuildAnalyisMessage(analysisOutput);
                }

                ApplicationViewModel.Instance.Container
                             .Resolve<IErrorWindowService>()
                             .Show("There was an error running the analysis.", details);

                // Log the error to the Log web service
                ApplicationViewModel.Instance.Log.Error("There was an error running the analysis.", details);
            }
            this.SCMOutput = finalOutput;
        }

        public void ResetStability(bool showStability)
        {
            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            if (modelVM == null)
                return;

            foreach (var variableVM in modelVM.VariableViewModels)
            {
                variableVM.IsStable = true;
                variableVM.StabilityValue = "";
                variableVM.ShowStability = showStability;
            }

            foreach (var containerVM in modelVM.ContainerViewModels)
            {
                containerVM.IsStable = true;
                containerVM.ShowStability = showStability;

                foreach (var variableVM in containerVM.VariableViewModels)
                {
                    variableVM.IsStable = true;
                    variableVM.StabilityValue = "";
                    variableVM.ShowStability = showStability;
                }
            }
        }
        private string BuildAnalyisErrrorMessage(AnalysisOutput output)
        {
            string details = analysisOutput.Error;

            details += Environment.NewLine;
            details += Environment.NewLine;

            details += BuildAnalyisMessage(output);
            return details;
        }
        private string BuildAnalyisMessage(AnalysisOutput output)
        {
            string details = "";

            if (output.ErrorMessages != null && output.ErrorMessages.Count() > 0)
            {
                details = "Error Messages:";
                details += Environment.NewLine;

                var log = String.Join(Environment.NewLine, output.ErrorMessages);
                details = details + Environment.NewLine + log;

                details += Environment.NewLine;
            }

            if (output.Dto.ZippedLog != null)
            {
                var debug = ZipHelper.Unzip(output.Dto.ZippedLog);

                details += Environment.NewLine;
                details += "Debug Messages:";
                details += Environment.NewLine;

                details = details + Environment.NewLine + debug;
            }

            if (proofVM != null && proofVM.FurtherTestingOutput != null)
            {
                details += Environment.NewLine;
                details += Environment.NewLine;

                if (proofVM.FurtherTestingOutput.ErrorMessages != null &&
                    proofVM.FurtherTestingOutput.ErrorMessages.Count > 0)
                {
                    details += "Further Testing Error Messages:";
                    details = details + Environment.NewLine + String.Join(Environment.NewLine, proofVM.FurtherTestingOutput.ErrorMessages);

                    details += Environment.NewLine;
                    details += Environment.NewLine;
                }

                details += "Further Testing Debug Messages:";
                details += Environment.NewLine;

                var cexOutputDto = proofVM.FurtherTestingOutput.Dto;
                var cexLog = ZipHelper.Unzip(cexOutputDto.ZippedLog);

                details = details + Environment.NewLine + cexLog;
            }
            return details;
        }

        private void OnSCMError(Exception ex)
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


        /// <summary>
        /// Gets or sets the value of the <see cref="SCMOutput"/> property.
        /// </summary>
        public string SCMOutput
        {
            get { return this.scmOutput; }
            set
            {
                if (this.scmOutput != value)
                {
                    this.scmOutput = value;
                    OnPropertyChanged(() => SCMOutput);
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
                .Resolve<ISCMWindowService>()
                .Close();
        }


        public DelegateCommand CancelSCMCommand
        {
            get { return this.cancelSCMCommand; }
        }

        bool hasClosed;

        private void OnCancelSCMExecuted()
        {
            hasClosed = true;

            if (analyzerClient != null)
            {
                //analyzerClient.SCMCompleted -= OnSCMCompleted;
                //analyzerClient = null;
            }

            ApplicationViewModel.Instance.Container
           .Resolve<IBusyIndicatorService>()
           .Close();
        }
    }
}
