using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BioCheck.ViewModel.Factories;
using BioCheck.ViewModel.Proof;
using Microsoft.Practices.ObjectBuilder2;
using MvvmFx.Common.Helpers;
using MvvmFx.Common.ViewModels.Behaviors.Observable;
using MvvmFx.Common.ViewModels.Commands;
using System;

namespace BioCheck.ViewModel.Cells
{
    public class EditVariableViewModel : EditingViewModelBase
    {
        private readonly VariableViewModel variableVM;

        private static List<FormulaKeyword> formulaKeywords;
        private static FormulaKeyword selectedFormulaKeyword;
        private readonly ObservableCollection<string> inputs;
        private FormulaValidator formulaValidator;

        public const string DefaultFormula = "avg(pos)-avg(neg)";

        private readonly DelegateCommand insertFormulaKeywordCommand;

        private bool isFormulaValid;
        private bool isFormulaInvalid;
        private int caretPosition;
        private string formulaValidationMessage;
        private string formulaValidationDetails;
        private string selectedInput;
        private bool formulaHasChanged;

        static EditVariableViewModel()
        {
            // Create the list and info for the keywords for using in formulas.
            // This is static and shared across all variablevm's for use in the property box
            formulaKeywords = FormulaFactory.CreateKeywords();
            selectedFormulaKeyword = formulaKeywords[0];
        }

