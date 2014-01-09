using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using BioCheck.AnalysisService;
using BioCheck.ViewModel.Cells;
using BioCheck.ViewModel.Factories;

namespace BioCheck.ViewModel.Proof
{
    public class FormulaValidator
    {
        private readonly EditVariableViewModel editVariableVM;
        private readonly VariableViewModel variableVM;
        private AnalysisServiceClient analyzerClient;

        public FormulaValidator(EditVariableViewModel editVariableVM, VariableViewModel variableVM)
        {
            this.editVariableVM = editVariableVM;
            this.variableVM = variableVM;

            // Create the client proxy to the Analyzer web service
            var serviceUri = new Uri("../Services/AnalysisService.svc", UriKind.Relative);
            var endpoint = new EndpointAddress(serviceUri);
            analyzerClient = new AnalysisServiceClient("AnalysisServiceCustom", endpoint);
            analyzerClient.ValidateCompleted += OnValidateCompleted;
        }

        public void Validate(string formula)
        {
            if (string.IsNullOrEmpty(editVariableVM.Formula))
            {
                ResetFormulaValidation();
            }
            else if (editVariableVM.Formula == EditVariableViewModel.DefaultFormula)
            {
                ResetFormulaValidation();
            }
            else
            {
                string message = string.Empty;
                bool isValid = ValidateInputs(editVariableVM, variableVM, formula, out message);

                if (isValid)
                {
                    analyzerClient.ValidateAsync(formula);
                    //    var result = UIExpr.check_syntax(formula);
                }
                else
                {
                    editVariableVM.FormulaValidationMessage = message;
                    editVariableVM.FormulaValidationDetails = message;
                    editVariableVM.IsFormulaValid = false;
                    editVariableVM.IsFormulaInvalid = true;
                }
            }
        }

        private void ResetFormulaValidation()
        {
            editVariableVM.IsFormulaValid = true;
            editVariableVM.IsFormulaInvalid = false;
            editVariableVM.FormulaValidationMessage = string.Empty;
            editVariableVM.FormulaValidationDetails = string.Empty;
        }

        private void OnValidateCompleted(object sender, ValidateCompletedEventArgs e)
        {
            bool isValid;
            string message = string.Empty;
            string details = string.Empty;

            try
            {
                var validationOutput = e.Result;
                isValid = validationOutput.IsValid;

                if (!isValid)
                {
                    message = validationOutput.Message;
                    details = validationOutput.Details;
                    // Debug.WriteLine(string.Format("IsValid={0}, Line={1}, Column={2}, Message={3}", validationOutput.IsValid, validationOutput.Line, validationOutput.Column, validationOutput.Message));
                }
            }
            catch (Exception ex)
            {
                details = ex.ToString();
                if (ex.InnerException != null)
                {
                    details += ex.InnerException.ToString();
                }

                editVariableVM.FormulaValidationMessage = "There was an error validating the formula";
                editVariableVM.FormulaValidationDetails = details;
                editVariableVM.IsFormulaValid = false;
                editVariableVM.IsFormulaInvalid = true;

                return;
            }

            if (e.Error == null)
            {
                editVariableVM.FormulaValidationDetails = details;
                editVariableVM.FormulaValidationMessage = message;
                editVariableVM.IsFormulaValid = isValid;
                editVariableVM.IsFormulaInvalid = !isValid;
            }
        }

        public static bool ValidateInputs(EditVariableViewModel editVariableVM, VariableViewModel variableVM, string newFormula, out string message)
        {
            bool inputsAreValid = true;
            message = string.Empty;
            string variableKeyword = "var";

            if (!newFormula.Contains(variableKeyword))
            {
                return true;
            }

            string currentFormula = newFormula;

            var modelVM = ApplicationViewModel.Instance.ActiveModel;

            var inputVariables =
                   (from extRvm in modelVM.RelationshipViewModels
                    where extRvm.To == variableVM
                    select extRvm.From)
                      .ToList();

            int startIndex = 0;

            int varIndex = currentFormula.IndexOf(variableKeyword, startIndex);

            while (varIndex >= 0)
            {
                // Get the first bracket after it
                int leftBracketIndex = currentFormula.IndexOf('(', varIndex);
                int rightBracketIndex = currentFormula.IndexOf(')', varIndex);

                if (leftBracketIndex < 0 || rightBracketIndex < 0)
                {
                    message = "No variable name specified";
                    inputsAreValid = false;
                    return false;
                }

                string varName = currentFormula.Substring(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1);

                var vvm = (from iv in inputVariables
                           let vvmName = NameFactory.GetVariableName(iv)
                           where vvmName == varName
                           select iv).FirstOrDefault();

                if (vvm == null)
                {
                    vvm = (from iv in inputVariables
                           let vvmName = iv.Name
                           where vvmName == varName
                           select iv).FirstOrDefault();
                }

                if (vvm != null)
                {
                    int id = vvm.Id;

                    currentFormula = currentFormula.Remove(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1);
                    currentFormula = currentFormula.Insert(leftBracketIndex + 1, id.ToString());
                }
                else
                {
                    message = "Invalid input variable name: " + varName;
                    inputsAreValid = false;
                    return false;
                }

                varIndex = currentFormula.IndexOf(variableKeyword, ++varIndex);
            }

            return inputsAreValid;
        }
    }
}