using System.Collections.ObjectModel;
using System.Linq;
using System.Collections.Generic;
using System.Xml.Linq;
using BioCheck.AnalysisService;
using BioCheck.Helpers;
using BioCheck.ViewModel.Factories;
using BioCheck.ViewModel.Models;
using BioCheck.ViewModel.Proof;
using System.Diagnostics;

namespace BioCheck.ViewModel.Synth
{
    /// <summary>
    /// Static Factory class for converting a ModelViewModel to AnalysisInput
    /// </summary>
    public static class SynthInputDTOFactory
    {
        /// <summary>
        /// Creates the AnalysisInput from the specified model VM.
        /// </summary>
        /// <param name="modelVM">The model VM.</param>
        /// <returns></returns>
        public static AnalysisInputDTO Create(ModelViewModel modelVM, SynthViewModel synthVM)
        {
            // Use the BMA-loaded model the first time only
            var data = new AnalysisInputDTO();

            data.ModelName = modelVM.Name;

            // Erased Formula input checkers

            var xdoc = new XDocument(
                new XElement("AnalysisInput",
                                new XAttribute("ModelName", data.ModelName),
                                new XElement("Engine",
                                    new XElement("Name", "SYN")),
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

            string tempXML = xdoc.ToString();
            data.ZippedXml = ZipHelper.Zip(tempXML);

            return data;
        }

        // Use overloaded function to add an edge == relationship
        // and edit target functions.
        public static string CreateNewEdgeString(SynthViewModel synthVM, int choiceEdge, int maxCurrRelationshipN)
        {

            // Retrieve for Relationship: fromID, toID, Nature
            string edgeNature = synthVM.TheList[choiceEdge].Nature;       // Nature
            string parseIds = synthVM.TheList[choiceEdge].wholeOutput;

            // FromId
            string[] varLineNodes = parseIds.Split(';');
            string[] idNfrom_string = varLineNodes[0].Split('=');
            string idNfrom = idNfrom_string[1].Trim();

            // ToId
            string[] idNto_string = varLineNodes[6].Split('=');
            string idNto = idNto_string[2].Trim();

            string newEdgesForXML = @"    <Relationship Id=""" + (maxCurrRelationshipN + 1) + @""">
      <FromVariableId>" + idNfrom + @"</FromVariableId>
      <ToVariableId>" + idNto + @"</ToVariableId>
      <Type>" + edgeNature + @"</Type>
    </Relationship>
";

            return newEdgesForXML;
        }

        public static string CreateAppendableXMLString(ModelViewModel modelVM)
        {
            string XMLappendable = "";

            var xdoc = new XDocument(
            new XElement("AnalysisInput",
                            new XAttribute("ModelName", modelVM.Name),
                            new XElement("Engine", 
                                new XElement ("Name", "SYN")),
                            new XElement("Variables",
                                (from v in
                                (from extVvm in modelVM.VariableViewModels select extVvm)
                                .Union(                                    (from cvm in modelVM.ContainerViewModels
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

            string fullXML = xdoc.ToString();
            string[] newlineDetails = fullXML.Split('\n');
            int Nlines = newlineDetails.Length;
            int lineCounter = 1;
            foreach (string s in newlineDetails)
            {
                if (lineCounter < Nlines -1)
                {
                    XMLappendable = XMLappendable + s;
                }
                lineCounter++;
            }
            lineCounter++;
            return XMLappendable;
        }

        // Use overloaded function to add an edge == relationship
        // and edit target functions.
        public static AnalysisInputDTO Create(ModelViewModel modelVM, SynthViewModel synthVM, int choiceEdge, int maxCurrRelationshipN)
        {
            var data = new AnalysisInputDTO();

            data.ModelName = modelVM.Name;

            // Retrieve for Relationship: fromID, toID, Nature
            string edgeNature = synthVM.TheList[choiceEdge].Nature;       // Nature
            string parseIds = synthVM.TheList[choiceEdge].wholeOutput;

            // FromId
            string[] varLineNodes = parseIds.Split(';');
            string[] idNfrom_string = varLineNodes[0].Split('=');
            string idNfrom = idNfrom_string[1].Trim();

            // ToId
            string[] idNto_string = varLineNodes[6].Split('=');
            string idNto = idNto_string[2].Trim();


            // Do I need to change the target formula?
            // No: if from-node == to-node || from node is already in input to To-node (not included here..)        
            // Why does same-ID to/from make no diff to the Tv?
            if (idNfrom != idNto)
            { 
                int idNto_int = 0;
                bool result = int.TryParse(idNto, out idNto_int); //Test whether idNto converts to int ok, stores int in idNto_int. Must convert.
                string formulaTo = null;

                // Id whether ToId has default formula
            
                // Check external vars first:
                int Nvars = modelVM.VariableViewModels.Count;
                for (int i = 0; i < modelVM.VariableViewModels.Count; i++)
                {
                    if (modelVM.VariableViewModels[i].Id == idNto_int)
                    {
                        formulaTo = modelVM.VariableViewModels[i].Formula;
                    }
                }

                // Check cell-contained vars:
                if (formulaTo == null)
                { 
                    int Ncells = modelVM.ContainerViewModels.Count;
                    for (int i = 0; i < Ncells; i++)
                    {
                        // Double-check, as I can't break within the loop if I've found it already.
                        if (formulaTo == null)
                        {
                            for (int j = 0; j < modelVM.ContainerViewModels[i].VariableViewModels.Count; j++)
                            {
                                if (modelVM.ContainerViewModels[i].VariableViewModels[j].Id == idNto_int)
                                {
                                    formulaTo = modelVM.ContainerViewModels[i].VariableViewModels[j].Formula;
                                }
                            }
                        }
                    }
                }

                // If the formula converts to an int, it's a constant, so needs no editing either.
                int formulaTo_asInt;
                bool k_formula = int.TryParse(formulaTo, out formulaTo_asInt);


                if (formulaTo != "" && !k_formula)
                { 
                    // need to edit the ToID's formula, and keep track of this (so store the SYN output XML and use That HERE AS WELL!)
                    Debug.WriteLine("\n==================error=====================\nSynthInputDTOFactory 135: non-default formula to-node chosen! May result in non-stability where stability Should have happened.\n==================error=====================\n");
                }
            }

            

            var xdoc = new XDocument(
                new XElement("AnalysisInput",
                                new XAttribute("ModelName", data.ModelName),
                                new XElement("Engine",
                                    new XElement("Name", "SYN")),
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
                                                         new XElement("Function", FormulaFactory.Create(v))))
                                ),
                                new XElement("Relationships",
                                                from r in modelVM.RelationshipViewModels
                                                select new XElement("Relationship",
                                                                new XAttribute("Id", r.Id),
                                                                new XElement("FromVariableId", r.From.Id),
                                                                new XElement("ToVariableId", r.To.Id),
                                                                new XElement("Type", r.Type.ToString())),
                                                new XElement("Relationship",
                                                                new XAttribute("Id", maxCurrRelationshipN + 1),
                                                                new XElement("FromVariableId", idNfrom),
                                                                new XElement("ToVariableId", idNto),
                                                                new XElement("Type", edgeNature)
                                                )
                                )
                             )
            );

            data.ZippedXml = ZipHelper.Zip(xdoc.ToString());

            return data;
        }

        // XML hacks
        public static AnalysisInputDTO CreateFromTaggedXML(string chosenXML, string chosenXML_modelName)
        {
            // Use the BMA-loaded model the first time only
            var data = new AnalysisInputDTO();

            data.ModelName = chosenXML_modelName;
            string xmlEnd = @"  </Relationships>
</AnalysisInput>
";
            string finishedXML = chosenXML + xmlEnd;

            data.ZippedXml = ZipHelper.Zip(finishedXML);

            return data;
        }

        public static AnalysisInputDTO CreateFromStoredXML_chosenEdge(ModelViewModel modelVM, string basicXML_forAddingEdgesTo, string currentEdges)
        {
            // Use the BMA-loaded model for name only
            AnalysisInputDTO data = new AnalysisInputDTO();

            data.ModelName = modelVM.Name;
            string xmlEnd = @"  </Relationships>
</AnalysisInput>
";
            string finishedXML = basicXML_forAddingEdgesTo + currentEdges + xmlEnd;

            data.ZippedXml = ZipHelper.Zip(finishedXML);

            return data;
        }


        public static string MakeNewEdges(SynthViewModel synthVM, string inputXML, int choiceEdge, int XML_topRelationshipN)
        {
            // Add chosen edges to the model (and update the stored XML data)
            // Retrieve for Relationship: fromID, toID, Nature
            string edgeNature = synthVM.TheList[choiceEdge].Nature;       // Nature
            string parseIdLines = synthVM.TheList[choiceEdge].wholeOutput;
            string newEdgesForXML = "";
            int newEdgeIdN_start = XML_topRelationshipN + 1;

            string[] newlineDetails = parseIdLines.Split('\n');
            foreach (string parseIds in newlineDetails)
            {
                // FromId
                string[] varLineNodes = parseIds.Split(';');
                string[] idNfrom_string = varLineNodes[0].Split('=');
                string idNfrom = idNfrom_string[1].Trim();

                // ToId
                string[] idNto_string = varLineNodes[6].Split('=');
                string idNto = idNto_string[2].Trim();

                newEdgesForXML = newEdgesForXML + @"    <Relationship Id=""" + newEdgeIdN_start + @""">
      <FromVariableId>" + idNfrom + @"</FromVariableId>
      <ToVariableId>" + idNto + @"</ToVariableId>
      <Type>" + edgeNature + @"</Type>
    </Relationship>
";

                newEdgeIdN_start++;
            }
            return newEdgesForXML;
        }

        public static AnalysisInputDTO CreateFromTaggedXML_chosenEdge(string chosenXML, string newEdgesForXML, string chosenXML_modelName)
        {
            // Use the BMA-loaded model the first time only
            var data = new AnalysisInputDTO();
            data.ModelName = chosenXML_modelName;

            string xmlEnd = @"  </Relationships>
</AnalysisInput>
";
            string finishedXML = chosenXML + newEdgesForXML + xmlEnd;
            data.ZippedXml = ZipHelper.Zip(finishedXML);

            return data;
        }
    }
}