﻿using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using BioCheck.AnalysisService;
using BioCheck.Helpers;
using BioCheck.ViewModel.Factories;
using BioCheck.ViewModel.Models;
using BioCheck.ViewModel.Proof;

namespace BioCheck.ViewModel.Time
{
    /// <summary>
    /// Static Factory class for converting a ModelViewModel to AnalysisInput
    /// </summary>
    public static class TimeInputDTOFactory
    {
        /// <summary>
        /// Creates the AnalysisInput from the specified model VM.
        /// </summary>
        /// <param name="modelVM">The model VM.</param>
        /// <returns></returns>
        public static AnalysisInputDTO Create(ModelViewModel modelVM, TimeViewModel timeVM)
        {
            var data = new AnalysisInputDTO();

            data.ModelName = modelVM.Name;

            // Check that fields contain data, and data of the correct type
            // Formula checker
            if (timeVM.LTLInput == "" || string.IsNullOrEmpty(timeVM.LTLInput) || timeVM.LTLInput.Length == 0)
            {
                timeVM.LTLInput = "True";
            }
            
            string bracketedFormula;
            string currFormula = timeVM.LTLInput;
            string noWhite = currFormula.Trim();    // Strip start and end whitespace

            // If not bracket enclosed, bracket-enclose
            if (!noWhite.StartsWith("(") || !noWhite.EndsWith(")"))
            {
                bracketedFormula = "(";
                bracketedFormula += noWhite;
                bracketedFormula += ")";
            }
            else if (noWhite.StartsWith("(((") && noWhite.EndsWith(")))"))
            {
                bracketedFormula = noWhite.Remove(0, 2);
                bracketedFormula = bracketedFormula.Remove(bracketedFormula.Length - 2, 2);
            }
            else if (noWhite.StartsWith("((") && noWhite.EndsWith("))"))
            {
                bracketedFormula = noWhite.Remove(0,1);
                bracketedFormula = bracketedFormula.Remove(bracketedFormula.Length - 1,1);
            }
            else 
            {
                bracketedFormula = noWhite;         // Pre-bracketed correctly by user.
            }

            string finalFormula = bracketedFormula;
            bool checkTmp = timeVM.LTLProof;
            if (checkTmp)
            {
                finalFormula = "(Not ";
                finalFormula += bracketedFormula;
                finalFormula += ")";
            }


            // Path checker
            if (timeVM.LTLPath < 1)
            {
                timeVM.LTLPath = 100;                   // No simulations with 0 or a negative number of steps are allowed.
            }
            else if (timeVM.LTLPath == null) 
            {
                timeVM.LTLPath = 100;
            }
            else
            {
                string testInt2String = timeVM.LTLPath.ToString();
                int testInt;
                try
                {
                    testInt = System.Int32.Parse(testInt2String);
                }
                catch (System.FormatException)
                {
                    //Could not make an integer
                    timeVM.LTLPath = 100;
                }
                catch (System.OverflowException)
                {
                    //The int is too big/small for an int
                    timeVM.LTLPath = 100;
                }
            }


            var xdoc = new XDocument(
                new XElement("AnalysisInput",
                                new XAttribute("ModelName", data.ModelName),
                                new XElement("Engine",
                                    new XElement("Name", "CAV"),
                                    new XElement("Formula", finalFormula),
                                    new XElement("Number_of_steps", timeVM.LTLPath)),
                                new XElement("Variables",
                                    (from v in
                                         (from extVvm in modelVM.VariableViewModels select extVvm)
                                         .Union(
                                         (from cvm in modelVM.ContainerViewModels
                                          from intVvm in cvm.VariableViewModels
                                          select intVvm))
                                     select new XElement("Variable",
                                                         new XAttribute("Id", v.Id),
                                                         new XElement("Name", NameFactory.GetVariableName(v)),
                                                         new XElement("RangeFrom", v.RangeFrom),
                                                         new XElement("RangeTo", v.RangeTo),
                                                         new XElement("Function", FormulaFactory.Create(v))))),
                            new XElement("Relationships",
                                            from r in modelVM.RelationshipViewModels
                                            select new XElement("Relationship",
                                                                new XAttribute("Id", r.Id),
                                                                new XElement("FromVariableId", r.From.Id),
                                                                new XElement("ToVariableId", r.To.Id),
                                                                new XElement("Type", r.Type.ToString())))));

            data.ZippedXml = ZipHelper.Zip(xdoc.ToString());

            return data;
        }
    }
}