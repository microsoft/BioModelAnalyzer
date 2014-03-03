using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
using System.Xml;
using System.ServiceModel;
using BioCheck.AnalysisService;
using BioCheck.Services;
using BioCheck.Helpers;
using BioCheck.ViewModel.Cells;
using BioCheck.ViewModel.Proof;
using BioCheck.ViewModel.Factories;
using MvvmFx.Common.ViewModels.Behaviors.Observable;
using MvvmFx.Common.ViewModels.Commands;
using Microsoft.Practices.Unity;
using System.Windows.Media;
using System.Windows.Controls;
using System.Collections.ObjectModel;           // For ObservableCollection
// using FurtherTestingOutputDTO = BioCheck.AnalysisService.FurtherTestingOutputDTO;


// Synth edit
namespace BioCheck.ViewModel.Synth
{

    public class CarolinesObject
    {
        public string Score { get; set; }
        public string EdgeExplanation { get; set; }
        public string Nature { get; set; }
        public string wholeOutput { get; set; }
    }

    public class XMLObject
    {
        public string XMLfile { get; set; }
        public string XMLfilename { get; set; }
        public int XMLmaxRelationshipN { get; set; }
    }

    public class SynthViewModel : ObservableViewModel
    {
        private readonly DelegateCommand synthCommand;
        private readonly DelegateCommand saveCommand;
        private readonly DelegateCommand closeCommand;
        private readonly DelegateCommand cancelSynthCommand;
        private readonly DelegateCommand consoleCommand;
        private readonly DelegateCommand runSimulation;
        private readonly DelegateCommand runProve;

        private ObservableCollection<CarolinesObject> theList = new ObservableCollection<CarolinesObject>();        // GavinEdit
        private ObservableCollection<XMLObject> theXMLlist = new ObservableCollection<XMLObject>();        // GavinEdit

        private AnalysisServiceClient analyzerClient;
        private DateTime timer;
        private string modelName;
        private bool rejectEnabled = false;
        private SolidColorBrush rejectColor = new SolidColorBrush(Colors.Gray);
        private int synPath = 100;
        private string synInput = "True";   
        private string synOutput = "Click 'Seek edges' to check whether your loaded network model is stable,\nand if not stable, seek interactions that stabilise it.";
        private string greenButtonText = "Seek edges";
        private int topRelationship_IdN = 0;
        private int selectedEdge;
        private int selectedXML;
        private string stabilizingChosenEdges = null;
        private int stabilizingChosenEdges_N = 0;
        private string currentEdges = "";
        private string basicXML_forAddingEdgesTo = "";

        SolidColorBrush RejectColorBlack = new SolidColorBrush(Colors.Black);
        SolidColorBrush RejectColorGray = new SolidColorBrush(Colors.Gray);

        // XML pre-tagged.
        private string xmltext = "For multicellular models,\nchoose a pre-tagged input file:";   


        public SynthViewModel()
        {
            this.synthCommand = new DelegateCommand(OnSynthExecuted);
            this.saveCommand = new DelegateCommand(OnSaveExecuted);
            this.closeCommand = new DelegateCommand(OnCloseExecuted);
            this.cancelSynthCommand = new DelegateCommand(OnCancelSynthExecuted);
            this.consoleCommand = new DelegateCommand(OnConsoleExecuted);
            this.runSimulation = new DelegateCommand(OnRunSimulationExecuted);
            this.runProve = new DelegateCommand(OnRunProveExecuted);
            SelectedEdge = -1;

            // XML related
            SelectedXML = -1;
            addXMLs();
        }

        public ObservableCollection<XMLObject> TheXMLlist            // What's bound to in the XAML
        {
            get { return this.theXMLlist; }          // GavinEdit
        }

        public DelegateCommand SynthCommand
        {
            get { return this.synthCommand; }
        }

        public DelegateCommand SaveCommand
        {
            get { return this.saveCommand; }
        }

        public DelegateCommand SYNSimulation
        {
            get { return this.runSimulation; }
        }

        public DelegateCommand SYNProve
        {
            get { return this.runProve; }
        }

        // --------------------------
        //      SYN Backend INPUT
        // --------------------------
        private AnalysisInputDTO synthInputDto;
        private void OnSynthExecuted()
        {
            // Sanity check: Check that the model is active (early on at startup, it's not active)
            // And that there is a model loaded at all.
            if (!ApplicationViewModel.Instance.HasActiveModel)
            {
                ApplicationViewModel.Instance.Container
                 .Resolve<IMessageWindowService>()
                 .Show("There is no active model to stabilize. Please load a model to continue.");
                return;
            } 
            else
            {
                if (GreenButtonText == "Seek edges")
                { 
                    // Brand new run.
                    ApplicationViewModel.Instance.Container
                        .Resolve<IBusyIndicatorService>()
                        .Show("Synthesizing...", CancelSynthCommand);

                    // Reset variables
                    this.stabilizingChosenEdges = null;
                    this.stabilizingChosenEdges_N = 0;
                    basicXML_forAddingEdgesTo = "";
                    currentEdges = "";

                    if (SelectedXML > 0)
                    {
                        // Use pre-loaded, tagged XML
                        synthInputDto = SynthInputDTOFactory.CreateFromTaggedXML(this.TheXMLlist[SelectedXML].XMLfile, this.TheXMLlist[SelectedXML].XMLfilename);
                    }
                    else
                    {
                        // single-cell, BMA-loaded model
                        // Create the Analysis Input Data from the active Model ViewModel
                        var modelVM = ApplicationViewModel.Instance.ActiveModel;
                        synthInputDto = SynthInputDTOFactory.Create(modelVM, this);
                    }
                } 
                else
                {
                    // Have reached F# at least once. Needs user to select what edge to incorporate.
                    if (SelectedEdge < 0)
                    {
                        this.SYNOutput = "Please select an edge from the list below.";
                        return;
                    }
                    else
                    { 
                        ApplicationViewModel.Instance.Container
                            .Resolve<IBusyIndicatorService>()
                            .Show("Synthesizing...", CancelSynthCommand);

                        if (SelectedXML <= 0)
                        {
                            // Use loaded, single-cell model.

                            // Check if edges have been added. 
                            // If no, use the BMA-loaded model's max N edges and raw network.
                            
                            // Create the Analysis Input Data from the active Model ViewModel
                            var modelVM = ApplicationViewModel.Instance.ActiveModel;

                            // Retrieve the top idN
                            topRelationship_IdN = 0;    // Reset.
                            foreach (var r in modelVM.RelationshipViewModels)
                            {
                                if (topRelationship_IdN < r.Id)
                                {
                                    topRelationship_IdN = r.Id;
                                }
                            }
                            Debug.WriteLine("194: N of edges currently in the model = " + topRelationship_IdN.ToString());
                            Debug.WriteLine("Sending selected edge index " + this.SelectedEdge.ToString() + " to SYN.");
                            
                            // If user-assigned edges added, edit the maxIdN and prepare the XML to send to backend
                            if (currentEdges == "")
                            {
                                // First edge to be added. 
                                // Store curr chosen edge as XML-style string (easy to append next time)
                                currentEdges = SynthInputDTOFactory.CreateNewEdgeString(this, SelectedEdge, topRelationship_IdN);

                                // Store the curr BMA loaded model as a string that's easy to append edges to in teh future
                                basicXML_forAddingEdgesTo = SynthInputDTOFactory.CreateAppendableXMLString(modelVM);

                                // Now that strings that need saving are saved, add the edge and zip up for F# processing.
                                synthInputDto = SynthInputDTOFactory.Create(modelVM, this, SelectedEdge, topRelationship_IdN); 

                            }
                            else
                            {
                                // Edges were previously added. 
                                // They need to be incorporated before the current edge choice.
                                // Edit the max id N to reflect the current added edges.
                                topRelationship_IdN = topRelationship_IdN + currentNedges();

                                // Make an XML-tyle string to for the newly added edge, providing the current top relationship N id
                                string newRelationships = SynthInputDTOFactory.CreateNewEdgeString(this, SelectedEdge, topRelationship_IdN);
                                //string newRelationships = SynthInputDTOFactory.MakeNewEdges(this, this.TheXMLlist[SelectedXML].XMLfile, SelectedEdge, currentMaxIdN);
                                // Update the added edges, if any:
                                currentEdges = currentEdges + newRelationships;

                                // Build the XML from past stored and the new edge
                                synthInputDto = SynthInputDTOFactory.CreateFromStoredXML_chosenEdge(modelVM, this.basicXML_forAddingEdgesTo, currentEdges);
                            }
                        }
                        else
                        {
                            int currentMaxIdN = this.TheXMLlist[SelectedXML].XMLmaxRelationshipN;
                            if (currentEdges != "")
                            {
                                // Edges previously added. Edit the max id N to reflect the current added edges.
                                currentMaxIdN = this.TheXMLlist[SelectedXML].XMLmaxRelationshipN + currentNedges();
                            }
                            // Multicellular pre-tagged XML chosen
                            string newRelationships = SynthInputDTOFactory.MakeNewEdges(this, this.TheXMLlist[SelectedXML].XMLfile, SelectedEdge, currentMaxIdN);

                            // Update the added edges, if any:
                            currentEdges = currentEdges + newRelationships;

                            synthInputDto = SynthInputDTOFactory.CreateFromTaggedXML_chosenEdge(this.TheXMLlist[SelectedXML].XMLfile, currentEdges, this.TheXMLlist[SelectedXML].XMLfilename);
                        }
                        // Update the string that keeps track of what edges are added (whether multicellular or not)
                        StabilizingChosenEdges = this.TheList[SelectedEdge].EdgeExplanation;      
                    }
                }
                
                // Enable/Disable logging
                //synthInputDto.EnableLogging = ApplicationViewModel.Instance.ToolbarViewModel.EnableAnalyzerLogging;
                synthInputDto.EnableLogging = true;         // Could be a variable (if so, make reachable from SYN window: tickbox?). Hardcoded here.

                // Create the analyzer client
                if (analyzerClient == null)
                {
                    var serviceUri = new Uri("../Services/AnalysisService.svc", UriKind.Relative);
                    var endpoint = new EndpointAddress(serviceUri);
                    analyzerClient = new AnalysisServiceClient("AnalysisServiceCustom", endpoint);
                    analyzerClient.AnalyzeCompleted += OnSynthCompleted;
                }

                // Invoke the async Analyze method on the service
                timer = DateTime.Now;
                analyzerClient.AnalyzeAsync(synthInputDto);                // Result in an AnalysisService.svc.cs input with a Proof signature.
            }
        }

