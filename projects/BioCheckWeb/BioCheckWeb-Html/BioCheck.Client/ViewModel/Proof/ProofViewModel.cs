using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using BioCheck.AnalysisService;
using BioCheck.Services;
using BioCheck.ViewModel.Cells;
using BioCheck.ViewModel.Factories;
using MvvmFx.Common.ViewModels.Behaviors.Observable;
using MvvmFx.Common.ViewModels.Commands;
using Microsoft.Practices.Unity;
using FurtherTestingOutputDTO = BioCheck.AnalysisService.FurtherTestingOutputDTO;

namespace BioCheck.ViewModel.Proof
{
    public class ProofViewModel : ObservableViewModel
    {
        private readonly AnalysisInputDTO input;
        private readonly AnalysisOutput output;

        private readonly DelegateCommand closeCommand;
        private readonly DelegateCommand getCounterExamplesCommand;
        private readonly DelegateCommand cancelProofCommand;

        private ProofViewState state;

        private string modelName;
        private double time;
        private int steps;
        private List<VariableProofViewModel> variables;
        private VariableProofViewModel selectedProofVariable;
        private List<ProgressionInfo> progressionInfos;
        private ProgressionInfo selectedProgressionInfo;
        private List<CounterExampleInfo> counterExampleInfos;

