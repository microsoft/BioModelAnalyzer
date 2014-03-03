using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.ServiceModel;
using System.Text;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using BioCheck.AnalysisService;
using BioCheck.Helpers;
using BioCheck.Services;
using BioCheck.ViewModel.Simulation;
using BioCheck.ViewModel.Time;                  //Time edit
using BioCheck.ViewModel.XML;
using BioCheck.ViewModel.Models;
using BioCheck.ViewModel.Proof;
using Microsoft.Practices.Unity;
using MvvmFx.Common.Helpers;
using MvvmFx.Common.ViewModels.Behaviors.LoadingSaving;
using MvvmFx.Common.ViewModels.Behaviors.Observable;
using MvvmFx.Common.ViewModels.Commands;

namespace BioCheck.ViewModel
{
    /// <summary>
    /// ViewModel for the timebar
    /// This should track what's dragged and update the derived rules.
    /// </summary>
    /// 

    public class TimebarViewModel : ObservableViewModel
    {
        #region Command fields
        private readonly DelegateCommand saveCommand;
        private readonly DelegateCommand clearCommand;
        private readonly DelegateCommand deleteCommand;
        private readonly DelegateCommand runProofCommand;
        private readonly DelegateCommand runTimeCommand;    //Time edit
        private readonly DelegateCommand runSimulationCommand;
        private readonly DelegateCommand clearProofCommand;
        private readonly DelegateCommand cancelProofCommand;
        private readonly DelegateCommand resetLibraryCommand;
        private readonly DelegateCommand cutCommand;
        private readonly DelegateCommand copyCommand;
        private readonly DelegateCommand pasteCommand;
        private readonly DelegateCommand undoCommand;
        private readonly DelegateCommand redoCommand;
        private readonly DelegateCommand logVisualTreeCommand;
        private readonly DelegateCommand gcCollectCommand;
        private readonly DelegateCommand toggleAnalyzerLoggingCommand;
        #endregion

        private bool isSelectionActive;
        private bool isActivatorActive;
        private bool isInhibitorActive;
        private bool showTimebar;
        private bool isVariableActive;
        private bool mouseDownIsHandled;

        private AnalysisServiceClient analyzerClient;
        private ProofViewModel proofVM;

