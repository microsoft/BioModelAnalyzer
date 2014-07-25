using System.Collections.Generic;
using System.Linq;
using BioCheck.ViewModel.Cells;
using BioCheck.ViewModel.Factories;

namespace BioCheck.ViewModel.Proof
{
    /// <summary>
    /// Static Factory class for creating the formula for AnalysisInput.
    /// </summary>
    /// <remarks>
    /// This means stripping out the variable names and replacing them with their Id's
    /// </remarks>
    public static class FormulaFactory
    {
        const string variableKeyword = "var";

        public static List<FormulaKeyword> CreateKeywords()
        {
            var formulaKeywords = new List<FormulaKeyword>
                                  {
                                     new FormulaKeyword { Name= "var", Syntax = "var(name)", Description="A variable, where name is the name of the variable", InsertString = "var()", CaretIndex = 4},
                                     new FormulaKeyword { Name= "avg", Syntax="avg(x,y,z)", Description = "The average of a list of expressions. E.g., avg( var(X); var(Y); 22; var(Z)*2 )", InsertString = "avg(,)", CaretIndex = 4},
                                     new FormulaKeyword { Name= "min", Syntax="min(x, y)", Description = "The minimum of two expressions. E.g., min(var(X),var(Y)), or min(var(X), 0)) ", InsertString = "min(,)", CaretIndex = 4},
                                     new FormulaKeyword { Name= "max", Syntax="max(x, y)", Description = "The maximum of two expressions. E.g., max(var(X),var(Y))", InsertString = "max(,)", CaretIndex = 4},
                                     new FormulaKeyword { Name= "const", Syntax="22 or const(22)", Description = "An integer number. E.g., 1234, 42, -9", InsertString = "const()", CaretIndex = 6},
                                     new FormulaKeyword { Name= "plus", Syntax="x + y", Description = "Usual addition operator. E.g., 2+3, 44 + var(X)", InsertString = " + ", CaretIndex = 3},
                                     new FormulaKeyword { Name= "minus", Syntax="x - y", Description = "Usual subtraction operator. E.g., 2-3, 44 - var(X)", InsertString = " - ", CaretIndex = 3},
                                     new FormulaKeyword { Name= "times", Syntax="x * y", Description = "Usual multiplication operator. E.g., 2*3, 44 * var(X)", InsertString = " * ", CaretIndex = 3},
                                     new FormulaKeyword { Name= "div", Syntax="x / y", Description = "Usual division operator. E.g., 2/3, 44 / var(X)", InsertString = " / ", CaretIndex = 3},
                                     new FormulaKeyword { Name= "ceil", Syntax="ceil(x)", Description = "The ceiling of an expression. E.g., ceil(var(X))", InsertString = "ceil()", CaretIndex = 5},
                                     new FormulaKeyword { Name= "floor", Syntax="floor(x)", Description = "The floor of an expression. E.g., floor(var(X))", InsertString = "floor()", CaretIndex = 6},
                                     //new FormulaKeyword { Name= "pos", Syntax="", Description = ""},
                                     //new FormulaKeyword { Name= "neg", Syntax="", Description = ""},
                                  };

            return formulaKeywords;
        }

        /// <summary>
        /// Creates the formula for the Variable for the AnalysisInput
        /// </summary>
        /// <param name="variableVM">The variable VM.</param>
        /// <returns></returns>
        public static string Create(VariableViewModel variableVM)
        {
            if (string.IsNullOrEmpty(variableVM.Formula))
            {
                return "";
            }

            if (variableVM.Formula == EditVariableViewModel.DefaultFormula)
            {
                return "";
            }

            string oldFormula = variableVM.Formula;

            // Check if a var(name) keyword exists in the formula, just return it if not.
            if (!oldFormula.Contains(variableKeyword))
            {
                return oldFormula;
            }

            string currentFormula = oldFormula;

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
                    return "";

                string varName = currentFormula.Substring(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1);

                var vvm = (from iv in inputVariables
                           let vvmName = NameFactory.GetVariableName(iv)
                           where vvmName == varName
                           select iv).FirstOrDefault();

                if (vvm == null)
                {
                    // Check this container for variables with the same name
                    vvm = (from iv in inputVariables
                           let vvmName = iv.Name
                           where vvmName == varName &&
                           iv.ContainerViewModel == variableVM.ContainerViewModel
                           select iv).FirstOrDefault();

                    // Check all containers for variables with the same name
                    if(vvm == null)
                    {
                        vvm = (from iv in inputVariables
                               let vvmName = iv.Name
                               where vvmName == varName
                               select iv).FirstOrDefault();
                    }
                }

                if (vvm != null)
                {
                    int id = vvm.Id;

                    currentFormula = currentFormula.Remove(leftBracketIndex + 1, rightBracketIndex - leftBracketIndex - 1);
                    currentFormula = currentFormula.Insert(leftBracketIndex + 1, id.ToString());
                }

                varIndex = currentFormula.IndexOf(variableKeyword, ++varIndex);
            }

            string newFormula = currentFormula;

            return newFormula;
        }

        /// <summary>
        /// Verifies the specified old formula.
        /// </summary>
        /// <param name="oldFormula">The old formula.</param>
        /// <returns></returns>
        public static string Verify(string oldFormula)
        {
            // Fix the ave/avg issue
            string newFormula = oldFormula.Replace("ave(", "avg(");
            return newFormula;
        }
    }
}