        // --------------------------
        //      SYN Backend OUTPUT
        // --------------------------
        private SynthOutput synthOutput;
        private void OnSynthCompleted(object sender, AnalyzeCompletedEventArgs e)           // This is an object in Reference.cs!
        {
            
            if (e.Error != null)
            {
                OnSynthError(e.Error);
            }
            else
            {
                try
                { 
                     
                    // Get the resulting dictionary of variables and their values
                    this.synthOutput = SynthOutputFactory.Create(e.Result);

                    // Check the Result field: if "Single Stable Point", then all don: change GreenButtonText and run no else
                    string result = synthOutput.Status;
                    if (result == "Single Stable Point")
                    {
                        // Done!
                        if (StabilizingChosenEdges != null)
                        {
                            if (SelectedXML > 0 && currentEdges != "")
                            {
                                stabilizingChosenEdges_N = currentNedges();
                            }

                            if (stabilizingChosenEdges_N == 1)
                            {
                                // Only one stabilizing edge required: singular
                                this.theList.Clear();       // Clear the Listbox with suggested edges
                                this.SYNOutput = "The network " + synthInputDto.ModelName + " is stabilized by adding the following edge:\n" + StabilizingChosenEdges;
                                this.GreenButtonText = "Seek edges";
                                this.RejectEnabled = true;
                                this.RejectColor = RejectColorBlack;
                            }
                            else
                            {
                                // Multiple edges were required: plural
                                this.theList.Clear();       // Clear the Listbox with suggested edges
                                this.SYNOutput = "The network " + synthInputDto.ModelName + " is stabilized by adding the following " +  stabilizingChosenEdges_N + " edges:\n" + StabilizingChosenEdges;
                                this.GreenButtonText = "Seek edges";
                                this.RejectEnabled = true;
                                this.RejectColor = RejectColorBlack;
                            }
                        }
                        else
                        {
                            this.SYNOutput = "The network " + synthInputDto.ModelName + " is already stable.";
                            this.GreenButtonText = "Seek edges";
                            this.RejectEnabled = false;
                            this.RejectColor = RejectColorGray;
                        }
                    }
                    else 
                    { 
                        // Check if > 1.0 edges were proposed 
                        string logString = OnShowAnalyzerLogExecuted();
                        int N_stabilizingEdges = this.theList.Count;
                        if (N_stabilizingEdges > 0)
                        {
                            if (this.GreenButtonText == "Seek edges")
                            {
                                // First round of edge-retrieval. Msg reflects this
                                this.SYNOutput = "Stabilizing edges found.\nPlease add an edge from the list below and run again.";
                                this.GreenButtonText = "Add edge and rerun";
                                this.RejectEnabled = false;
                                this.RejectColor = RejectColorGray;
                            }
                            else
                            {
                                // Edges already added. More needed.
                                this.SYNOutput = "Stability not yet reached; more stabilizing interactions are required.\nPlease add an edge from the list below and run again.";
                                this.GreenButtonText = "Add edge and rerun";
                                this.RejectEnabled = false;
                                this.RejectColor = RejectColorGray;
                            }
                            
                        }
                        else if (synthOutput.ErrorMessages != null && synthOutput.ErrorMessages.Count() > 0)
                        {
                            // Error
                            string errorMsg = BuildAnalyisErrrorMessage(synthOutput);
                            this.SYNOutput = "An error occurred.\n" + errorMsg;
                            this.GreenButtonText = "Seek edges";
                            this.RejectEnabled = false;
                            this.RejectColor = RejectColorGray;
                        }
                        else { 
                            // Stability couldn't be reached
                            this.SYNOutput = "The network " + synthInputDto.ModelName + " could not be stabilized by any added edges.";
                            this.GreenButtonText = "Seek edges";
                            this.RejectEnabled = false;
                            this.RejectColor = RejectColorGray;
                        }
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
                    OnSynthError(ex);
                    this.SYNOutput = "There was an error.";
                    this.GreenButtonText = "Seek edges";
                    this.RejectEnabled = false;
                    this.RejectColor = RejectColorGray;
                }
            }
        }

        private int currentNedges()
        {
            stabilizingChosenEdges_N = -1;
            string[] newlineDetails = this.currentEdges.Split('\n');
            foreach (string edge in newlineDetails)
            {
                stabilizingChosenEdges_N++;
            }
            return stabilizingChosenEdges_N / 5;
        }

        // Log messages ------------------------------------------------------------

        private string OnShowAnalyzerLogExecuted()
        {
            if (synthOutput == null)                     // wap for synthOutput? was analysisOutput
            {
                ApplicationViewModel.Instance.Container
                     .Resolve<IMessageWindowService>()
                     .Show("Please run the analysis to create the log.");
                return "Please run synthesis to create the log.";
            }

            string details = BuildAnalyisMessage(synthOutput);
            return details;

            //ApplicationViewModel.Instance.Container
            //            .Resolve<ILogWindowService>()
            //            .Show(details);
        }

        private string BuildAnalyisErrrorMessage(SynthOutput output)
        {
            string details = synthOutput.Error;

            details += Environment.NewLine;
            details += Environment.NewLine;

            details += BuildAnalyisMessage(output);
            return details;
        }

        // Create the Log string
        private string BuildAnalyisMessage(SynthOutput output)
        {
            string details = "";

            // If error:
            if (output.ErrorMessages != null && output.ErrorMessages.Count() > 0)
            {
                details = "Error Messages:";
                details += Environment.NewLine;

                var log = String.Join(Environment.NewLine, output.ErrorMessages);
                details = details + Environment.NewLine + log;

                details += Environment.NewLine;
            }

            // Log messages!
            if (output.Dto.ZippedLog != null)
            {
                var debug = ZipHelper.Unzip(output.Dto.ZippedLog);

                // Parse out "Edges:" from log
                string parsedDebug = null;
                int top10 = 1;
                bool varLine = false;
                int varLineN = 0;
                string multicell_edges = "";
                bool natureLine = false;
                bool scoreLine = false;
                string name = "";
                string nameTo = "";
                string edge = null;

                List<double> edgeScoreList = new List<double>();
                List<string> edgeList = new List<string>();
                List<string> natureList = new List<string>();
                List<string> fullOutputList = new List<string>();

                string[] newlineDetails = debug.Split('\n');
                foreach (string s in newlineDetails)
                {
                    if (s.Contains("Edges:"))
                    {
                        varLine = true;
                    }
                    else if (varLine)
                    {
                        if (SelectedXML <= 0)
                        {
                            // SINGLE CELL 
                            // there is one suggestion per line.
                            fullOutputList.Add(s);

                            //parsedDebug = parsedDebug + "Working on: " + s + ":\n";
                            parsedDebug = parsedDebug + "\n\n" + top10 + ". ";
                            string[] varLineNodes = s.Split('}');

                            //From: Name, IdN (from which test can be traced IFF underlying model. But I store a new one!
                            string[] varLineNamePart = varLineNodes[0].Split(';');
                            string[] namebit = varLineNamePart[2].Split('=');
                            name = namebit[1];
                            //.Substring(6, (varLineNamePart[2].Length -6));
                            //parsedDebug = parsedDebug + name + " to ";

                            //To: Name, IdN (from which test can be traced)
                            string[] varLineNamePartTo = varLineNodes[2].Split(';');
                            string[] namebit2 = varLineNamePartTo[2].Split('=');
                            nameTo = namebit2[1];
                            //.Substring(6, (varLineNamePartTo[2].Length -6));
                            //parsedDebug = parsedDebug + nameTo + ".";
                        
                            varLine = false;
                            natureLine = true;
                            top10++;
                        }
                        else
                        {
                            // MULTICELLULAR
                            // May be one or several edges proposed. Needs teasing out.
                            
                            // Incorporate Nature-line since I don't know when it comes.
                            if (s.Contains("Nature:"))
                            {
                                string[] edgeType = s.Split(' ');
                                if (edgeType[1] == "Act")
                                {
                                    parsedDebug = parsedDebug + name + " activates " + nameTo;
                                    edge = name + " activates " + nameTo;
                                    natureList.Add("Activator");
                                } else 
                                {
                                    parsedDebug = parsedDebug + name + " inhibits " + nameTo;
                                    edge = name + " inhibits " + nameTo;
                                    natureList.Add("Inhibitor");
                                }
                                scoreLine = true;
                                varLine = false;
                                varLineN = 0;
                                name = "";
                                nameTo = "";
                                fullOutputList.Add(multicell_edges);
                                multicell_edges = "";
                                top10++;
                            } 
                            else
                            {
                                // Real values.
                                if (varLineN == 0 || varLineN == 3)
                                {
                                    // For single cells, there is one suggestion per line.
                                    if (multicell_edges == "")
                                    {
                                        multicell_edges = s;
                                    }
                                    else
                                    {
                                        multicell_edges = multicell_edges + "\n" + s;
                                    }
                                    

                                    //parsedDebug = parsedDebug + "Working on: " + s + ":\n";
                                    parsedDebug = parsedDebug + "\n\n" + top10 + ". ";
                                    string[] varLineNodes = s.Split('}');

                                    //From: Name, IdN (from which test can be traced IFF underlying model. But I store a new one!
                                    string[] varLineNamePart = varLineNodes[0].Split(';');
                                    string[] namebit = varLineNamePart[2].Split('=');
                                    if (name == "")
                                    {
                                        name = namebit[1];
                                    }
                                    else
                                    {
                                        name = name + ", " + namebit[1];
                                    }
                                    
                                    //.Substring(6, (varLineNamePart[2].Length -6));
                                    //parsedDebug = parsedDebug + name + " to ";

                                    //To: Name, IdN (from which test can be traced)
                                    string[] varLineNamePartTo = varLineNodes[2].Split(';');
                                    string[] namebit2 = varLineNamePartTo[2].Split('=');
                                    if (nameTo == "")
                                    {
                                        nameTo = namebit2[1];
                                    }
                                    else
                                    {
                                        nameTo = nameTo + ", " + namebit2[1];
                                    }
                                    
                                    varLineN = 0;
                                }
                                varLineN++;
                            }
                        }
                    }
                    else if (natureLine)
                    {
                        //parsedDebug = parsedDebug + "Working on: " + s + ":\n";
                        string[] edgeType = s.Split(' ');
                        if (edgeType[1] == "Act")
                        {
                            parsedDebug = parsedDebug + name + " activates " + nameTo;
                            edge = name + " activates " + nameTo;
                            natureList.Add("Activator");
                        } else 
                        {
                            parsedDebug = parsedDebug + name + " inhibits " + nameTo;
                            edge = name + " inhibits " + nameTo;
                            natureList.Add("Inhibitor");
                        }

                        //parsedDebug = parsedDebug + "\nNature = " + s;
                        natureLine = false;
                        scoreLine = true;
                    }
                    else if (scoreLine)
                    {
                        //parsedDebug = parsedDebug + "\nWorking on: " + s + ":\n";
                        string[] stabilityScore = s.Split(':');
                        parsedDebug = parsedDebug + "\nPotential stability increase = " + stabilityScore[1];

                        string trimmedStabilityScore = stabilityScore[1].Trim();
                        double booltest = 0.0;
                        bool result = double.TryParse(trimmedStabilityScore, out booltest); //Test whether Stability converts to double ok

                        // Single cells have high scores, multicellular cells have < 1.0 scores.. use all for the list.
                        if (SelectedXML > 0)
                        {
                            edgeScoreList.Add(booltest);
                            edgeList.Add(edge);
                        }
                        else
                        {
                            // Add to the real list if stability > 1
                            if (result && booltest > 1.0)
                            {
                                edgeScoreList.Add(booltest);
                                edgeList.Add(edge);
                            }
                        }
                        scoreLine = false;
                    }
                }

                string rollup = null;
                this.theList.Clear();
                for (int i = 0; i < edgeList.Count; i++ ) // Loop through List with foreach
                {
                    rollup = rollup  + "\n" + edgeScoreList[i] + " " + edgeList[i];     // E.g. 4 NoCell activates cAMP_ic
                    this.theList.Add(new CarolinesObject { 
                        Score = edgeScoreList[i].ToString(), 
                        EdgeExplanation = edgeList[i], 
                        wholeOutput = fullOutputList[i], 
                        Nature = natureList[i] 
                    });
                }

                //details += Environment.NewLine;
                //details += "Debug Messages:";
                //details += Environment.NewLine;

                details = details + Environment.NewLine + parsedDebug + "\n\n" + rollup;
            }

            //if (proofVM != null && proofVM.FurtherTestingOutput != null)
            //{
            //    details += Environment.NewLine;
            //    details += Environment.NewLine;

            //    if (proofVM.FurtherTestingOutput.ErrorMessages != null &&
            //        proofVM.FurtherTestingOutput.ErrorMessages.Count > 0)
            //    {
            //        details += "Further Testing Error Messages:";
            //        details = details + Environment.NewLine + String.Join(Environment.NewLine, proofVM.FurtherTestingOutput.ErrorMessages);

            //        details += Environment.NewLine;
            //        details += Environment.NewLine;
            //    }

            //    details += "Further Testing Debug Messages:";
            //    details += Environment.NewLine;

            //    var cexOutputDto = proofVM.FurtherTestingOutput.Dto;
            //    var cexLog = ZipHelper.Unzip(cexOutputDto.ZippedLog);

            //    details = details + Environment.NewLine + cexLog;
            //}

            return details;
        }

        // This outputs what I put into Marshal... So v little atm. Can be changed! 
        // Edit to include suggested edges, rm actual model being displayed onto Textbox output.
        private void OnSaveExecuted()
        {
            if (synthOutput == null)
            {
                ApplicationViewModel.Instance.Container
                 .Resolve<IMessageWindowService>()
                 .Show("Please run Synthesis to create the output.");
                return;
            }

            AnalysisInputDTO modelVM;
            if (SelectedXML > 0)
            {
                // multicellular output
                modelVM = SynthInputDTOFactory.CreateFromTaggedXML_chosenEdge(this.TheXMLlist[SelectedXML].XMLfile, currentEdges, this.TheXMLlist[SelectedXML].XMLfilename);
            }
            else
            {
                // unicellular output
                //modelVM = ApplicationViewModel.Instance.ActiveModel;  
                //modelVM = SynthInputDTOFactory.CreateFromTaggedXML_chosenEdge(this.TheXMLlist[SelectedXML].XMLfile, currentEdges, this.TheXMLlist[SelectedXML].XMLfilename);
                var modelVM_sys = ApplicationViewModel.Instance.ActiveModel;
                modelVM = SynthInputDTOFactory.CreateFromStoredXML_chosenEdge(modelVM_sys, this.basicXML_forAddingEdgesTo, currentEdges);
            }

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "XML files (*.xml)|*.xml";
            saveFileDialog.DefaultExt = ".xml";
            saveFileDialog.DefaultFileName = modelVM.ModelName + " BMASynthesisOutput";

            if (saveFileDialog.ShowDialog() == true)
            {
                //saveFileDialog.ShowDialog();
                using (var stream = saveFileDialog.OpenFile())
                {
                    //var xml = ZipHelper.Unzip(synthOutput.Dto.ZippedXml);
                    var xml = ZipHelper.Unzip(modelVM.ZippedXml);
                    var xdoc = XDocument.Parse(xml);

                    var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings() { Indent = true });
                    xdoc.Save(xmlWriter);
                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
        }

        // Log messages end ------------------------------------------------------------


        private void OnSynthError(Exception ex)
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
        public int SYNPath
        {
            get { return this.synPath; }
            set
            {
                if (this.synPath != value)
                {
                    this.synPath = value;
                    OnPropertyChanged(() => SYNPath);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="SYNInput"/> property.
        /// </summary>
        public string SYNInput
        {
            get { return this.synInput; }
            set
            {
                if (this.synInput != value)
                {
                    this.synInput = value;
                    OnPropertyChanged(() => SYNInput);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="SYNOutput"/> property.
        /// </summary>
        public string SYNOutput
        {
            get { return this.synOutput; }
            set
            {
                if (this.synOutput != value)
                {
                    this.synOutput = value;
                    OnPropertyChanged(() => SYNOutput);
                }
            }
        }

        
        /// <summary>
        /// Gets or sets the value of the <see cref="XMLtext"/> property.
        /// </summary>
        public string XMLtext
        {
            get { return this.xmltext; }
            set
            {
                if (this.xmltext != value)
                {
                    this.xmltext = value;
                    OnPropertyChanged(() => XMLtext);
                }
            }
        }

        public string StabilizingChosenEdges
        {
            get { return this.stabilizingChosenEdges; }
            set
            {
                if (this.stabilizingChosenEdges != value)
                {
                    this.stabilizingChosenEdges = stabilizingChosenEdges + "\n" + value;
                    this.stabilizingChosenEdges_N++;
                    OnPropertyChanged(() => StabilizingChosenEdges);
                }
            }
        }

        public ObservableCollection<CarolinesObject> TheList            // What's bound to in the XAML
        {
            get { return this.theList; }          // GavinEdit
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="SelectedEdge"/> property.
        /// </summary>
        public int SelectedEdge
        {
            get {
                return this.selectedEdge;
            }
            set
            {
                if (this.selectedEdge != value)
                {
                    Debug.WriteLine("868: SelectedEdge edited from getsetter to " + value);
                    this.selectedEdge = value;
                    OnPropertyChanged(() => SelectedEdge);
                }
            }
        }

        
        /// <summary>
        /// Gets or sets the value of the <see cref="SelectedXML"/> property.
        /// </summary>
        public int SelectedXML
        {
            get {
                return this.selectedXML;
            }
            set
            {
                if (this.selectedXML != value)
                {
                    this.selectedXML = value;
                    OnPropertyChanged(() => SelectedXML);
                }
            }
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="GreenButtonText"/> property.
        /// </summary>
        public string GreenButtonText
        {
            get { return this.greenButtonText; }
            set
            {
                if (this.greenButtonText != value)
                {
                    this.greenButtonText = value;
                    OnPropertyChanged(() => GreenButtonText);

                }
            }
        }

        public bool RejectEnabled
        {
            get { return this.rejectEnabled; }
            set
            {
                if (this.rejectEnabled != value)
                {
                    this.rejectEnabled = value; 
                    OnPropertyChanged(() => RejectEnabled);                    
                }
            }
        }

        public SolidColorBrush RejectColor
        {
            get { return this.rejectColor; }
            set
            {
                if (this.rejectColor != value)
                {
                    this.rejectColor = value;         
                    OnPropertyChanged(() => RejectColor);
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
                .Resolve<ISynthWindowService>()
                .Close();
        }


        public DelegateCommand CancelSynthCommand
        {
            get { return this.cancelSynthCommand; }
        }


        public DelegateCommand ConsoleCommand
        {
            get { return this.consoleCommand; }
        }

        private void OnCancelSynthExecuted()
        {
            if (analyzerClient != null)
            {
                //analyzerClient.SynthCompleted -= OnSynthCompleted;
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
            this.rejectEnabled = false;
        }

        private void OnRunProveExecuted()
        {
            this.rejectEnabled = true;
        }
    
        // Passing View-only data back to ViewModel! Pretty clever. Was internal void .. private made it uncallable.
        public void runFunWhenListBoxSelChanges(int selIndex)
        {   
            //if (selIndex < 0)
            //{
            //    this.SelectedEdge = 0;
            //    Debug.WriteLine("sel was <0! Correct..");
            //    return 0;
            //}
            //else
            //{

            SelectedEdge = selIndex;       // Will be -1 when nothing's selected.
            Debug.WriteLine("776: Chose index: " + SelectedEdge.ToString());

            //    return selIndex;
            //}

            // Eventually do something cleverer, like draw the edge out.
        }

        public void runFunWhenXMLListBoxSelChanges(int selXMLIndex)
        {
            SelectedXML = selXMLIndex;       // Will be -1 when nothing's selected.
            Debug.WriteLine("783: Chose XML file index: " + SelectedXML.ToString());
        }

        // Hard-coded tagged XML data
        private void addXMLs()
        {
            this.theXMLlist.Add(new XMLObject
            {
                XMLfilename = "Single cell (use loaded BMA model)"
            });

            this.theXMLlist.Add(new XMLObject
            {
                XMLmaxRelationshipN = 56,
                XMLfilename = "C.elegans vulval precursor cells",
                XMLfile = @"<AnalysisInput ModelName=""C.elegans vulval precursor cells"">
    <Engine>
	  <Name>SYN</Name>
    </Engine>
    <Cells>
              <Cell Name=""A""></Cell>
          <Cell Name=""B""></Cell>
        <Cell Name=""C""></Cell>
        <Cell Name=""D""></Cell>
        <Cell Name=""E""></Cell>
        <Cell Name=""F""></Cell>
        <Cell Name=""G""></Cell>
    </Cells>
    <Variables>
	  <Variable Id=""1"">
      <Name>LIN3</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function>2</Function>
      <Number>12</Number>
      <Tags><Tag Id=""1"" Name=""G""></Tag></Tags>
    </Variable>
    <Variable Id=""2"">
      <Name>FarL</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function>var(1)-2</Function>
      <Number>15</Number>
      <Tags><Tag Id=""1"" Name=""G""></Tag></Tags>
    </Variable>
    <Variable Id=""3"">
      <Name>NearL</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function>var(1)-1</Function>
      <Number>14</Number>
      <Tags><Tag Id=""1"" Name=""G""></Tag></Tags>
    </Variable>
    <Variable Id=""4"">
      <Name>Next</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function>var(1)</Function>
      <Number>13</Number>
      <Tags><Tag Id=""1"" Name=""G""></Tag></Tags>
    </Variable>
    <Variable Id=""5"">
      <Name>NearR</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function>var(1)-1</Function>
      <Number>16</Number>
      <Tags><Tag Id=""1"" Name=""G""></Tag></Tags>
    </Variable>
    <Variable Id=""6"">
      <Name>FarR</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function>var(1)-2</Function>
      <Number>17</Number>
      <Tags><Tag Id=""1"" Name=""G""></Tag></Tags>
    </Variable>
    <Variable Id=""7"">
      <Name>LET23_p3P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>1</Number>
      <Tags><Tag Id=""1"" Name=""A""></Tag></Tags>
    </Variable>
    <Variable Id=""8"">
      <Name>LIN12R_p3P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>8</Number>
      <Tags><Tag Id=""1"" Name=""A""></Tag></Tags>
    </Variable>
    <Variable Id=""9"">
      <Name>LSR_p3P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>7</Number>
      <Tags><Tag Id=""1"" Name=""A""></Tag></Tags>
    </Variable>
    <Variable Id=""10"">
      <Name>LIN12L_p3P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>9</Number>
      <Tags><Tag Id=""1"" Name=""A""></Tag></Tags>
    </Variable>
    <Variable Id=""11"">
      <Name>LSL_p3P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>6</Number>
      <Tags><Tag Id=""1"" Name=""A""></Tag></Tags>
    </Variable>
    <Variable Id=""12"">
      <Name>SEM5_p3P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>2</Number>
      <Tags><Tag Id=""1"" Name=""A""></Tag></Tags>
    </Variable>
    <Variable Id=""13"">
      <Name>LET60_p3P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>3</Number>
      <Tags><Tag Id=""1"" Name=""A""></Tag></Tags>
    </Variable>
    <Variable Id=""14"">
      <Name>MAPK_p3P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>4</Number>
      <Tags><Tag Id=""1"" Name=""A""></Tag></Tags>
    </Variable>
    <Variable Id=""15"">
      <Name>SUR2_p3P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>5</Number>
      <Tags><Tag Id=""1"" Name=""A""></Tag></Tags>
    </Variable>
    <Variable Id=""16"">
      <Name>LIN12IC_p3P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>10</Number>
      <Tags><Tag Id=""1"" Name=""A""></Tag></Tags>
    </Variable>
    <Variable Id=""73"">
      <Name>lst_p3P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>11</Number>
      <Tags><Tag Id=""1"" Name=""A""></Tag></Tags>
    </Variable>
    <Variable Id=""74"">
      <Name>LET23_p4P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>1</Number>
      <Tags><Tag Id=""1"" Name=""B""></Tag></Tags>
    </Variable>
    <Variable Id=""75"">
      <Name>LIN12R_p4P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>8</Number>
      <Tags><Tag Id=""1"" Name=""B""></Tag></Tags>
    </Variable>
    <Variable Id=""76"">
      <Name>LSR_p4P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>7</Number>
      <Tags><Tag Id=""1"" Name=""B""></Tag></Tags>
    </Variable>
    <Variable Id=""77"">
      <Name>LIN12L_p4P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>9</Number>
      <Tags><Tag Id=""1"" Name=""B""></Tag></Tags>
    </Variable>
    <Variable Id=""78"">
      <Name>LSL_p4P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>6</Number>
      <Tags><Tag Id=""1"" Name=""B""></Tag></Tags>
    </Variable>
    <Variable Id=""79"">
      <Name>SEM5_p4P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>2</Number>
      <Tags><Tag Id=""1"" Name=""B""></Tag></Tags>
    </Variable>
    <Variable Id=""80"">
      <Name>LET60_p4P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>3</Number>
      <Tags><Tag Id=""1"" Name=""B""></Tag></Tags>
    </Variable>
    <Variable Id=""81"">
      <Name>MAPK_p4P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>4</Number>
      <Tags><Tag Id=""1"" Name=""B""></Tag></Tags>
    </Variable>
    <Variable Id=""82"">
      <Name>SUR2_p4P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>5</Number>
      <Tags><Tag Id=""1"" Name=""B""></Tag></Tags>
    </Variable>
    <Variable Id=""83"">
      <Name>LIN12IC_p4P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>10</Number>
      <Tags><Tag Id=""1"" Name=""B""></Tag></Tags>
    </Variable>
    <Variable Id=""84"">
      <Name>lst_p4P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>11</Number>
      <Tags><Tag Id=""1"" Name=""B""></Tag></Tags>
    </Variable>
    <Variable Id=""85"">
      <Name>LET23_p5P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>1</Number>
      <Tags><Tag Id=""1"" Name=""C""></Tag></Tags>
    </Variable>
    <Variable Id=""86"">
      <Name>LIN12R_p5P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>8</Number>
      <Tags><Tag Id=""1"" Name=""C""></Tag></Tags>
    </Variable>
    <Variable Id=""87"">
      <Name>LSR_p5P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>7</Number>
      <Tags><Tag Id=""1"" Name=""C""></Tag></Tags>
    </Variable>
    <Variable Id=""88"">
      <Name>LIN12L_p5P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>9</Number>
      <Tags><Tag Id=""1"" Name=""C""></Tag></Tags>
    </Variable>
    <Variable Id=""89"">
      <Name>LSL_p5P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>6</Number>
      <Tags><Tag Id=""1"" Name=""C""></Tag></Tags>
    </Variable>
    <Variable Id=""90"">
      <Name>SEM5_p5P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>2</Number>
      <Tags><Tag Id=""1"" Name=""C""></Tag></Tags>
    </Variable>
    <Variable Id=""91"">
      <Name>LET60_p5P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>3</Number>
      <Tags><Tag Id=""1"" Name=""C""></Tag></Tags>
    </Variable>
    <Variable Id=""92"">
      <Name>MAPK_p5P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>4</Number>
      <Tags><Tag Id=""1"" Name=""C""></Tag></Tags>
    </Variable>
    <Variable Id=""93"">
      <Name>SUR2_p5P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>5</Number>
      <Tags><Tag Id=""1"" Name=""C""></Tag></Tags>
    </Variable>
    <Variable Id=""94"">
      <Name>LIN12IC_p5P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>10</Number>
      <Tags><Tag Id=""1"" Name=""C""></Tag></Tags>
    </Variable>
    <Variable Id=""95"">
      <Name>lst_p5P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>11</Number>
      <Tags><Tag Id=""1"" Name=""C""></Tag></Tags>
    </Variable>
    <Variable Id=""96"">
      <Name>LET23_p6P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>1</Number>
      <Tags><Tag Id=""1"" Name=""D""></Tag></Tags>
    </Variable>
    <Variable Id=""97"">
      <Name>LIN12R_p6P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>8</Number>
      <Tags><Tag Id=""1"" Name=""D""></Tag></Tags>
    </Variable>
    <Variable Id=""98"">
      <Name>LSR_p6P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>7</Number>
      <Tags><Tag Id=""1"" Name=""D""></Tag></Tags>
    </Variable>
    <Variable Id=""99"">
      <Name>LIN12L_p6P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>9</Number>
      <Tags><Tag Id=""1"" Name=""D""></Tag></Tags>
    </Variable>
    <Variable Id=""100"">
      <Name>LSL_p6P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>6</Number>
      <Tags><Tag Id=""1"" Name=""D""></Tag></Tags>
    </Variable>
    <Variable Id=""101"">
      <Name>SEM5_p6P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>2</Number>
      <Tags><Tag Id=""1"" Name=""D""></Tag></Tags>
    </Variable>
    <Variable Id=""102"">
      <Name>LET60_p6P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>3</Number>
      <Tags><Tag Id=""1"" Name=""D""></Tag></Tags>
    </Variable>
    <Variable Id=""103"">
      <Name>MAPK_p6P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>4</Number>
      <Tags><Tag Id=""1"" Name=""D""></Tag></Tags>
    </Variable>
    <Variable Id=""104"">
      <Name>SUR2_p6P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>5</Number>
      <Tags><Tag Id=""1"" Name=""D""></Tag></Tags>
    </Variable>
    <Variable Id=""105"">
      <Name>LIN12IC_p6P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>10</Number>
      <Tags><Tag Id=""1"" Name=""D""></Tag></Tags>
    </Variable>
    <Variable Id=""106"">
      <Name>lst_p6P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>11</Number>
      <Tags><Tag Id=""1"" Name=""D""></Tag></Tags>
    </Variable>
    <Variable Id=""107"">
      <Name>LET23_p7P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>1</Number>
      <Tags><Tag Id=""1"" Name=""E""></Tag></Tags>
    </Variable>
    <Variable Id=""108"">
      <Name>LIN12R_p7P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>8</Number>
      <Tags><Tag Id=""1"" Name=""E""></Tag></Tags>
    </Variable>
    <Variable Id=""109"">
      <Name>LSR_p7P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>7</Number>
      <Tags><Tag Id=""1"" Name=""E""></Tag></Tags>
    </Variable>
    <Variable Id=""110"">
      <Name>LIN12L_p7P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>9</Number>
      <Tags><Tag Id=""1"" Name=""E""></Tag></Tags>
    </Variable>
    <Variable Id=""111"">
      <Name>LSL_p7P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>6</Number>
      <Tags><Tag Id=""1"" Name=""E""></Tag></Tags>
    </Variable>
    <Variable Id=""112"">
      <Name>SEM5_p7P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>2</Number>
      <Tags><Tag Id=""1"" Name=""E""></Tag></Tags>
    </Variable>
    <Variable Id=""113"">
      <Name>LET60_p7P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>3</Number>
      <Tags><Tag Id=""1"" Name=""E""></Tag></Tags>
    </Variable>
    <Variable Id=""114"">
      <Name>MAPK_p7P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>4</Number>
      <Tags><Tag Id=""1"" Name=""E""></Tag></Tags>
    </Variable>
    <Variable Id=""115"">
      <Name>SUR2_p7P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>5</Number>
      <Tags><Tag Id=""1"" Name=""E""></Tag></Tags>
    </Variable>
    <Variable Id=""116"">
      <Name>LIN12IC_p7P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>10</Number>
      <Tags><Tag Id=""1"" Name=""E""></Tag></Tags>
    </Variable>
    <Variable Id=""117"">
      <Name>lst_p7P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>11</Number>
      <Tags><Tag Id=""1"" Name=""E""></Tag></Tags>
    </Variable>
    <Variable Id=""118"">
      <Name>LET23_p8P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>1</Number>
      <Tags><Tag Id=""1"" Name=""F""></Tag></Tags>
    </Variable>
    <Variable Id=""119"">
      <Name>LIN12R_p8P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>8</Number>
      <Tags><Tag Id=""1"" Name=""F""></Tag></Tags>
    </Variable>
    <Variable Id=""120"">
      <Name>LSR_p8P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>7</Number>
      <Tags><Tag Id=""1"" Name=""F""></Tag></Tags>
    </Variable>
    <Variable Id=""121"">
      <Name>LIN12L_p8P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>9</Number>
      <Tags><Tag Id=""1"" Name=""F""></Tag></Tags>
    </Variable>
    <Variable Id=""122"">
      <Name>LSL_p8P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>6</Number>
      <Tags><Tag Id=""1"" Name=""F""></Tag></Tags>
    </Variable>
    <Variable Id=""123"">
      <Name>SEM5_p8P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>2</Number>
      <Tags><Tag Id=""1"" Name=""F""></Tag></Tags>
    </Variable>
    <Variable Id=""124"">
      <Name>LET60_p8P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>3</Number>
      <Tags><Tag Id=""1"" Name=""F""></Tag></Tags>
    </Variable>
    <Variable Id=""125"">
      <Name>MAPK_p8P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>4</Number>
      <Tags><Tag Id=""1"" Name=""F""></Tag></Tags>
    </Variable>
    <Variable Id=""126"">
      <Name>SUR2_p8P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>5</Number>
      <Tags><Tag Id=""1"" Name=""F""></Tag></Tags>
    </Variable>
    <Variable Id=""127"">
      <Name>LIN12IC_p8P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>10</Number>
      <Tags><Tag Id=""1"" Name=""F""></Tag></Tags>
    </Variable>
    <Variable Id=""128"">
      <Name>lst_p8P</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>2</RangeTo>
      <Function></Function>
      <Number>11</Number>
      <Tags><Tag Id=""1"" Name=""F""></Tag></Tags>
    </Variable>
  </Variables>
  <Relationships>
    <Relationship Id=""1"">
      <FromVariableId>1</FromVariableId>
      <ToVariableId>4</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""2"">
      <FromVariableId>1</FromVariableId>
      <ToVariableId>3</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""3"">
      <FromVariableId>1</FromVariableId>
      <ToVariableId>2</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""4"">
      <FromVariableId>1</FromVariableId>
      <ToVariableId>5</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""5"">
      <FromVariableId>1</FromVariableId>
      <ToVariableId>6</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""6"">
      <FromVariableId>7</FromVariableId>
      <ToVariableId>12</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""7"">
      <FromVariableId>12</FromVariableId>
      <ToVariableId>13</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""8"">
      <FromVariableId>13</FromVariableId>
      <ToVariableId>14</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""9"">
      <FromVariableId>14</FromVariableId>
      <ToVariableId>9</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""10"">
      <FromVariableId>14</FromVariableId>
      <ToVariableId>11</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""11"">
      <FromVariableId>14</FromVariableId>
      <ToVariableId>15</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""13"">
      <FromVariableId>15</FromVariableId>
      <ToVariableId>16</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""15"">
      <FromVariableId>10</FromVariableId>
      <ToVariableId>16</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""16"">
      <FromVariableId>8</FromVariableId>
      <ToVariableId>16</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""28"">
      <FromVariableId>16</FromVariableId>
      <ToVariableId>73</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""29"">
      <FromVariableId>73</FromVariableId>
      <ToVariableId>14</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""31"">
      <FromVariableId>79</FromVariableId>
      <ToVariableId>80</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""32"">
      <FromVariableId>80</FromVariableId>
      <ToVariableId>81</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""35"">
      <FromVariableId>81</FromVariableId>
      <ToVariableId>82</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""36"">
      <FromVariableId>82</FromVariableId>
      <ToVariableId>83</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""39"">
      <FromVariableId>83</FromVariableId>
      <ToVariableId>84</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""40"">
      <FromVariableId>84</FromVariableId>
      <ToVariableId>81</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""30"">
      <FromVariableId>74</FromVariableId>
      <ToVariableId>79</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""38"">
      <FromVariableId>75</FromVariableId>
      <ToVariableId>83</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""33"">
      <FromVariableId>81</FromVariableId>
      <ToVariableId>76</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""37"">
      <FromVariableId>77</FromVariableId>
      <ToVariableId>83</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""34"">
      <FromVariableId>81</FromVariableId>
      <ToVariableId>78</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""31"">
      <FromVariableId>90</FromVariableId>
      <ToVariableId>91</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""32"">
      <FromVariableId>91</FromVariableId>
      <ToVariableId>92</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""35"">
      <FromVariableId>92</FromVariableId>
      <ToVariableId>93</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""36"">
      <FromVariableId>93</FromVariableId>
      <ToVariableId>94</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""39"">
      <FromVariableId>94</FromVariableId>
      <ToVariableId>95</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""40"">
      <FromVariableId>95</FromVariableId>
      <ToVariableId>92</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""30"">
      <FromVariableId>85</FromVariableId>
      <ToVariableId>90</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""38"">
      <FromVariableId>86</FromVariableId>
      <ToVariableId>94</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""33"">
      <FromVariableId>92</FromVariableId>
      <ToVariableId>87</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""37"">
      <FromVariableId>88</FromVariableId>
      <ToVariableId>94</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""34"">
      <FromVariableId>92</FromVariableId>
      <ToVariableId>89</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""31"">
      <FromVariableId>101</FromVariableId>
      <ToVariableId>102</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""32"">
      <FromVariableId>102</FromVariableId>
      <ToVariableId>103</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""35"">
      <FromVariableId>103</FromVariableId>
      <ToVariableId>104</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""36"">
      <FromVariableId>104</FromVariableId>
      <ToVariableId>105</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""39"">
      <FromVariableId>105</FromVariableId>
      <ToVariableId>106</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""40"">
      <FromVariableId>106</FromVariableId>
      <ToVariableId>103</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""30"">
      <FromVariableId>96</FromVariableId>
      <ToVariableId>101</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""38"">
      <FromVariableId>97</FromVariableId>
      <ToVariableId>105</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""33"">
      <FromVariableId>103</FromVariableId>
      <ToVariableId>98</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""37"">
      <FromVariableId>99</FromVariableId>
      <ToVariableId>105</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""34"">
      <FromVariableId>103</FromVariableId>
      <ToVariableId>100</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""31"">
      <FromVariableId>112</FromVariableId>
      <ToVariableId>113</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""32"">
      <FromVariableId>113</FromVariableId>
      <ToVariableId>114</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""35"">
      <FromVariableId>114</FromVariableId>
      <ToVariableId>115</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""36"">
      <FromVariableId>115</FromVariableId>
      <ToVariableId>116</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""39"">
      <FromVariableId>116</FromVariableId>
      <ToVariableId>117</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""40"">
      <FromVariableId>117</FromVariableId>
      <ToVariableId>114</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""30"">
      <FromVariableId>107</FromVariableId>
      <ToVariableId>112</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""38"">
      <FromVariableId>108</FromVariableId>
      <ToVariableId>116</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""33"">
      <FromVariableId>114</FromVariableId>
      <ToVariableId>109</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""37"">
      <FromVariableId>110</FromVariableId>
      <ToVariableId>116</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""34"">
      <FromVariableId>114</FromVariableId>
      <ToVariableId>111</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""31"">
      <FromVariableId>123</FromVariableId>
      <ToVariableId>124</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""32"">
      <FromVariableId>124</FromVariableId>
      <ToVariableId>125</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""35"">
      <FromVariableId>125</FromVariableId>
      <ToVariableId>126</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""36"">
      <FromVariableId>126</FromVariableId>
      <ToVariableId>127</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""39"">
      <FromVariableId>127</FromVariableId>
      <ToVariableId>128</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""40"">
      <FromVariableId>128</FromVariableId>
      <ToVariableId>125</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""30"">
      <FromVariableId>118</FromVariableId>
      <ToVariableId>123</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""38"">
      <FromVariableId>119</FromVariableId>
      <ToVariableId>127</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""33"">
      <FromVariableId>125</FromVariableId>
      <ToVariableId>120</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""37"">
      <FromVariableId>121</FromVariableId>
      <ToVariableId>127</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""34"">
      <FromVariableId>125</FromVariableId>
      <ToVariableId>122</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""41"">
      <FromVariableId>4</FromVariableId>
      <ToVariableId>96</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""42"">
      <FromVariableId>5</FromVariableId>
      <ToVariableId>107</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""43"">
      <FromVariableId>3</FromVariableId>
      <ToVariableId>85</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""44"">
      <FromVariableId>2</FromVariableId>
      <ToVariableId>74</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""45"">
      <FromVariableId>6</FromVariableId>
      <ToVariableId>118</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""46"">
      <FromVariableId>2</FromVariableId>
      <ToVariableId>7</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""47"">
      <FromVariableId>100</FromVariableId>
      <ToVariableId>86</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""48"">
      <FromVariableId>89</FromVariableId>
      <ToVariableId>75</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""49"">
      <FromVariableId>78</FromVariableId>
      <ToVariableId>8</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""50"">
      <FromVariableId>111</FromVariableId>
      <ToVariableId>97</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""51"">
      <FromVariableId>122</FromVariableId>
      <ToVariableId>108</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""52"">
      <FromVariableId>109</FromVariableId>
      <ToVariableId>121</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""53"">
      <FromVariableId>98</FromVariableId>
      <ToVariableId>110</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""54"">
      <FromVariableId>87</FromVariableId>
      <ToVariableId>99</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""55"">
      <FromVariableId>76</FromVariableId>
      <ToVariableId>88</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""56"">
      <FromVariableId>9</FromVariableId>
      <ToVariableId>77</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
"
            });
            this.theXMLlist.Add(new XMLObject
            {
                XMLmaxRelationshipN = 115,
                XMLfilename = "Mammalian Epidermis - Wnt and Notch signalling",
                XMLfile = @"<AnalysisInput ModelName=""Mammalian Epidermis - Wnt and Notch signalling"">
  <Engine>
    <Name>SYN</Name>
  </Engine>
  <Cells>
	<Cell Name=""A""></Cell>
	<Cell Name=""B""></Cell>
	<Cell Name=""C""></Cell>
	<Cell Name=""D""></Cell>
	<Cell Name=""E""></Cell>
  </Cells>
  <Variables>
    <Variable Id=""41"">
      <Name>Wnt-ext</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>1</Number>
	  <Tags>
		<Tag Id=""1"" Name=""B""></Tag>
		<Tag Id=""2"" Name=""C""></Tag>
		<Tag Id=""3"" Name=""D""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""42"">
      <Name>Wnt-ext</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>1</Number>
	  <Tags>
		<Tag Id=""1"" Name=""C""></Tag>
		<Tag Id=""2"" Name=""D""></Tag>
		<Tag Id=""3"" Name=""E""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""43"">
      <Name>Ligand-in</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>2</Number>
	  <Tags>
		<Tag Id=""1"" Name=""C""></Tag>
		<Tag Id=""2"" Name=""D""></Tag>
		<Tag Id=""3"" Name=""E""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""44"">
      <Name>Ligand-in</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>2</Number>
	  <Tags>
		<Tag Id=""1"" Name=""B""></Tag>
		<Tag Id=""2"" Name=""C""></Tag>
		<Tag Id=""3"" Name=""D""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""45"">
      <Name>Ligand-in</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>2</Number>
	  <Tags>
		<Tag Id=""1"" Name=""C""></Tag>
		<Tag Id=""2"" Name=""D""></Tag>
		<Tag Id=""3"" Name=""E""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""72"">
      <Name>Wnt-ext</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>1</Number>
	  <Tags>
		<Tag Id=""1"" Name=""A""></Tag>
		<Tag Id=""2"" Name=""B""></Tag>
		<Tag Id=""3"" Name=""C""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""73"">
      <Name>Wnt-ext</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>(4+var(7))/2</Function>
	  <Number>1</Number>
	  <Tags>
		<!--<Tag Id=""1"" Name=""_""></Tag>-->
		<Tag Id=""2"" Name=""A""></Tag>
		<Tag Id=""3"" Name=""B""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""74"">
      <Name>Ligand-in</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>(2+var(11))/2</Function>
	  <Number>2</Number>
	  <Tags>
		<!--<Tag Id=""1"" Name=""_""></Tag>-->
		<Tag Id=""2"" Name=""A""></Tag>
		<Tag Id=""3"" Name=""B""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""75"">
      <Name>Wnt-ext</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>var(33)/2</Function>
	  <Number>1</Number>
	  <Tags>
		<Tag Id=""1"" Name=""D""></Tag>
		<Tag Id=""2"" Name=""E""></Tag>
		<!--<Tag Id=""3"" Name=""_""></Tag>-->
	  </Tags>
    </Variable>
    <Variable Id=""76"">
      <Name>Ligand-in</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>(2+var(39))/2</Function>
	  <Number>2</Number>
	  <Tags>
		<Tag Id=""1"" Name=""D""></Tag>
		<Tag Id=""2"" Name=""E""></Tag>
		<!--<Tag Id=""3"" Name=""_""></Tag>-->
	  </Tags>
    </Variable>
    <Variable Id=""1"">
      <Name>DSH</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>3</Number>
	  <Tags>
		<Tag Id=""1"" Name=""B""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""2"">
      <Name>Axin</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>4</Number>
	  <Tags>
		<Tag Id=""1"" Name=""B""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""3"">
      <Name>B-Cat</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>5</Number>
	  <Tags>
		<Tag Id=""1"" Name=""B""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""4"">
      <Name>GT1</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>6</Number>
	  <Tags>
		<Tag Id=""1"" Name=""B""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""5"">
      <Name>Notch-IC</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>min(var(12), var(45))</Function>
	  <Number>7</Number>
	  <Tags>
		<Tag Id=""1"" Name=""B""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""6"">
      <Name>P21</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>8</Number>
	  <Tags>
		<Tag Id=""1"" Name=""B""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""7"">
      <Name>Wnt</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>9</Number>
	  <Tags>
		<Tag Id=""1"" Name=""B""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""8"">
      <Name>Cask1a</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>4</Function>
	  <Number>10</Number>
	  <Tags>
		<Tag Id=""1"" Name=""B""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""9"">
      <Name>BCat exp</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>4</Function>
	  <Number>11</Number>
	  <Tags>
		<Tag Id=""1"" Name=""B""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""10"">
      <Name>Frizzled</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>12</Number>
	  <Tags>
		<Tag Id=""1"" Name=""B""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""11"">
      <Name>Jagged</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>13</Number>
	  <Tags>
		<Tag Id=""1"" Name=""B""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""12"">
      <Name>Notch</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>14</Number>
	  <Tags>
		<Tag Id=""1"" Name=""B""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""13"">
      <Name>GT2</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>15</Number>
	  <Tags>
		<Tag Id=""1"" Name=""B""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""14"">
      <Name>DSH</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>3</Number>
	  <Tags>
		<Tag Id=""1"" Name=""C""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""15"">
      <Name>Axin</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>4</Number>
	  <Tags>
		<Tag Id=""1"" Name=""C""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""16"">
      <Name>B-Cat</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>5</Number>
	  <Tags>
		<Tag Id=""1"" Name=""C""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""17"">
      <Name>GT1</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>6</Number>
	  <Tags>
		<Tag Id=""1"" Name=""C""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""18"">
      <Name>Notch-IC</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>min(var(26), var(44))</Function>
	  <Number>7</Number>
	  <Tags>
		<Tag Id=""1"" Name=""C""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""19"">
      <Name>P21</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>8</Number>
	  <Tags>
		<Tag Id=""1"" Name=""C""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""20"">
      <Name>Wnt</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>9</Number>
	  <Tags>
		<Tag Id=""1"" Name=""C""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""21"">
      <Name>GT2</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>15</Number>
	  <Tags>
		<Tag Id=""1"" Name=""C""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""22"">
      <Name>Cask1a</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>4</Function>
	  <Number>10</Number>
	  <Tags>
		<Tag Id=""1"" Name=""C""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""23"">
      <Name>bCATexp</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>4</Function>
	  <Number>11</Number>
	  <Tags>
		<Tag Id=""1"" Name=""C""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""24"">
      <Name>Frizzled</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>12</Number>
	  <Tags>
		<Tag Id=""1"" Name=""C""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""25"">
      <Name>Jagged</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>13</Number>
	  <Tags>
		<Tag Id=""1"" Name=""C""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""26"">
      <Name>Notch</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>14</Number>
	  <Tags>
		<Tag Id=""1"" Name=""C""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""27"">
      <Name>DSH</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>3</Number>
	  <Tags>
		<Tag Id=""1"" Name=""D""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""28"">
      <Name>Axin</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>4</Number>
	  <Tags>
		<Tag Id=""1"" Name=""D""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""29"">
      <Name>B-Cat</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>5</Number>
	  <Tags>
		<Tag Id=""1"" Name=""D""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""30"">
      <Name>GT1</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>6</Number>
	  <Tags>
		<Tag Id=""1"" Name=""D""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""31"">
      <Name>Notch-IC</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>min(var(38), var(43))</Function>
	  <Number>7</Number>
	  <Tags>
		<Tag Id=""1"" Name=""D""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""32"">
      <Name>P21</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>8</Number>
	  <Tags>
		<Tag Id=""1"" Name=""D""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""33"">
      <Name>Wnt</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>9</Number>
	  <Tags>
		<Tag Id=""1"" Name=""D""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""34"">
      <Name>GT2</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>15</Number>
	  <Tags>
		<Tag Id=""1"" Name=""D""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""35"">
      <Name>Cask1a</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>4</Function>
	  <Number>10</Number>
	  <Tags>
		<Tag Id=""1"" Name=""D""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""36"">
      <Name>BCat exp</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>4</Function>
	  <Number>11</Number>
	  <Tags>
		<Tag Id=""1"" Name=""D""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""37"">
      <Name>Frizzled</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>12</Number>
	  <Tags>
		<Tag Id=""1"" Name=""D""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""38"">
      <Name>Notch</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>14</Number>
	  <Tags>
		<Tag Id=""1"" Name=""D""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""39"">
      <Name>Jagged</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>13</Number>
	  <Tags>
		<Tag Id=""1"" Name=""D""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""77"">
      <Name>DSH</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>3</Number>
	  <Tags>
		<Tag Id=""1"" Name=""A""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""78"">
      <Name>Axin</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>4</Number>
	  <Tags>
		<Tag Id=""1"" Name=""A""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""79"">
      <Name>B-Cat</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>5</Number>
	  <Tags>
		<Tag Id=""1"" Name=""A""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""80"">
      <Name>GT1</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>6</Number>
	  <Tags>
		<Tag Id=""1"" Name=""A""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""81"">
      <Name>Notch-IC</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>min(var(88), var(74))</Function>
	  <Number>7</Number>
	  <Tags>
		<Tag Id=""1"" Name=""A""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""82"">
      <Name>P21</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>8</Number>
	  <Tags>
		<Tag Id=""1"" Name=""A""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""83"">
      <Name>Wnt</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>9</Number>
	  <Tags>
		<Tag Id=""1"" Name=""A""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""84"">
      <Name>Cask1a</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>4</Function>
	  <Number>10</Number>
	  <Tags>
		<Tag Id=""1"" Name=""A""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""85"">
      <Name>BCat exp</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>4</Function>
	  <Number>11</Number>
	  <Tags>
		<Tag Id=""1"" Name=""A""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""86"">
      <Name>Frizzled</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>12</Number>
	  <Tags>
		<Tag Id=""1"" Name=""A""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""87"">
      <Name>Jagged</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>13</Number>
	  <Tags>
		<Tag Id=""1"" Name=""A""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""88"">
      <Name>Notch</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>14</Number>
	  <Tags>
		<Tag Id=""1"" Name=""A""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""89"">
      <Name>GT2</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>15</Number>
	  <Tags>
		<Tag Id=""1"" Name=""A""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""90"">
      <Name>DSH</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>3</Number>
	  <Tags>
		<Tag Id=""1"" Name=""E""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""91"">
      <Name>Axin</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>4</Number>
	  <Tags>
		<Tag Id=""1"" Name=""E""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""92"">
      <Name>B-Cat</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>5</Number>
	  <Tags>
		<Tag Id=""1"" Name=""E""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""93"">
      <Name>GT1</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>6</Number>
	  <Tags>
		<Tag Id=""1"" Name=""E""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""94"">
      <Name>Notch-IC</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>min(var(101), var(76))</Function>
	  <Number>7</Number>
	  <Tags>
		<Tag Id=""1"" Name=""E""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""95"">
      <Name>P21</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>8</Number>
	  <Tags>
		<Tag Id=""1"" Name=""E""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""96"">
      <Name>Wnt</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>9</Number>
	  <Tags>
		<Tag Id=""1"" Name=""E""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""97"">
      <Name>GT2</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>15</Number>
	  <Tags>
		<Tag Id=""1"" Name=""E""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""98"">
      <Name>Cask1a</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>4</Function>
	  <Number>10</Number>
	  <Tags>
		<Tag Id=""1"" Name=""E""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""99"">
      <Name>BCat exp</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function>4</Function>
	  <Number>11</Number>
	  <Tags>
		<Tag Id=""1"" Name=""E""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""100"">
      <Name>Frizzled</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>12</Number>
	  <Tags>
		<Tag Id=""1"" Name=""E""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""101"">
      <Name>Notch</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>14</Number>
	  <Tags>
		<Tag Id=""1"" Name=""E""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""102"">
      <Name>Jagged</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>4</RangeTo>
      <Function></Function>
	  <Number>13</Number>
	  <Tags>
		<Tag Id=""1"" Name=""E""></Tag>
	  </Tags>
    </Variable>
  </Variables>
  <Relationships>
    <Relationship Id=""37"">
      <FromVariableId>7</FromVariableId>
      <ToVariableId>41</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""38"">
      <FromVariableId>20</FromVariableId>
      <ToVariableId>42</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""39"">
      <FromVariableId>33</FromVariableId>
      <ToVariableId>41</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""104"">
      <FromVariableId>96</FromVariableId>
      <ToVariableId>42</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""107"">
      <FromVariableId>74</FromVariableId>
      <ToVariableId>81</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""108"">
      <FromVariableId>45</FromVariableId>
      <ToVariableId>5</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""109"">
      <FromVariableId>44</FromVariableId>
      <ToVariableId>18</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""110"">
      <FromVariableId>43</FromVariableId>
      <ToVariableId>31</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""111"">
      <FromVariableId>76</FromVariableId>
      <ToVariableId>94</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""1"">
      <FromVariableId>1</FromVariableId>
      <ToVariableId>2</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""2"">
      <FromVariableId>2</FromVariableId>
      <ToVariableId>3</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""3"">
      <FromVariableId>3</FromVariableId>
      <ToVariableId>4</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""4"">
      <FromVariableId>5</FromVariableId>
      <ToVariableId>6</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""5"">
      <FromVariableId>6</FromVariableId>
      <ToVariableId>7</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""6"">
      <FromVariableId>8</FromVariableId>
      <ToVariableId>2</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""7"">
      <FromVariableId>9</FromVariableId>
      <ToVariableId>3</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""10"">
      <FromVariableId>5</FromVariableId>
      <ToVariableId>13</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""11"">
      <FromVariableId>14</FromVariableId>
      <ToVariableId>15</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""12"">
      <FromVariableId>15</FromVariableId>
      <ToVariableId>16</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""13"">
      <FromVariableId>16</FromVariableId>
      <ToVariableId>17</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""14"">
      <FromVariableId>18</FromVariableId>
      <ToVariableId>19</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""15"">
      <FromVariableId>19</FromVariableId>
      <ToVariableId>20</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""16"">
      <FromVariableId>18</FromVariableId>
      <ToVariableId>21</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""17"">
      <FromVariableId>22</FromVariableId>
      <ToVariableId>15</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""18"">
      <FromVariableId>23</FromVariableId>
      <ToVariableId>16</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""22"">
      <FromVariableId>27</FromVariableId>
      <ToVariableId>28</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""23"">
      <FromVariableId>28</FromVariableId>
      <ToVariableId>29</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""24"">
      <FromVariableId>29</FromVariableId>
      <ToVariableId>30</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""25"">
      <FromVariableId>31</FromVariableId>
      <ToVariableId>32</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""26"">
      <FromVariableId>32</FromVariableId>
      <ToVariableId>33</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""27"">
      <FromVariableId>31</FromVariableId>
      <ToVariableId>34</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""28"">
      <FromVariableId>35</FromVariableId>
      <ToVariableId>28</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""29"">
      <FromVariableId>36</FromVariableId>
      <ToVariableId>29</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""58"">
      <FromVariableId>77</FromVariableId>
      <ToVariableId>78</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""59"">
      <FromVariableId>78</FromVariableId>
      <ToVariableId>79</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""60"">
      <FromVariableId>79</FromVariableId>
      <ToVariableId>80</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""61"">
      <FromVariableId>81</FromVariableId>
      <ToVariableId>82</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""62"">
      <FromVariableId>82</FromVariableId>
      <ToVariableId>83</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""63"">
      <FromVariableId>84</FromVariableId>
      <ToVariableId>78</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""64"">
      <FromVariableId>85</FromVariableId>
      <ToVariableId>79</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""67"">
      <FromVariableId>81</FromVariableId>
      <ToVariableId>89</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""90"">
      <FromVariableId>90</FromVariableId>
      <ToVariableId>91</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""91"">
      <FromVariableId>91</FromVariableId>
      <ToVariableId>92</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""92"">
      <FromVariableId>92</FromVariableId>
      <ToVariableId>93</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""93"">
      <FromVariableId>94</FromVariableId>
      <ToVariableId>95</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""94"">
      <FromVariableId>95</FromVariableId>
      <ToVariableId>96</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""95"">
      <FromVariableId>94</FromVariableId>
      <ToVariableId>97</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""96"">
      <FromVariableId>98</FromVariableId>
      <ToVariableId>91</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""97"">
      <FromVariableId>99</FromVariableId>
      <ToVariableId>92</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""112"">
      <FromVariableId>33</FromVariableId>
      <ToVariableId>75</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""113"">
      <FromVariableId>83</FromVariableId>
      <ToVariableId>72</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""114"">
      <FromVariableId>7</FromVariableId>
      <ToVariableId>73</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""115"">
      <FromVariableId>20</FromVariableId>
      <ToVariableId>72</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""68"">
      <FromVariableId>72</FromVariableId>
      <ToVariableId>10</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""8"">
      <FromVariableId>10</FromVariableId>
      <ToVariableId>1</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""42"">
      <FromVariableId>11</FromVariableId>
      <ToVariableId>44</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""78"">
      <FromVariableId>11</FromVariableId>
      <ToVariableId>74</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""9"">
      <FromVariableId>13</FromVariableId>
      <ToVariableId>11</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""41"">
      <FromVariableId>45</FromVariableId>
      <ToVariableId>12</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""87"">
      <FromVariableId>12</FromVariableId>
      <ToVariableId>5</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""34"">
      <FromVariableId>41</FromVariableId>
      <ToVariableId>24</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""19"">
      <FromVariableId>24</FromVariableId>
      <ToVariableId>14</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""43"">
      <FromVariableId>25</FromVariableId>
      <ToVariableId>45</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""44"">
      <FromVariableId>25</FromVariableId>
      <ToVariableId>43</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""20"">
      <FromVariableId>21</FromVariableId>
      <ToVariableId>25</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""46"">
      <FromVariableId>44</FromVariableId>
      <ToVariableId>26</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""21"">
      <FromVariableId>26</FromVariableId>
      <ToVariableId>18</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""88"">
      <FromVariableId>26</FromVariableId>
      <ToVariableId>18</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""35"">
      <FromVariableId>42</FromVariableId>
      <ToVariableId>37</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""30"">
      <FromVariableId>37</FromVariableId>
      <ToVariableId>27</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""40"">
      <FromVariableId>43</FromVariableId>
      <ToVariableId>38</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""31"">
      <FromVariableId>38</FromVariableId>
      <ToVariableId>31</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""89"">
      <FromVariableId>38</FromVariableId>
      <ToVariableId>31</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""45"">
      <FromVariableId>39</FromVariableId>
      <ToVariableId>44</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""81"">
      <FromVariableId>39</FromVariableId>
      <ToVariableId>76</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""32"">
      <FromVariableId>34</FromVariableId>
      <ToVariableId>39</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""71"">
      <FromVariableId>73</FromVariableId>
      <ToVariableId>86</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""65"">
      <FromVariableId>86</FromVariableId>
      <ToVariableId>77</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""77"">
      <FromVariableId>87</FromVariableId>
      <ToVariableId>45</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""66"">
      <FromVariableId>89</FromVariableId>
      <ToVariableId>87</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""76"">
      <FromVariableId>74</FromVariableId>
      <ToVariableId>88</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""82"">
      <FromVariableId>88</FromVariableId>
      <ToVariableId>81</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""103"">
      <FromVariableId>75</FromVariableId>
      <ToVariableId>100</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""98"">
      <FromVariableId>100</FromVariableId>
      <ToVariableId>90</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""105"">
      <FromVariableId>76</FromVariableId>
      <ToVariableId>101</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""99"">
      <FromVariableId>101</FromVariableId>
      <ToVariableId>94</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""102"">
      <FromVariableId>101</FromVariableId>
      <ToVariableId>94</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""106"">
      <FromVariableId>102</FromVariableId>
      <ToVariableId>43</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""100"">
      <FromVariableId>97</FromVariableId>
      <ToVariableId>102</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
"

            });
            this.theXMLlist.Add(new XMLObject
            {
                XMLmaxRelationshipN = 131,
                XMLfilename = "Hair follicle development",
                XMLfile = @"<AnalysisInput ModelName=""Hair follicle development"">
  <Engine>
    <Name>SYN</Name>
  </Engine>
  <Cells>
	<Cell Name=""EA""></Cell>
	<Cell Name=""EB""></Cell>
	<Cell Name=""EC""></Cell>
	<Cell Name=""MA""></Cell>
	<Cell Name=""MB""></Cell>
	<Cell Name=""MC""></Cell>
  </Cells>
  <Variables>
    <Variable Id=""10"">
      <Name>WNT_A</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>1</Number>
	  <Tags>
		<Tag Id=""3"" Name=""EA""></Tag>
		<Tag Id=""4"" Name=""MA""></Tag>
		<Tag Id=""5"" Name=""EB""></Tag>
		<Tag Id=""6"" Name=""MB""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""13"">
      <Name>Noggin_A</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>3</Number>
	  <Tags>
		<Tag Id=""3"" Name=""EA""></Tag>
		<Tag Id=""4"" Name=""MA""></Tag>
		<Tag Id=""5"" Name=""EB""></Tag>
		<Tag Id=""6"" Name=""MB""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""45"">
      <Name>WNT_B</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>1</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EA""></Tag>
		<Tag Id=""2"" Name=""MA""></Tag>
		<Tag Id=""3"" Name=""EB""></Tag>
		<Tag Id=""4"" Name=""MB""></Tag>
		<Tag Id=""5"" Name=""EC""></Tag>
		<Tag Id=""6"" Name=""MC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""46"">
      <Name>Noggin_B</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>3</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EA""></Tag>
		<Tag Id=""2"" Name=""MA""></Tag>
		<Tag Id=""3"" Name=""EB""></Tag>
		<Tag Id=""4"" Name=""MB""></Tag>
		<Tag Id=""5"" Name=""EC""></Tag>
		<Tag Id=""6"" Name=""MC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""48"">
      <Name>WNT_C</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>1</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EB""></Tag>
		<Tag Id=""2"" Name=""MB""></Tag>
		<Tag Id=""3"" Name=""EC""></Tag>
		<Tag Id=""4"" Name=""MC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""49"">
      <Name>Noggin_C</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>3</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EB""></Tag>
		<Tag Id=""2"" Name=""MB""></Tag>
		<Tag Id=""3"" Name=""EC""></Tag>
		<Tag Id=""4"" Name=""MC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""62"">
      <Name>EDA_A</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>4</Number>
	  <Tags>
		<Tag Id=""3"" Name=""EA""></Tag>
		<Tag Id=""4"" Name=""MA""></Tag>
		<Tag Id=""5"" Name=""EB""></Tag>
		<Tag Id=""6"" Name=""MB""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""63"">
      <Name>EDA_B</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>4</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EA""></Tag>
		<Tag Id=""2"" Name=""MA""></Tag>
		<Tag Id=""3"" Name=""EB""></Tag>
		<Tag Id=""4"" Name=""MB""></Tag>
		<Tag Id=""5"" Name=""EC""></Tag>
		<Tag Id=""6"" Name=""MC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""64"">
      <Name>EDA_C</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>4</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EB""></Tag>
		<Tag Id=""2"" Name=""MB""></Tag>
		<Tag Id=""3"" Name=""EC""></Tag>
		<Tag Id=""4"" Name=""MC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""65"">
      <Name>BMP_C</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>2</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EB""></Tag>
		<Tag Id=""2"" Name=""MB""></Tag>
		<Tag Id=""3"" Name=""EC""></Tag>
		<Tag Id=""4"" Name=""MC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""66"">
      <Name>BMP_B</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>2</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EA""></Tag>
		<Tag Id=""2"" Name=""MA""></Tag>
		<Tag Id=""3"" Name=""EB""></Tag>
		<Tag Id=""4"" Name=""MB""></Tag>
		<Tag Id=""5"" Name=""EC""></Tag>
		<Tag Id=""6"" Name=""MC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""67"">
      <Name>BMP_A</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>2</Number>
	  <Tags>
		<Tag Id=""3"" Name=""EA""></Tag>
		<Tag Id=""4"" Name=""MA""></Tag>
		<Tag Id=""5"" Name=""EB""></Tag>
		<Tag Id=""6"" Name=""MB""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""1"">
      <Name>EctodermA.FZD</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>5</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EA""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""2"">
      <Name>EctodermA.DVL</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>6</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EA""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""3"">
      <Name>EctodermA.APC_axin_GSK3B</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>7</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EA""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""4"">
      <Name>EctodermA.B-catenin</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>8</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EA""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""6"">
      <Name>EctodermA.EctoExpression</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function>1</Function>
	  <Number>9</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EA""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""7"">
      <Name>EctodermA.BMPR</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>10</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EA""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""8"">
      <Name>EctodermA.LEF1</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>11</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EA""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""9"">
      <Name>EctodermA.ECadherin</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>12</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EA""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""11"">
      <Name>EctodermA.PlacodeFate</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>13</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EA""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""54"">
      <Name>EctodermA.PCadherin</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>14</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EA""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""58"">
      <Name>EctodermA.EDAR</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>15</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EA""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""14"">
      <Name>MesochymeA.MesoExpression</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function>1</Function>
	  <Number>16</Number>
	  <Tags>
		<Tag Id=""1"" Name=""MA""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""15"">
      <Name>EctodermB.FZD</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>5</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EB""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""16"">
      <Name>EctodermB.DVL</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>6</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EB""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""17"">
      <Name>EctodermB.APC_axin_GSK3B</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>7</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EB""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""18"">
      <Name>EctodermB.B-catenin</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>8</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EB""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""19"">
      <Name>EctodermB.EctoExpression</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function>1</Function>
	  <Number>9</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EB""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""20"">
      <Name>EctodermB.BMPR</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>10</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EB""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""21"">
      <Name>EctodermB.LEF1</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>11</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EB""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""22"">
      <Name>EctodermB.ECadherin</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>12</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EB""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""23"">
      <Name>EctodermB.PlacodeFate</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>13</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EB""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""55"">
      <Name>EctodermB.PCadherin</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>14</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EB""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""59"">
      <Name>EctodermB.EDAR</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>15</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EB""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""24"">
      <Name>EctodermC.FZD</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>5</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""25"">
      <Name>EctodermC.DVL</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>6</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""26"">
      <Name>EctodermC.APC_axin_GSK3B</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>7</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""27"">
      <Name>EctodermC.B-catenin</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>8</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""28"">
      <Name>EctodermC.EctoExpression</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function>1</Function>
	  <Number>9</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""29"">
      <Name>EctodermC.BMPR</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>10</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""30"">
      <Name>EctodermC.LEF1</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>11</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""31"">
      <Name>EctodermC.ECadherin</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>12</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""32"">
      <Name>EctodermC.PlacodeFate</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>13</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""56"">
      <Name>EctodermC.PCadherin</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>14</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""60"">
      <Name>EctodermC.EDAR</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function></Function>
	  <Number>15</Number>
	  <Tags>
		<Tag Id=""1"" Name=""EC""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""42"">
      <Name>MesochymeB.MesoExpression</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function>1</Function>
	  <Number>16</Number>
	  <Tags>
		<Tag Id=""1"" Name=""MB""></Tag>
	  </Tags>
    </Variable>
    <Variable Id=""43"">
      <Name>MesochymeC.MesoExpression</Name>
      <RangeFrom>0</RangeFrom>
      <RangeTo>1</RangeTo>
      <Function>1</Function>
	  <Number>16</Number>
	  <Tags>
		<Tag Id=""1"" Name=""MC""></Tag>
	  </Tags>
    </Variable>
  </Variables>
  <Relationships>
    <Relationship Id=""2"">
      <FromVariableId>2</FromVariableId>
      <ToVariableId>3</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""3"">
      <FromVariableId>3</FromVariableId>
      <ToVariableId>4</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""4"">
      <FromVariableId>6</FromVariableId>
      <ToVariableId>3</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""5"">
      <FromVariableId>6</FromVariableId>
      <ToVariableId>4</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""10"">
      <FromVariableId>4</FromVariableId>
      <ToVariableId>9</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""11"">
      <FromVariableId>8</FromVariableId>
      <ToVariableId>9</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""12"">
      <FromVariableId>6</FromVariableId>
      <ToVariableId>9</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""13"">
      <FromVariableId>8</FromVariableId>
      <ToVariableId>11</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""14"">
      <FromVariableId>4</FromVariableId>
      <ToVariableId>11</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""15"">
      <FromVariableId>9</FromVariableId>
      <ToVariableId>11</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""20"">
      <FromVariableId>14</FromVariableId>
      <ToVariableId>13</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""23"">
      <FromVariableId>16</FromVariableId>
      <ToVariableId>17</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""24"">
      <FromVariableId>17</FromVariableId>
      <ToVariableId>18</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""25"">
      <FromVariableId>19</FromVariableId>
      <ToVariableId>17</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""26"">
      <FromVariableId>19</FromVariableId>
      <ToVariableId>18</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""27"">
      <FromVariableId>18</FromVariableId>
      <ToVariableId>22</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""28"">
      <FromVariableId>21</FromVariableId>
      <ToVariableId>22</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""29"">
      <FromVariableId>19</FromVariableId>
      <ToVariableId>22</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""30"">
      <FromVariableId>21</FromVariableId>
      <ToVariableId>23</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""31"">
      <FromVariableId>18</FromVariableId>
      <ToVariableId>23</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""32"">
      <FromVariableId>22</FromVariableId>
      <ToVariableId>23</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""23"">
      <FromVariableId>25</FromVariableId>
      <ToVariableId>26</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""24"">
      <FromVariableId>26</FromVariableId>
      <ToVariableId>27</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""25"">
      <FromVariableId>28</FromVariableId>
      <ToVariableId>26</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""26"">
      <FromVariableId>28</FromVariableId>
      <ToVariableId>27</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""27"">
      <FromVariableId>27</FromVariableId>
      <ToVariableId>31</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""28"">
      <FromVariableId>30</FromVariableId>
      <ToVariableId>31</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""29"">
      <FromVariableId>28</FromVariableId>
      <ToVariableId>31</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""30"">
      <FromVariableId>30</FromVariableId>
      <ToVariableId>32</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""31"">
      <FromVariableId>27</FromVariableId>
      <ToVariableId>32</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""32"">
      <FromVariableId>31</FromVariableId>
      <ToVariableId>32</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""34"">
      <FromVariableId>42</FromVariableId>
      <ToVariableId>46</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""35"">
      <FromVariableId>43</FromVariableId>
      <ToVariableId>49</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""37"">
      <FromVariableId>42</FromVariableId>
      <ToVariableId>13</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""38"">
      <FromVariableId>43</FromVariableId>
      <ToVariableId>46</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""56"">
      <FromVariableId>6</FromVariableId>
      <ToVariableId>8</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""57"">
      <FromVariableId>23</FromVariableId>
      <ToVariableId>55</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""58"">
      <FromVariableId>11</FromVariableId>
      <ToVariableId>54</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""59"">
      <FromVariableId>32</FromVariableId>
      <ToVariableId>56</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""61"">
      <FromVariableId>19</FromVariableId>
      <ToVariableId>21</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""62"">
      <FromVariableId>28</FromVariableId>
      <ToVariableId>30</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""64"">
      <FromVariableId>14</FromVariableId>
      <ToVariableId>62</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""67"">
      <FromVariableId>42</FromVariableId>
      <ToVariableId>63</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""70"">
      <FromVariableId>43</FromVariableId>
      <ToVariableId>64</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""81"">
      <FromVariableId>6</FromVariableId>
      <ToVariableId>10</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""82"">
      <FromVariableId>19</FromVariableId>
      <ToVariableId>45</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""83"">
      <FromVariableId>28</FromVariableId>
      <ToVariableId>48</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""113"">
      <FromVariableId>14</FromVariableId>
      <ToVariableId>46</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""114"">
      <FromVariableId>42</FromVariableId>
      <ToVariableId>49</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""121"">
      <FromVariableId>49</FromVariableId>
      <ToVariableId>65</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""122"">
      <FromVariableId>13</FromVariableId>
      <ToVariableId>67</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""127"">
      <FromVariableId>46</FromVariableId>
      <ToVariableId>66</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""128"">
      <FromVariableId>67</FromVariableId>
      <ToVariableId>66</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""129"">
      <FromVariableId>66</FromVariableId>
      <ToVariableId>65</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""130"">
      <FromVariableId>65</FromVariableId>
      <ToVariableId>66</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""131"">
      <FromVariableId>66</FromVariableId>
      <ToVariableId>67</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""1"">
      <FromVariableId>1</FromVariableId>
      <ToVariableId>2</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""9"">
      <FromVariableId>10</FromVariableId>
      <ToVariableId>1</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""18"">
      <FromVariableId>7</FromVariableId>
      <ToVariableId>8</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""124"">
      <FromVariableId>67</FromVariableId>
      <ToVariableId>7</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""65"">
      <FromVariableId>62</FromVariableId>
      <ToVariableId>58</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""123"">
      <FromVariableId>58</FromVariableId>
      <ToVariableId>67</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""22"">
      <FromVariableId>15</FromVariableId>
      <ToVariableId>16</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""42"">
      <FromVariableId>45</FromVariableId>
      <ToVariableId>15</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""33"">
      <FromVariableId>20</FromVariableId>
      <ToVariableId>21</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""126"">
      <FromVariableId>66</FromVariableId>
      <ToVariableId>20</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""68"">
      <FromVariableId>63</FromVariableId>
      <ToVariableId>59</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""125"">
      <FromVariableId>59</FromVariableId>
      <ToVariableId>66</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""22"">
      <FromVariableId>24</FromVariableId>
      <ToVariableId>25</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""45"">
      <FromVariableId>48</FromVariableId>
      <ToVariableId>24</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""33"">
      <FromVariableId>29</FromVariableId>
      <ToVariableId>30</ToVariableId>
      <Type>Inhibitor</Type>
    </Relationship>
    <Relationship Id=""120"">
      <FromVariableId>65</FromVariableId>
      <ToVariableId>29</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""71"">
      <FromVariableId>64</FromVariableId>
      <ToVariableId>60</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
    <Relationship Id=""119"">
      <FromVariableId>60</FromVariableId>
      <ToVariableId>65</ToVariableId>
      <Type>Activator</Type>
    </Relationship>
"
            });
        }
    }
}