        public TimebarViewModel()
        {
            this.saveCommand = new DelegateCommand(OnSaveExecuted);
            this.clearCommand = new DelegateCommand(OnClearExecuted);
            this.deleteCommand = new DelegateCommand(OnDeleteExecuted);
            this.runProofCommand = new DelegateCommand(OnRunProofExecuted);
            this.runTimeCommand = new DelegateCommand(OnRunTimeExecuted);       // Time edit
            this.runSimulationCommand = new DelegateCommand(OnRunSimulationExecuted);
            this.clearProofCommand = new DelegateCommand(OnClearProofExecuted);
            this.cancelProofCommand = new DelegateCommand(OnCancelProofExecuted);
            this.copyCommand = new DelegateCommand(OnCopyExecuted);
            this.pasteCommand = new DelegateCommand(OnPasteExecuted);
            this.cutCommand = new DelegateCommand(OnCutExecuted);
            // TODO: Seems MVVM FX doesn't process the can execute handler
            // automatically, and there is no information on how to use this
            // at all, so ignore that for the time being
            this.undoCommand = new DelegateCommand(OnUndoExecuted);//, OnCanExecuteUndo);
            this.redoCommand = new DelegateCommand(OnRedoExecuted);//, OnCanExecuteRedo);

            // Debug tools
            this.gcCollectCommand = new DelegateCommand(OnGCCollectExecuted);
            this.exportAnalysisInputCommand = new DelegateCommand(OnExportAnalysisInputExecuted);
            this.exportAnalysisOutputCommand = new DelegateCommand(OnExportAnalysisOutputExecuted);
            this.showAnalyzerLogCommand = new DelegateCommand(OnShowAnalyzerLogExecuted);
            this.showAnalysisInputCommand = new DelegateCommand(OnShowAnalysisInputExecuted);
            this.showAnalysisOutputCommand = new DelegateCommand(OnShowAnalysisOutputExecuted);
            this.showModelXmlCommand = new DelegateCommand(OnShowModelXmlExecuted);
            this.exportModelXmlCommand = new DelegateCommand(OnExportModelXmlExecuted);
            this.showErrorTraceCommand = new DelegateCommand(OnShowErrorTraceCommand);
            this.toggleAnalyzerLoggingCommand = new DelegateCommand(OnToggleAnalyzerLoggingExecuted);
            this.logVisualTreeCommand = new DelegateCommand(OnLogVisualTreeExecuted);

            // Default to the Selection tool
            this.isSelectionActive = true;
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="IsSelectionActive"/> property.
        /// </summary>
        public bool IsSelectionActive
        {
            get { return this.isSelectionActive; }
            set
            {
                if (this.isSelectionActive != value)
                {
                    ResetIsSelectionActive(value);
                    if (value)
                    {
                        ResetIsVariableActive(false);
                        ResetIsActivatorActive(false);
                        ResetIsInhibitorActive(false);

                        ApplicationViewModel.Instance
                            .Context.SelectionTool();
                    }
                }
            }
        }

        private void ResetIsSelectionActive(bool value)
        {
            this.isSelectionActive = value;
            OnPropertyChanged(() => IsSelectionActive);
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="IsVariableActive"/> property.
        /// </summary>
        public bool IsVariableActive
        {
            get { return this.isVariableActive; }
            set
            {
                if (this.isVariableActive != value)
                {
                    ResetIsVariableActive(value);
                    ResetIsSelectionActive(!value);
                    ResetIsActivatorActive(false);
                    ResetIsInhibitorActive(false);

                    if (value)
                    {
                        ApplicationViewModel.Instance
                        .Context.PaintVariableTool();
                    }
                }
            }
        }

        private void ResetIsVariableActive(bool value)
        {
            this.isVariableActive = value;
            OnPropertyChanged(() => IsVariableActive);
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="MouseDownIsHandled"/> property.
        /// </summary>
        public bool MouseDownIsHandled
        {
            get { return this.mouseDownIsHandled; }
            set
            {
                if (this.mouseDownIsHandled != value)
                {
                    this.mouseDownIsHandled = value;
                    OnPropertyChanged(() => MouseDownIsHandled);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="IsActivatorActive"/> property.
        /// </summary>
        public bool IsActivatorActive
        {
            get { return this.isActivatorActive; }
            set
            {
                if (this.isActivatorActive != value)
                {
                    ResetIsActivatorActive(value);
                    ResetIsVariableActive(false);
                    ResetIsSelectionActive(!value);
                    ResetIsInhibitorActive(false);

                    if (value)
                    {
                        ApplicationViewModel.Instance
                            .Context.SelectRelationshipTool();
                    }
                }
            }
        }

        private void ResetIsActivatorActive(bool value)
        {
            this.isActivatorActive = value;
            OnPropertyChanged(() => IsActivatorActive);
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="IsInhibitorActive"/> property.
        /// </summary>
        public bool IsInhibitorActive
        {
            get { return this.isInhibitorActive; }
            set
            {
                if (this.isInhibitorActive != value)
                {
                    ResetIsInhibitorActive(value);
                    ResetIsActivatorActive(false);
                    ResetIsVariableActive(false);
                    ResetIsSelectionActive(!value);

                    if (value)
                    {
                        ApplicationViewModel.Instance
                            .Context.SelectRelationshipTool();
                    }
                }
            }
        }

        private void ResetIsInhibitorActive(bool value)
        {
            this.isInhibitorActive = value;
            OnPropertyChanged(() => IsInhibitorActive);
        }

        /// <summary>
        /// Gets a value indicating whether either relationship tool is active.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is relationship active; otherwise, <c>false</c>.
        /// </value>
        public bool IsRelationshipActive
        {
            get { return (this.isActivatorActive || this.isInhibitorActive); }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="ShowTimebar"/> property.
        /// </summary>
        public bool ShowTimebar
        {
            get { return this.showTimebar; }
            set
            {
                if (this.showTimebar != value)
                {
                    this.showTimebar = value;
                    OnPropertyChanged(() => ShowTimebar);
                }
            }
        }

        /// <summary>
        /// Gets the value of the <see cref="SaveCommand"/> property.
        /// </summary>
        public DelegateCommand SaveCommand
        {
            get { return this.saveCommand; }
        }

        private void OnSaveExecuted()
        {
            if (!ApplicationViewModel.Instance.HasActiveModel)
            {
                return;
            }

            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            modelVM.ModifiedDate = DateTime.Now;

            ApplicationViewModel.Instance.Container
                .Resolve<IViewModelSaver<ModelViewModel>>()
                .Save(modelVM);

            // Log the saving of the model to the Log web service
            ApplicationViewModel.Instance.Log.SaveModel();
        }

        /// <summary>
        /// Gets the value of the <see cref="ClearCommand"/> property.
        /// </summary>
        public DelegateCommand ClearCommand
        {
            get { return this.clearCommand; }
        }

        private void OnClearExecuted()
        {
            if (!ApplicationViewModel.Instance.HasActiveModel)
            {
                return;
            }

            // Prompt the user and clear the model if they confirm
            ApplicationViewModel.Instance.Container
                                        .Resolve<IMessageWindowService>()
                                        .Show("Are you sure you want to clear the current model? This will delete all the cells and variables.", MessageType.YesCancel, result =>
                                        {
                                            if (result == MessageResult.Yes)
                                            {
                                                ResetStability(false);

                                                ApplicationViewModel.Instance.DupActiveModel();
                                                var modelVM = ApplicationViewModel.Instance.ActiveModel;

                                                ApplicationViewModel.Instance.ActiveVariable = null;
                                                ApplicationViewModel.Instance.ActiveContainer = null;
                                                ApplicationViewModel.Instance.Container.Resolve<IProofWindowService>().Close();

                                                modelVM.Reset();
                                            }
                                        });
        }

        /// <summary>
        /// Gets the value of the <see cref="DeleteCommand"/> property.
        /// </summary>
        public DelegateCommand DeleteCommand
        {
            get { return this.deleteCommand; }
        }

        /// <summary>
        /// Called when the DeleteCommand is executed.
        /// Deletes any checked/selected container and variable.
        /// </summary>
        private void OnDeleteExecuted()
        {
            if (!ApplicationViewModel.Instance.HasActiveModel)
            {
                return;
            }

            // Get checked objects:
            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            int checkedRelationships = modelVM.RelationshipViewModels.Count(relationshipVM => relationshipVM.IsChecked);
            int checkedVariables = modelVM.ContainerViewModels.Sum(containerVM => containerVM.VariableViewModels.Count(variableVM => variableVM.IsChecked));
            int checkedConstants = modelVM.VariableViewModels.Count(variableVM => variableVM.IsChecked);
            int checkedContainers = modelVM.ContainerViewModels.Count(containerVM => containerVM.IsChecked);

            int checkedObjects = checkedRelationships + checkedVariables + checkedConstants + checkedContainers;

            // If there's no checked objects then return 
            if (checkedObjects == 0)
                return;


            // Prompt the user and delete the checked objects if they confirm

            string message =
                "Are you sure you want to delete the selected objects?" + Environment.NewLine + Environment.NewLine;

            if (checkedContainers == 1)
                message += "There is 1 selected container." + Environment.NewLine;
            else if (checkedContainers > 0)
                message += string.Format("There are {0} selected containers.", checkedContainers) + Environment.NewLine;

            if (checkedVariables == 1)
                message += "There is 1 selected variable." + Environment.NewLine;
            else if (checkedVariables > 0)
                message += string.Format("There are {0} selected variables.", checkedVariables) + Environment.NewLine;

            if (checkedConstants == 1)
                message += "There is 1 selected constant." + Environment.NewLine;
            else if (checkedConstants > 0)
                message += string.Format("There are {0} selected constants.", checkedConstants) + Environment.NewLine;

            if (checkedRelationships == 1)
                message += "There is 1 selected relationship." + Environment.NewLine;
            else if (checkedRelationships > 0)
                message += string.Format("There are {0} selected relationships.", checkedRelationships);

            ApplicationViewModel.Instance.Container
                                        .Resolve<IMessageWindowService>()
                                        .Show(message, MessageType.YesCancel, result =>
                                        {
                                            if (result == MessageResult.Yes)
                                            {
                                                ApplicationViewModel.Instance.DupActiveModel();
                                                modelVM.RelationshipViewModels.RemoveAll(relationshipVM => relationshipVM.IsChecked);

                                                foreach (var containerVM in modelVM.ContainerViewModels)
                                                {
                                                    containerVM.VariableViewModels.RemoveAll(variableVM => variableVM.IsChecked);
                                                }

                                                modelVM.VariableViewModels.RemoveAll(variableVM => variableVM.IsChecked);
                                                modelVM.ContainerViewModels.RemoveAll(containerVM => containerVM.IsChecked);

                                                // Reset the active variable and container and close the context menu
                                                ApplicationViewModel.Instance.ActiveVariable = null;
                                                ApplicationViewModel.Instance.ActiveContainer = null;
                                                ApplicationViewModel.Instance.Container.Resolve<IContextBarService>().Close();
                                            }
                                        });
        }

        public DelegateCommand UndoCommand
        {
            get { return this.undoCommand; }
        }

        private void OnUndoExecuted()
        {
            ApplicationViewModel.Instance.UndoActiveModel();
        }

        private bool OnCanExecuteUndo()
        {
            return ApplicationViewModel.Instance.CanUndoActiveModel;
        }

        public DelegateCommand RedoCommand
        {
            get { return this.redoCommand; }
        }

        private bool OnCanExecuteRedo()
        {
            return ApplicationViewModel.Instance.CanRedoActiveModel;
        }

        private void OnRedoExecuted()
        {
            ApplicationViewModel.Instance.RedoActiveModel();
        }

        #region Cut/Copy/Paste

        /// <summary>
        /// Gets the value of the <see cref="CutCommand"/> property.
        /// </summary>
        public DelegateCommand CutCommand
        {
            get { return this.cutCommand; }
        }

        private void OnCutExecuted()
        {
            if (!ApplicationViewModel.Instance.HasActiveModel)
            {
                return;
            }
            var modelVM = ApplicationViewModel.Instance.ActiveModel;

            if (ApplicationViewModel.Instance.HasActiveVariable)
            {
                ApplicationViewModel.Instance.ActiveVariable.CutCommand.Execute();
            }
            else
            {
                var activeContainers = modelVM.ContainerViewModels.Where(containerVM => containerVM.IsChecked).ToList();
                if (activeContainers.Count == 1)
                {
                    var activeContainer = activeContainers[0];
                    activeContainer.CopyCommand.Execute();
                }
                else if (activeContainers.Count > 1)
                {
                    ApplicationViewModel.Instance.Container
                    .Resolve<IMessageWindowService>()
                    .Show("Please select only one cell to cut.");
                }
            }
        }

        /// <summary>
        /// Gets the value of the <see cref="CopyCommand"/> property.
        /// </summary>
        public DelegateCommand CopyCommand
        {
            get { return this.copyCommand; }
        }

        private void OnCopyExecuted()
        {
            if (!ApplicationViewModel.Instance.HasActiveModel)
            {
                return;
            }
            var modelVM = ApplicationViewModel.Instance.ActiveModel;

            if (ApplicationViewModel.Instance.HasActiveVariable)
            {
                ApplicationViewModel.Instance.ActiveVariable.CopyCommand.Execute();
            }
            else
            {
                var activeContainers = modelVM.ContainerViewModels.Where(containerVM => containerVM.IsChecked).ToList();
                if (activeContainers.Count == 1)
                {
                    var activeContainer = activeContainers[0];
                    activeContainer.CopyCommand.Execute();
                }
                else if (activeContainers.Count > 1)
                {
                    ApplicationViewModel.Instance.Container
                    .Resolve<IMessageWindowService>()
                    .Show("Please select only one cell to copy.");
                }
            }
        }

        /// <summary>
        /// Gets the value of the <see cref="PasteCommand"/> property.
        /// </summary>
        public DelegateCommand PasteCommand
        {
            get { return this.pasteCommand; }
        }

        private void OnPasteExecuted()
        {
            if (!ApplicationViewModel.Instance.HasActiveModel)
                return;

            ApplicationViewModel.Instance.DupActiveModel();
            var modelVM = ApplicationViewModel.Instance.ActiveModel;

            if (ApplicationViewModel.Instance.HasActiveVariable)
            {
                ApplicationViewModel.Instance.ActiveVariable.PasteCommand.Execute();
            }
            else
            {
                var activeContainers = modelVM.ContainerViewModels.Where(containerVM => containerVM.IsChecked).ToList();
                if (activeContainers.Count == 1)
                {
                    var activeContainer = activeContainers[0];
                    activeContainer.PasteCommand.Execute();
                }
                else if (activeContainers.Count > 1)
                {
                    ApplicationViewModel.Instance.Container
                    .Resolve<IMessageWindowService>()
                    .Show("Please select only one cell to paste.");
                }
            }
        }

        #endregion

        #region Proof

        /// <summary>
        /// Gets the value of the <see cref="RunProofCommand"/> property.
        /// </summary>
        public DelegateCommand RunProofCommand
        {
            get { return this.runProofCommand; }
        }

        /// <summary>
        /// Gets the value of the <see cref="RunTimeCommand"/> property.
        /// </summary>
        public DelegateCommand RunTimeCommand
        {
            get { return this.runTimeCommand; }     // Time edit
        }

        public DelegateCommand RunSimulationCommand
        {
            get { return this.runSimulationCommand; }
        }

        /// <summary>
        /// Gets the value of the <see cref="ClearProofCommand"/> property.
        /// </summary>
        public DelegateCommand ClearProofCommand
        {
            get { return this.clearProofCommand; }
        }

        public DelegateCommand CancelProofCommand
        {
            get { return this.cancelProofCommand; }
        }

        private DateTime timer;

        private void OnRunProofExecuted()
        {
            if (!ApplicationViewModel.Instance.HasActiveModel)
            {
                return;
            }

            // Show a Cancellable Busy Indicator window
            ApplicationViewModel.Instance.Container
                    .Resolve<IBusyIndicatorService>()
                    .Show("Running proof...", CancelProofCommand);

            var modelVM = ApplicationViewModel.Instance.ActiveModel;

            // Create the Analysis Input Data from the active Model ViewModel
            analysisInputDto = AnalysisInputDTOFactory.Create(modelVM);

            // Enable/Disable logging
            analysisInputDto.EnableLogging = this.EnableAnalyzerLogging;

            // Create the analyzer client
            if (analyzerClient == null)
            {
                var serviceUri = new Uri("../Services/AnalysisService.svc", UriKind.Relative);
                var endpoint = new EndpointAddress(serviceUri);
                analyzerClient = new AnalysisServiceClient("AnalysisServiceCustom", endpoint);
                analyzerClient.AnalyzeCompleted += OnAnalysisCompleted;
            }

            // Invoke the async Analyze method on the service
            timer = DateTime.Now;
            analyzerClient.AnalyzeAsync(analysisInputDto);
        }

        // Time edit
        private void OnRunTimeExecuted()
        {
            if (!ApplicationViewModel.Instance.HasActiveModel)
            {
                return;
            }

            // Show a Cancellable Busy Indicator window
            ApplicationViewModel.Instance.Container
                    .Resolve<IBusyIndicatorService>()
                    .Show("Running temporal proof...", CancelProofCommand);

            //var modelVM = ApplicationViewModel.Instance.ActiveModel;

            /*
            // Create the Analysis Input Data from the active Model ViewModel
            analysisInputDto = AnalysisInputDTOFactory.Create(modelVM);

            // Enable/Disable logging
            analysisInputDto.EnableLogging = this.EnableAnalyzerLogging;

            // Create the analyzer client
            if (analyzerClient == null)
            {
                var serviceUri = new Uri("../Services/AnalysisService.svc", UriKind.Relative);
                var endpoint = new EndpointAddress(serviceUri);
                analyzerClient = new AnalysisServiceClient("AnalysisServiceCustom", endpoint);
                analyzerClient.AnalyzeCompleted += OnAnalysisCompleted;
            }

            // Invoke the async Analyze method on the service
            timer = DateTime.Now;
            analyzerClient.AnalyzeAsync(analysisInputDto);
            */
            timer = DateTime.Now;
            OnTimeProofCompleted();
        }


        private void OnCancelProofExecuted()
        {
            if (analyzerClient != null)
            {
                analyzerClient.AnalyzeCompleted -= OnAnalysisCompleted;
                analyzerClient = null;
            }

            ApplicationViewModel.Instance.Container
           .Resolve<IBusyIndicatorService>()
           .Close();
        }

        private void OnClearProofExecuted()
        {
            ResetStability(false);

            ApplicationViewModel.Instance.Container
               .Resolve<IProofWindowService>().
               Close();
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

        private AnalysisInputDTO analysisInputDto;
        private AnalysisOutput analysisOutput;

        private void OnAnalysisCompleted(object sender, AnalyzeCompletedEventArgs e)
        {

            var time = Math.Round((DateTime.Now - timer).TotalSeconds, 1);
            Debug.WriteLine(string.Format("Analyzer took {0} seconds to run.", time));

            // Error seems to handled below - why's this bit here?
            //if (e.Error != null)
            //{
            //    string details = e.Error.ToString();
            //    details = details + Environment.NewLine + e.Error.StackTrace;

            //    ApplicationViewModel.Instance.Container
            //             .Resolve<IErrorWindowService>()
            //             .Show("The Error property of AnalyzeCompletedEventArgs is not null.", details);
            //}

            if (e.Error == null)
            {
                try
                {
                    if (this.proofVM != null)
                        this.proofVM.ResetOutput();

                    this.analysisOutput = AnalysisOutputFactory.Create(e.Result);
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

                if (analysisOutput.Status == StatusTypes.Unknown || analysisOutput.Status == StatusTypes.Error)
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
                    // Clear the current proof
                    ResetStability(true);

                    // Process the analysis output results
                    AnalysisOutputHandler.Handle(analysisOutput);

                    ApplicationViewModel.Instance.Container
                        .Resolve<IBusyIndicatorService>()
                        .Close();

                    // Show the Proof view
                    this.proofVM = ProofViewModelFactory.Create(analysisInputDto, analysisOutput);

                    ApplicationViewModel.Instance.Container
                             .Resolve<IProofWindowService>().Show(proofVM);

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

            var simulationVM = SimulationViewModelFactory.Create(modelVM);

            ApplicationViewModel.Instance.Container
                     .Resolve<ISimulationWindowService>().Show(simulationVM);

            ApplicationViewModel.Instance.Container
               .Resolve<IBusyIndicatorService>()
               .Close();
        }

        /// <summary>
        /// Part of the LTL loop to show the window.
        /// // Time edit 
        /// </summary>
        /// <param name="?"></param>
        private void OnTimeProofCompleted()
        {
            if (!ApplicationViewModel.Instance.HasActiveModel)
            {
                return;
            }

            //var timeTaken = Math.Round((DateTime.Now - timer).TotalSeconds, 1); // Use this for display?

            // Show a Cancellable Busy Indicator window
            //ApplicationViewModel.Instance.Container
            //        .Resolve<IBusyIndicatorService>()
            //        .Show("Initialising LTL...");

            var modelVM = ApplicationViewModel.Instance.ActiveModel;        // Gets active model's values.

            var timeVM = TimeViewModelFactory.Create(modelVM);              // Sets only the name.

            ApplicationViewModel.Instance.Container
                    .Resolve<ITimeWindowService>().Show(timeVM);

            ApplicationViewModel.Instance.Container
               .Resolve<IBusyIndicatorService>()
               .Close();
        }


        /// <summary>
        /// Gets the value of the <see cref="ToggleAnalyzerLoggingCommand"/> property.
        /// </summary>
        public DelegateCommand ToggleAnalyzerLoggingCommand
        {
            get { return this.toggleAnalyzerLoggingCommand; }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="EnableAnalyzerLogging"/> property.
        /// </summary>
        public bool EnableAnalyzerLogging
        {
            get { return ApplicationViewModel.Instance.Settings.EnableLogging; }
            set
            {
                if (ApplicationViewModel.Instance.Settings.EnableLogging != value)
                {
                    ApplicationViewModel.Instance.Settings.EnableLogging = value;
                    OnPropertyChanged(() => EnableAnalyzerLogging);
                }
            }
        }

        private void OnToggleAnalyzerLoggingExecuted()
        {
            this.EnableAnalyzerLogging = !this.EnableAnalyzerLogging;
            OnPropertyChanged(() => ToggleLoggingText);
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="ToggleLoggingText"/> property.
        /// </summary>
        public string ToggleLoggingText
        {
            get
            {
                return EnableAnalyzerLogging ? "Disable Logging" : "Enable Logging";
            }
        }

        private readonly DelegateCommand exportAnalysisInputCommand;

        /// <summary>
        /// Gets the value of the <see cref="ExportAnalysisInputCommand"/> property.
        /// </summary>
        public DelegateCommand ExportAnalysisInputCommand
        {
            get { return this.exportAnalysisInputCommand; }
        }

        private void OnExportAnalysisInputExecuted()
        {
            var modelVM = ApplicationViewModel.Instance.ActiveModel;

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML files (*.xml)|*.xml";
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.DefaultFileName = modelVM.Name + " AnalysisInput";

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var stream = saveFileDialog.OpenFile())
                {
                    var xml = ZipHelper.Unzip(this.analysisInputDto.ZippedXml);
                    var xdoc = XDocument.Parse(xml);

                    var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true });
                    xdoc.Save(xmlWriter);
                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
        }

        private readonly DelegateCommand exportAnalysisOutputCommand;

        /// <summary>
        /// Gets the value of the <see cref="ExportAnalysisOutputCommand"/> property.
        /// </summary>
        public DelegateCommand ExportAnalysisOutputCommand
        {
            get { return this.exportAnalysisOutputCommand; }
        }

        private void OnExportAnalysisOutputExecuted()
        {
            if (analysisOutput == null)
            {
                ApplicationViewModel.Instance.Container
                 .Resolve<IMessageWindowService>()
                 .Show("Please run the analysis to create the output.");
                return;
            }

            var modelVM = ApplicationViewModel.Instance.ActiveModel;

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML files (*.xml)|*.xml";
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.DefaultFileName = modelVM.Name + " AnalysisOutput";

            if (saveFileDialog.ShowDialog() == true)
            {
                using (var stream = saveFileDialog.OpenFile())
                {
                    var xml = ZipHelper.Unzip(analysisOutput.Dto.ZippedXml);
                    var xdoc = XDocument.Parse(xml);

                    var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true });
                    xdoc.Save(xmlWriter);
                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
        }

        private readonly DelegateCommand showErrorTraceCommand;

        /// <summary>
        /// Gets the value of the <see cref="ShowErrorTraceCommand"/> property.
        /// </summary>
        public DelegateCommand ShowErrorTraceCommand
        {
            get { return this.showErrorTraceCommand; }
        }

        private void OnShowErrorTraceCommand()
        {
            if (proofVM == null)
            {
                ApplicationViewModel.Instance.Container
                 .Resolve<IMessageWindowService>()
                 .Show("There is no Proof View open.");
                return;
            }

            if (proofVM.FurtherTestingOutput == null)
            {
                ApplicationViewModel.Instance.Container
                .Resolve<IMessageWindowService>()
                .Show("There is no Error Trace to show.");
                return;
            }

            var dto = proofVM.FurtherTestingOutput.Dto;

            var detailsBuilder = new StringBuilder();

            foreach (var cexOutput in dto.CounterExamples)
            {
                var xml = ZipHelper.Unzip(cexOutput.ZippedXml);
                detailsBuilder.AppendLine(xml);
            }

            ApplicationViewModel.Instance.Container
                        .Resolve<ILogWindowService>()
                        .Show(detailsBuilder.ToString());
        }

        private readonly DelegateCommand showAnalyzerLogCommand;

        /// <summary>
        /// Gets the value of the <see cref="ShowAnalyzerLogCommand"/> property.
        /// </summary>
        public DelegateCommand ShowAnalyzerLogCommand
        {
            get { return this.showAnalyzerLogCommand; }
        }

        private void OnShowAnalyzerLogExecuted()
        {
            if (analysisOutput == null)
            {
                ApplicationViewModel.Instance.Container
                     .Resolve<IMessageWindowService>()
                     .Show("Please run the analysis to create the log.");
                return;
            }

            string details = BuildAnalyisMessage(analysisOutput);

            ApplicationViewModel.Instance.Container
                        .Resolve<ILogWindowService>()
                        .Show(details);
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

        private readonly DelegateCommand showAnalysisInputCommand;

        /// <summary>
        /// Gets the value of the <see cref="ShowAnalysisInputCommand"/> property.
        /// </summary>
        public DelegateCommand ShowAnalysisInputCommand
        {
            get { return this.showAnalysisInputCommand; }
        }

        private void OnShowAnalysisInputExecuted()
        {
            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            var input = AnalysisInputDTOFactory.Create(modelVM);

            var xml = ZipHelper.Unzip(input.ZippedXml);
            var xdoc = XDocument.Parse(xml, LoadOptions.PreserveWhitespace);

            string details = xdoc.ToString();

            ApplicationViewModel.Instance.Container
                        .Resolve<ILogWindowService>()
                        .Show(details);
        }

        private readonly DelegateCommand showAnalysisOutputCommand;

        /// <summary>
        /// Gets the value of the <see cref="ShowAnalysisOutputCommand"/> property.
        /// </summary>
        public DelegateCommand ShowAnalysisOutputCommand
        {
            get { return this.showAnalysisOutputCommand; }
        }

        private void OnShowAnalysisOutputExecuted()
        {
            if (analysisOutput == null)
            {
                ApplicationViewModel.Instance.Container
              .Resolve<IMessageWindowService>()
              .Show("Please run the analysis to create the output.");
                return;
            }

            var xml = ZipHelper.Unzip(analysisOutput.Dto.ZippedXml);
            var xdoc = XDocument.Parse(xml);
            var details = xdoc.ToString();

            ApplicationViewModel.Instance.Container
                        .Resolve<ILogWindowService>()
                        .Show(details);
        }

        private readonly DelegateCommand showModelXmlCommand;

        /// <summary>
        /// Gets the value of the <see cref="ShowModelXmlCommand"/> property.
        /// </summary>
        public DelegateCommand ShowModelXmlCommand
        {
            get { return this.showModelXmlCommand; }
        }

        private void OnShowModelXmlExecuted()
        {
            if (!ApplicationViewModel.Instance.HasActiveModel)
            {
                ApplicationViewModel.Instance.Container
                 .Resolve<IMessageWindowService>()
                 .Show("There is no active model.");
                return;
            }

            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            var xdoc = ModelXmlSaver.SaveToXml(modelVM);
            string details = xdoc.ToString();

            ApplicationViewModel.Instance.Container
                        .Resolve<ILogWindowService>()
                        .Show(details);
        }

        private readonly DelegateCommand exportModelXmlCommand;

        /// <summary>
        /// Gets the value of the <see cref="ExportModelXmlCommand"/> property.
        /// </summary>
        public DelegateCommand ExportModelXmlCommand
        {
            get { return this.exportModelXmlCommand; }
        }

        private void OnExportModelXmlExecuted()
        {
            if (!ApplicationViewModel.Instance.HasActiveModel)
            {
                ApplicationViewModel.Instance.Container
               .Resolve<IMessageWindowService>()
               .Show("There is no active model to export.");
                return;
            }

            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            ModelXmlSaver.Save(modelVM);
        }

        #endregion

        #region Debugging

        /// <summary>
        /// Gets the value of the <see cref="LogVisualTreeCommand"/> property.
        /// </summary>
        public DelegateCommand LogVisualTreeCommand
        {
            get { return this.logVisualTreeCommand; }
        }

        private void OnLogVisualTreeExecuted()
        {
            VisualTreeVisualizer.WriteDownwards(App.Current.RootVisual);
        }

        /// <summary>
        /// Gets the value of the <see cref="GCCollectCommand"/> property.
        /// </summary>
        public DelegateCommand GCCollectCommand
        {
            get { return this.gcCollectCommand; }
        }

        private void OnGCCollectExecuted()
        {
            GC.Collect();
        }


        #endregion
    }
}