        public ProofViewModel(AnalysisInputDTO input, AnalysisOutput output)
        {
            this.input = input;
            this.output = output;

            this.closeCommand = new DelegateCommand(OnCloseExecuted);
            this.getCounterExamplesCommand = new DelegateCommand(OnGetCounterExamplesExecuted);
            this.cancelProofCommand = new DelegateCommand(OnCancelProofExecuted);

            this.variables = new List<VariableProofViewModel>();
            this.progressionInfos = new List<ProgressionInfo>();
            this.counterExampleInfos = new List<CounterExampleInfo>();

            AddHandlers();
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="State"/> property.
        /// </summary>
        public ProofViewState State
        {
            get { return this.state; }
            set
            {
                if (this.state != value)
                {
                    this.state = value;
                    OnPropertyChanged(() => State);
                    OnPropertyChanged(() => IsStable);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="IsStable"/> property.
        /// </summary>
        public bool IsStable
        {
            get { return this.state == ProofViewState.Stable; }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Variables"/> property.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the value of the <see cref="SelectedProofVariable"/> property.
        /// </summary>
        public VariableProofViewModel SelectedProofVariable
        {
            get { return this.selectedProofVariable; }
            set
            {
                if (this.selectedProofVariable != value)
                {
                    if (this.selectedProofVariable != null)
                        OnVariableUnselected();

                    this.selectedProofVariable = value;
                    OnPropertyChanged(() => SelectedProofVariable);

                    // When a variable is selected in the grid, highlight it in the model
                    OnVariableSelected();
                }
            }
        }

        private void OnVariableUnselected()
        {
            if (this.selectedProofVariable == null)
                return;

            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            var variableVM = modelVM.GetVariable(this.selectedProofVariable.Id);

            if (variableVM == null)
            {
                // Can't find the variable
                return;
            }

            variableVM.RemoveHandler("Name", OnVariableNameChanged);
            variableVM.RemoveHandler("RangeFrom", OnVariableRangeFromChanged);
            variableVM.RemoveHandler("RangeTo", OnVariableRangeToChanged);
            variableVM.RemoveHandler("Formula", OnVariableFormulaChanged);
        }

        private void OnVariableSelected()
        {
            if (this.selectedProofVariable == null)
                return;

            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            var variableVM = modelVM.GetVariable(this.selectedProofVariable.Id);

            if (variableVM == null)
            {
                // Can't find the variable
                return;
            }

            variableVM.IsChecked = true;
            ApplicationViewModel.Instance.ActiveVariable = variableVM;

            // Add Handlers to the variable
            variableVM.AddHandler("Name", OnVariableNameChanged);
            variableVM.AddHandler("RangeFrom", OnVariableRangeFromChanged);
            variableVM.AddHandler("RangeTo", OnVariableRangeToChanged);
            variableVM.AddHandler("Formula", OnVariableFormulaChanged);
        }

        private void OnActiveVariableChanged(object sender, EventArgs eventArgs)
        {
            var activeVariableVM = ApplicationViewModel.Instance.ActiveVariable;
            if (activeVariableVM == null)
                return;

            var proofVariableVM = this.variables.FirstOrDefault(v => v.Id == activeVariableVM.Id);
            if (proofVariableVM != null)
                this.SelectedProofVariable = proofVariableVM;

            var progressionInfo = this.progressionInfos.FirstOrDefault(p => p.Id == activeVariableVM.Id);
            if (progressionInfo != null)
                this.SelectedProgressionInfo = progressionInfo;
        }

        private void OnVariableNameChanged(object sender, EventArgs e)
        {
            var variableVM = (VariableViewModel)sender;
            this.selectedProofVariable.Name = NameFactory.GetVariableName(variableVM);
            this.selectedProgressionInfo.Name = this.selectedProofVariable.Name;
        }

        private void OnVariableFormulaChanged(object sender, EventArgs e)
        {
            var variableVM = (VariableViewModel)sender;
            this.selectedProofVariable.TargetFunction = variableVM.Formula;
        }

        private void OnVariableRangeToChanged(object sender, EventArgs e)
        {
            var variableVM = (VariableViewModel)sender;
            this.selectedProofVariable.Range = variableVM.RangeFrom == variableVM.RangeTo ? variableVM.RangeFrom.ToString() : string.Format("{0} - {1}", variableVM.RangeFrom, variableVM.RangeTo);
        }

        private void OnVariableRangeFromChanged(object sender, EventArgs e)
        {
            var variableVM = (VariableViewModel)sender;
            this.selectedProofVariable.Range = variableVM.RangeFrom == variableVM.RangeTo ? variableVM.RangeFrom.ToString() : string.Format("{0} - {1}", variableVM.RangeFrom, variableVM.RangeTo);
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="ProgressionInfos"/> property.
        /// </summary>
        public List<ProgressionInfo> ProgressionInfos
        {
            get { return this.progressionInfos; }
            set
            {
                if (this.progressionInfos != value)
                {
                    this.progressionInfos = value;
                    OnPropertyChanged(() => ProgressionInfos);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="SelectedProgressionInfo"/> property.
        /// </summary>
        public ProgressionInfo SelectedProgressionInfo
        {
            get { return this.selectedProgressionInfo; }
            set
            {
                if (this.selectedProgressionInfo != value)
                {
                    this.selectedProgressionInfo = value;
                    OnPropertyChanged(() => SelectedProgressionInfo);

                    if (this.selectedProgressionInfo != null)
                    {
                        var proofVariableVM = this.variables.FirstOrDefault(v => v.Id == this.selectedProgressionInfo.Id);
                        if (proofVariableVM != null)
                            this.SelectedProofVariable = proofVariableVM;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="CounterExampleInfos"/> property.
        /// </summary>
        public List<CounterExampleInfo> CounterExampleInfos
        {
            get { return this.counterExampleInfos; }
            set
            {
                if (this.counterExampleInfos != value)
                {
                    this.counterExampleInfos = value;
                    OnPropertyChanged(() => CounterExampleInfos);
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
                .Resolve<IProofWindowService>().
                Close();
        }

        /// <summary>
        /// Gets the value of the <see cref="GetCounterExamplesCommand"/> property.
        /// </summary>
        public DelegateCommand GetCounterExamplesCommand
        {
            get { return this.getCounterExamplesCommand; }
        }


        private AnalysisServiceClient analyzerClient;
        private DateTime timer;

        private void OnGetCounterExamplesExecuted()
        {
            if (!ApplicationViewModel.Instance.HasActiveModel)
            {
                return;
            }

            ApplicationViewModel.Instance.Container
                    .Resolve<IBusyIndicatorService>()
                    .Show("Getting error trace...", CancelProofCommand);

            // Enable/Disable logging
            input.EnableLogging = ApplicationViewModel.Instance.ToolbarViewModel.EnableAnalyzerLogging;

            // Create the analyzer client
            if (analyzerClient == null)
            {
                var serviceUri = new Uri("../Services/AnalysisService.svc", UriKind.Relative);
                var endpoint = new EndpointAddress(serviceUri);
                analyzerClient = new AnalysisServiceClient("AnalysisServiceCustom", endpoint);
                analyzerClient.FindCounterExamplesCompleted += analyzerClient_FindCounterExamplesCompleted;
            }

            // Trim the message payload
            var outputCopy = new AnalysisOutputDTO();
            outputCopy.ZippedXml = output.Dto.ZippedXml;

            // Invoke the async Analyze method on the service
            timer = DateTime.Now;
            analyzerClient.FindCounterExamplesAsync(input, outputCopy);
        }

        public DelegateCommand CancelProofCommand
        {
            get { return this.cancelProofCommand; }
        }

        private void OnCancelProofExecuted()
        {
            if (analyzerClient != null)
            {
                analyzerClient.FindCounterExamplesCompleted -= analyzerClient_FindCounterExamplesCompleted;
                analyzerClient = null;
            }

            ApplicationViewModel.Instance.Container
           .Resolve<IBusyIndicatorService>()
           .Close();
        }

        private FurtherTestingOutput cexOutput;

        public FurtherTestingOutput FurtherTestingOutput
        {
            get { return cexOutput; }
        }

        public void ResetOutput()
        {
            cexOutput = null;
        }

        void analyzerClient_FindCounterExamplesCompleted(object sender, FindCounterExamplesCompletedEventArgs e)
        {
            var time = Math.Round((DateTime.Now - timer).TotalSeconds, 1);
            Debug.WriteLine(string.Format("Further Testing took {0} seconds to run.", time));

            try
            {
                cexOutput = FurtherTestingFactory.Create(e.Result);
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
                      .Show("There was an error getting the counter examples.", details);

                // Log the error to the Log web service
                ApplicationViewModel.Instance.Log.Error("There was an error running the further testing.", details);

                return;
            }

            if (e.Error == null)
            {
                this.CounterExampleInfos = ProofViewModelFactory.CreateCounterExamples(this, cexOutput);

                // StableByExclusion
                if (cexOutput.CounterExamples.Count == 0)
                {
                    this.State = ProofViewState.StableByExclusion;
                }
                else
                {
                    this.State = ProofViewState.CounterExamples;
                }

                ApplicationViewModel.Instance.Container
                  .Resolve<IBusyIndicatorService>()
                  .Close();

                // Log the further testing
                ApplicationViewModel.Instance.Log.FurtherTesting();
            }
        }

        private void AddHandlers()
        {
            ApplicationViewModel.Instance.AddHandler("ActiveVariable", OnActiveVariableChanged);
        }

        protected override void RemoveHandlers()
        {
            // Remove the handlers on the selected variable
            OnVariableUnselected();

            ApplicationViewModel.Instance.RemoveHandler("ActiveVariable", OnActiveVariableChanged);

            base.RemoveHandlers();
        }
    }
}