        public EditVariableViewModel(VariableViewModel variableVM)
        {
            this.variableVM = variableVM;
            this.inputs = new ObservableCollection<string>();

            this.insertFormulaKeywordCommand = new DelegateCommand(OnInsertFormulaKeywordExecuted);
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Name"/> property.
        /// </summary>
        public string Name
        {
            get { return this.variableVM.Name; }
            set
            {
                if (this.variableVM.Name != value)
                {
                    this.variableVM.Name = value;
                    OnPropertyChanged(() => Name);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="RangeFrom"/> property.
        /// </summary>
        public int RangeFrom
        {
            get { return this.variableVM.RangeFrom; }
            set
            {
                if (this.variableVM.RangeFrom != value)
                {
                    this.variableVM.RangeFrom = value;
                    OnPropertyChanged(() => RangeFrom);

                    // Reset the Stability
                    this.variableVM.ResetStability();
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="RangeTo"/> property.
        /// </summary>
        public int RangeTo
        {
            get { return this.variableVM.RangeTo; }
            set
            {
                if (this.variableVM.RangeTo != value)
                {
                    this.variableVM.RangeTo = value;
                    OnPropertyChanged(() => RangeTo);

                    // Reset the Stability
                    this.variableVM.ResetStability();
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="Formula"/> property.
        /// </summary>
        public string Formula
        {
            get
            {
                return this.variableVM.Formula;
            }
            set
            {
                if (this.variableVM.Formula != value)
                {
                    this.variableVM.Formula = value;
                    OnPropertyChanged(() => Formula);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="DisplayFormula"/> property.
        /// </summary>
        public string DisplayFormula
        {
            get
            {
                if (!formulaHasChanged && string.IsNullOrEmpty(this.Formula))
                {
                    return DefaultFormula;
                }
                return this.Formula;
            }
            set
            {
                if (this.Formula != value)
                {
                    this.formulaHasChanged = true;

                    if (value == DefaultFormula)
                    {
                        this.Formula = "";
                    }
                    else
                    {
                        this.Formula = value;
                    }
                    OnPropertyChanged(() => DisplayFormula);

                    // Reset the Stability
                    variableVM.ResetStability();

                    // Reset the validity
                    DispatcherHelper.BeginInvoke(ResetFormulaValidity);
                }
            }
        }

        private void ResetFormulaValidity()
        {
            if (formulaValidator == null)
                formulaValidator = new FormulaValidator(this, this.variableVM);

            formulaValidator.Validate(this.Formula);
        }

        /// <summary>
        /// Gets the formula keywords.
        /// </summary>
        public List<FormulaKeyword> FormulaKeywords
        {
            get { return formulaKeywords; }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="SelectedFormulaKeyword"/> property.
        /// </summary>
        public FormulaKeyword SelectedFormulaKeyword
        {
            get { return selectedFormulaKeyword; }
            set
            {
                if (selectedFormulaKeyword != value)
                {
                    selectedFormulaKeyword = value;
                    OnPropertyChanged(() => SelectedFormulaKeyword);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="CaretPosition"/> property.
        /// </summary>
        public int CaretPosition
        {
            get { return this.caretPosition; }
            set
            {
                if (this.caretPosition != value)
                {
                    this.caretPosition = value;
                    OnPropertyChanged(() => CaretPosition);
                }
            }
        }

        /// <summary>
        /// Gets the value of the <see cref="InsertFormulaKeywordCommand"/> property.
        /// </summary>
        public DelegateCommand InsertFormulaKeywordCommand
        {
            get { return this.insertFormulaKeywordCommand; }
        }

        private void OnInsertFormulaKeywordExecuted()
        {
            if (this.SelectedFormulaKeyword == null)
                return;

            // Insert the keyword at the current caret position, and update the caret position to the new one.
            var newDisplayFormula = this.Formula ?? string.Empty;

            var newCaretPosition = this.CaretPosition < newDisplayFormula.Length ? this.CaretPosition : newDisplayFormula.Length - 1;
            newCaretPosition = Math.Max(newCaretPosition, 0);

            this.DisplayFormula = newDisplayFormula.Insert(newCaretPosition, this.SelectedFormulaKeyword.InsertString);

            this.CaretPosition = newCaretPosition + this.SelectedFormulaKeyword.CaretIndex;
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="FormulaValidationMessage"/> property.
        /// </summary>
        public string FormulaValidationMessage
        {
            get { return this.formulaValidationMessage; }
            set
            {
                if (this.formulaValidationMessage != value)
                {
                    this.formulaValidationMessage = value;
                    OnPropertyChanged(() => FormulaValidationMessage);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="FormulaValidationDetails"/> property.
        /// </summary>
        public string FormulaValidationDetails
        {
            get { return this.formulaValidationDetails; }
            set
            {
                if (this.formulaValidationDetails != value)
                {
                    this.formulaValidationDetails = value;
                    OnPropertyChanged(() => FormulaValidationDetails);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="IsFormulaValid"/> property.
        /// </summary>
        public bool IsFormulaValid
        {
            get { return this.isFormulaValid; }
            set
            {
                if (this.isFormulaValid != value)
                {
                    this.isFormulaValid = value;
                    OnPropertyChanged(() => IsFormulaValid);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="IsFormulaInvalid"/> property.
        /// </summary>
        public bool IsFormulaInvalid
        {
            get { return this.isFormulaInvalid; }
            set
            {
                if (this.isFormulaInvalid != value)
                {
                    this.isFormulaInvalid = value;
                    OnPropertyChanged(() => IsFormulaInvalid);
                }
            }
        }

        public ObservableCollection<string> Inputs
        {
            get
            {
                // Get the updated list of inputs
                DispatcherHelper.BeginInvoke(OnGetInputs);
                return this.inputs;
            }
        }

        private void OnGetInputs()
        {
            var modelVM = ApplicationViewModel.Instance.ActiveModel;

            var inputVariables =
                (from extRvm in modelVM.RelationshipViewModels
                 where extRvm.To == this.variableVM
                 let varName = NameFactory.GetVariableName(extRvm.From)
                 orderby varName
                 select varName);

            this.inputs.Clear();

            inputVariables.ForEach(iv => this.inputs.Add(iv));
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="SelectedInput"/> property.
        /// </summary>
        public string SelectedInput
        {
            get
            {
                return this.selectedInput;
            }
            set
            {
                if (this.selectedInput != value)
                {
                    this.selectedInput = value;
                    OnPropertyChanged(() => SelectedInput);

                    if (!string.IsNullOrEmpty(this.selectedInput))
                    {
                        // Insert the input name at the current caret position
                        this.DisplayFormula = (this.Formula ?? string.Empty).Insert(this.CaretPosition, this.selectedInput);

                        // Set the caret to the end
                        this.CaretPosition = this.DisplayFormula.Length;
                    }
                }
            }
        }
    }
}