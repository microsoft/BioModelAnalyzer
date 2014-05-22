using System;
using System.Collections.Generic;       // lists.
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using BioCheck.ViewModel.Time;
using MvvmFx.Common.ViewModels.States;

using System.Diagnostics;
using System.Windows.Controls.Primitives;
using System.ComponentModel;
using BioCheck.Controls;
using BioCheck.Services;
using BioCheck.ViewModel;
using MvvmFx.Common.Helpers;
using Microsoft.Practices.Unity;

using System.Collections.ObjectModel;           // For ObservableCollection

// Time edit
namespace BioCheck.Views
{
    // Public TimeView classes used in TimeView
    #region Public TimeView classes used in TimeView
    public class KeyFrames
    {
        // Keyframe features
        public string Name { get; set; }            // name (set by direct typing in the box)
        public string Rule { get; set; }
        public Color Color { get; set; }
        public string[] Content { 
            get; 
            set;
        }       // "var", "cell", "N", "<", "=" or ">". Feeds into the Rule OR allows retrieval of full name from NameContent
        public string[] NameContent { get; set; }   // Stores actual cell and variable names, and ints? Feeds into the Rule
    }


    public class FormulaType
    {
        // Formula grid content
        public string KeyframeName { get; set; }      // null or the keyframe's name
        public string Rule { get; set; }              // The logical expression, or the keyframe's rule
        public Color KeyframeColor { get; set; }      // The color of the keyframe rectangle, or unset (Color can't be null)
        public string Content { get; set; }           // "logics", "keyframe" or null
    }

    public class NonNull_Formula
    {
        // Post-processed Formula content
        public string Content { get; set; }           // "logics", "keyframe" or null
        public string Rule { get; set; }              // The logical expression, or the keyframe's rule
                                                      // "Until", "If", "Eventually", "Always", "cb", "ob" et.c.
    }

    public class BMAvars
    {
        public string Name { get; set; }       // name (set by popup)
    }

    public class BMAcells
    {
        public string Name { get; set; }       // name (set by popup)
    }
    #endregion

    public partial class TimeView : UserControl
    {
        private struct VisualStates
        {
            public const string TimeStateGroup = "TimeStateGroup";            
        }

        private TimeViewModel timeVM;
        public ObservableCollection<KeyFrames> allKeyframes;            // Now it's accessible throughout.
        public ObservableCollection<FormulaType> allFormulaElements;
        public List<NonNull_Formula> noNullElements = new List<NonNull_Formula>();     // Create list for parsed Formula consiting of non-null Formula elements only.
        public ObservableCollection<BMAvars> allVariables;
        public ObservableCollection<BMAcells> allCells;

        public string textForTempDebug = "";

        // Initialization at startup
        #region Initialization at startup
        public TimeView()
        {
            InitializeComponent();

            this.DataContextChanged += TimeView_DataContextChanged;
            this.AllowDrop = true;          // Still have not tested this.
            Debug.WriteLine("Opening TimeView window.");
                        
            // ObservableCollection makes it visible.
            allKeyframes = new ObservableCollection<KeyFrames>();

            allKeyframes.Add(new KeyFrames { Name = "Initial", Content = new string[35] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }, NameContent = new string[35] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }, Rule = null, Color = Colors.Green });
            //allKeyframes.Add(new KeyFrames { Name = "Cell2", Content = new string[35] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }, NameContent = new string[35] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }, Color = Colors.Blue });
            //allKeyframes.Add(new KeyFrames { Name = "CellDeath", Content = new string[35] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }, NameContent = new string[35] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }, Color = Colors.Red });
            

            this.KeyFrames.ItemsSource = allKeyframes;         // KeyFrames is the XAML x:Name for the top listbox
            this.KeyFrames.SelectedItem = allKeyframes[0];     // Works!
            textChange_by_Keyframe = false;                    // As edits are prevented at startup otherwise.


            // Add ten new Formula elements for the default viewable Formula grid
            allFormulaElements = new ObservableCollection<FormulaType>();
            for (int elementIndex = 0; elementIndex <= 10; elementIndex++)
            {
                allFormulaElements.Add(new FormulaType { KeyframeName = null, KeyframeColor = Colors.White, Rule = null, Content = null });
            }                

            // If a model is loaded, set available variables
            if (ApplicationViewModel.Instance.HasActiveModel)
            {
                addCells2ComboBox();
                addVars2ComboBox();
            }
        }
    #endregion

        // -----------------------------------------------------------------------------------------------
        //
        //      Combobox events: Populate with loaded model cell and variable names, open and close
        //
        // ------------------------------------------------------------------------------------------------
        #region Combobox events: Populate with loaded model cell and variable names, open and close

        // Populate a dropdown menu for Cells
        private void addCells2ComboBox()
        {
            var modelVM = ApplicationViewModel.Instance.ActiveModel;

            allCells = new ObservableCollection<BMAcells>();
            int Ncells = modelVM.ContainerViewModels.Count;           
            for (int i = 0; i < Ncells; i++)
            {
                allCells.Add(new BMAcells { Name = modelVM.ContainerViewModels[i].Name });
                Debug.WriteLine("New cell: " + modelVM.ContainerViewModels[i].Name);
            }

            // If more than one cell, enable the choice All
            if (allCells.Count > 1)
            {
                allCells.Add(new BMAcells { Name = "All" });
            }

            this.dropdownCell.ItemsSource = allCells;         // KeyFrames is the XAML x:Name for the top listbox
        }

        // Populate the variable name list
        private void addVars2ComboBox()
        {
            Debug.WriteLine("Populating the variable combobox.");
            var modelVM = ApplicationViewModel.Instance.ActiveModel;

            allVariables = new ObservableCollection<BMAvars>();
            List<string> allVarsByName = new List<string>();            // Non-unique entries
            int Ncells = modelVM.ContainerViewModels.Count;          

            // Add external vars first:
            int Nvars = modelVM.VariableViewModels.Count;
            for (int i = 0; i < modelVM.VariableViewModels.Count; i++)
            {
                allVariables.Add(new BMAvars { Name = modelVM.VariableViewModels[i].Name});     // May not be unique..
                allVarsByName.Add(modelVM.VariableViewModels[i].Name);
            }

            // Add cell-contained vars:
            string variableName;
            for (int i = 0; i < Ncells; i++)
            {   
                for (int j = 0; j < modelVM.ContainerViewModels[i].VariableViewModels.Count; j++)
                {
                    // If the cell name is specified, use it
                    if (modelVM.ContainerViewModels[i].Name != null && modelVM.ContainerViewModels[i].Name != "")
                    {
                        // Cellname.Varname
                        variableName = modelVM.ContainerViewModels[i].Name + "." + modelVM.ContainerViewModels[i].VariableViewModels[j].Name;
                        allVariables.Add(new BMAvars { Name = variableName });
                    }
                    else
                    {
                        //Varname
                        allVariables.Add(new BMAvars { Name =  modelVM.ContainerViewModels[i].VariableViewModels[j].Name});
                    }
                    allVarsByName.Add(modelVM.ContainerViewModels[i].VariableViewModels[j].Name);
                }
            }

            // Add unique vars that signify all such vars, whether inside/outside of a cell:
            List<string> moreThanOneVar = new List<string>();            // Non-unique entries
            List<string> uniqueVars = new List<string>();            // Non-unique entries
            
            int counter = 0;
            foreach (string someVarName in allVarsByName)
            {
                foreach (string cfVarName in allVarsByName)
                {
                    if (someVarName == cfVarName)
                    {
                        counter++;      // There should at least be one.
                    }
                }
                if (counter > 1)
                { 
                    // Multiples detected.
                    moreThanOneVar.Add(someVarName);
                }
                counter = 0;
            }

            // List with multiples only, sorted alphabetically
            moreThanOneVar.Sort();

            // If multiples detected, select unique
            if (moreThanOneVar.Count > 1) {
                for (int varIndex = 1; varIndex < moreThanOneVar.Count; varIndex++)
                {  
                    if (moreThanOneVar[varIndex] != moreThanOneVar[varIndex - 1] || varIndex == (moreThanOneVar.Count-1))
                    {   
                        uniqueVars.Add(moreThanOneVar[varIndex -1]);      // Modify a separate list
                    }
                }
            }

            foreach (string uniqueVar in uniqueVars)
            {
                allVariables.Add(new BMAvars { Name = "All " + uniqueVar });
            }

            // If more than one variable, enable the choice All
            if (allVariables.Count > 1)
            {
                allVariables.Add(new BMAvars { Name = "All" });
            }

            // Make this list the source of the cell name dropdownlist
            this.dropdownVar.ItemsSource = allVariables;
        }

        private void openComboBoxes(Point xy)
        {
            Debug.WriteLine("----- ComboBox opens by 712. Wanted at loci: " + xy.ToString() + "but x is - " + dropdownVar.Width.ToString() + "but actualwidth is " + dropdownVar.ActualWidth.ToString());
            switch (objectClicked)
            {
                case "cell":
                    this.dropdownCell.RenderTransform = new TranslateTransform { X = xy.X - dropdownCell.Width, Y = xy.Y - dropdownCell.Height * 10 };
                    this.dropdownCell.Visibility = System.Windows.Visibility.Visible;
                    break;
                case "var":
                    this.dropdownVar.RenderTransform = new TranslateTransform { X = xy.X - dropdownVar.Width, Y = xy.Y - dropdownVar.Height * 10 };
                    Debug.WriteLine("Put the Combobox at " + xy.X.ToString() + " and " + xy.Y.ToString());
                    this.dropdownVar.Visibility = System.Windows.Visibility.Visible;
                    break;
            }
        }

        bool cellDropWasOpen = false;
        bool varDropWasOpen = false;

        // Cell name
        private void dropdownCell_DropDownOpened(object sender, EventArgs e)
        {
            cellDropWasOpen = true;
        }

        private void dropdownCell_DropDownClosed(object sender, EventArgs e)
        {
            if (cellDropWasOpen)
            {
                this.dropdownCell.Visibility = System.Windows.Visibility.Collapsed;
                cellDropWasOpen = false;

                // Set the relevant textblock to cellName_choice (whether sel changed or not)       ___new.
                object selectedItem = this.dropdownCell.SelectedItem;
                cellName_choice = (selectedItem == null)
                                    ? string.Empty
                                    : ((BMAcells)selectedItem).Name.ToString();
                               
                // Update the current grid's textblock to reflect it.

                TextBlock exactTextBlock = (TextBlock)textID(nameChangeGridN);
                exactTextBlock.Text = cellName_choice;
                ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[nameChangeGridN] = cellName_choice;

            }
        }

        string cellName_choice;
        private void dropdownCell_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var src = (ComboBox)sender;
            //cellName_choice = (string)src.SelectedValue;     // But set to default value if nothing chosen actively.

            object selectedItem = this.dropdownCell.SelectedItem;
            cellName_choice = (selectedItem == null)
                                ? string.Empty
                                : ((BMAcells)selectedItem).Name.ToString();

            Debug.WriteLine(">>>>>>>> CellName DropDown!: You chose " + cellName_choice);
            // Update the current grid's textblock to reflect it.

            TextBlock exactTextBlock = (TextBlock)textID(nameChangeGridN);
            exactTextBlock.Text = cellName_choice;
            tempDebug.Text = "Changed Text from dropdownCell_SelectionChanged at line 270.";
            ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[nameChangeGridN] = cellName_choice;

            string allStorage = "";
            Debug.WriteLine("1172: Updated var/cellnames in storage by combobox selection change: ");
            foreach (string oneGridsContent in ((KeyFrames)this.KeyFrames.SelectedItem).NameContent)
            {
                allStorage = allStorage + ", " + oneGridsContent;
            }
            Debug.WriteLine(allStorage);
            //Keyframe_Storage_checker(); 
        }

        // Variable
        private void dropdownVar_DropDownOpened(object sender, EventArgs e)
        {
            varDropWasOpen = true;
        }

        private void dropdownVar_DropDownClosed(object sender, EventArgs e)
        {
            if (varDropWasOpen)
            {
                this.dropdownVar.Visibility = System.Windows.Visibility.Collapsed;
                varDropWasOpen = false;

                // Set the textbox and Storage
                object selectedItem = this.dropdownVar.SelectedItem;
                string theVarName_choice = (selectedItem == null)
                                    ? string.Empty
                                    : ((BMAvars)selectedItem).Name.ToString();

                // Update the current grid's textblock to reflect it.
                //textblock = textID(nameChangeGridN);

                TextBlock exactTextBlock = (TextBlock)textID(nameChangeGridN);
                exactTextBlock.Text = theVarName_choice;
                ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[nameChangeGridN] = theVarName_choice;
            }
        }

        // This is auto-called at startup, unfortunately
        string varName_choice;
        private void dropdownVar_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            object selectedItem = this.dropdownVar.SelectedItem;
            varName_choice = (selectedItem == null)
                                ? string.Empty
                                : ((BMAvars)selectedItem).Name.ToString();

            Debug.WriteLine(">>>>>>>> VarName DropDown!: You chose " + varName_choice);
            // Update the current grid's textblock to reflect it.

            //textblock = textID(nameChangeGridN);

            TextBlock exactTextBlock = (TextBlock)textID(nameChangeGridN);
            exactTextBlock.Text = varName_choice;
            ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[nameChangeGridN] = varName_choice;

            string allStorage = "";
            Debug.WriteLine("1525: Updated varnames in storage by combobox selection change: ");
            foreach (string oneGridsContent in ((KeyFrames)this.KeyFrames.SelectedItem).NameContent)
            {
                allStorage = allStorage + ", " + oneGridsContent;
            }
            Debug.WriteLine(allStorage);
            Keyframe_Storage_checker(); 
        }
#endregion

        // --------------------------------------
        //
        //       Keyframe listbox events
        //
        // --------------------------------------
        #region Keyframe listbox functions

        // Reflect a user's keyframe renaming in Storage, and change keyframe
        private void Keyframe_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Get new name
            var src = (TextBox)sender;
            string newText = src.Text;
            string noWhite = newText.Trim();

            // Get where it was written, using the Tag
            //string testing_origin = src.Tag.ToString();
            string testing_origin = ((BioCheck.Views.KeyFrames)(((System.Windows.FrameworkElement)(sender)).DataContext)).Name.ToString();
            int kf_index = 0;
            
            // Change the selected keyframe to the one whose name was edited
            foreach (KeyFrames keyframe in allKeyframes)
            {
                if (keyframe.Name.ToString() == testing_origin)
                {
                    // If a new KF's textbox was clicked, 
                    // just change what is the selected KF, and update the Grid to show its grid objects
                    if (this.KeyFrames.SelectedIndex != kf_index)
                    {
                        textChange_by_Keyframe = true;
                        this.KeyFrames.SelectedItem = allKeyframes[kf_index];       // Change selection
                        // Then change the stored name to the newly given name
                        ((KeyFrames)this.KeyFrames.SelectedItem).Name = noWhite;
                        
                        // Last update this keyframe's gridview
                        KeyFrames_SelectionChangeManually();    // If I just hit the textbox (it doesn't bring me to the KF, just edits the text.)
                        textChange_by_Keyframe = false;         // Because I'm done changing the grid around.
                    }
                    else
                    {
                        // The user is changing the KF's name! Update storage:
                        // Test: deactivate KF change (I might just be re-labeling my KF.)
                        textChange_by_Keyframe = false;     // Because I'm not changing the text by clicking Another KF.

                        // Search the Formula for instances of this keyframe: change this name, too, to keep everything updated.
                        for (int formulaElementIndex = 0; formulaElementIndex < allFormulaElements.Count(); formulaElementIndex++)
                        {
                            if (allFormulaElements[formulaElementIndex].Content != null)
                            {
                                // If a keyframe
                                if (allFormulaElements[formulaElementIndex].Content == "keyframe")
                                {
                                    if (allFormulaElements[formulaElementIndex].KeyframeName == testing_origin)
                                    {
                                        // Change the name in Formula storage.
                                        allFormulaElements[formulaElementIndex].KeyframeName = noWhite;
                                        // Update the textbox name too, if in sight:

                                        // Storage directs visual appearance, so must be offset by [- N_rightScrollClicks]
                                        int vis_from_storageElement = formulaElementIndex - N_rightScrollClicks;
                                        if (vis_from_storageElement >= 1 && vis_from_storageElement <= 10)
                                        {
                                            // Add visuals: 
                                            f_textID(vis_from_storageElement).Text = noWhite;
                                            // Not perfect. If two KFs have the same start name, both will be edited..
                                        }
                                    }
                                }
                            }
                        }
                        
                        // Then change the stored KF name to the newly given name
                        ((KeyFrames)this.KeyFrames.SelectedItem).Name = noWhite;
                    }
                    //this.KeyFrames.SelectedItem = allKeyframes[kf_index];
                }
                kf_index++;
            }

            // Then change the stored name to the newly given name
            //((KeyFrames)this.KeyFrames.SelectedItem).Name = noWhite;

            // Change the tag. Doesn't this update automatically?
            tempDebug.Text = "Keyframe name changed to " + noWhite + " and changing its grid view..";
        }

        // What happens when I make any, or a different selection in the Listbox
        private void KeyFrames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // If no keyframes exist, do not proceed
            if (allKeyframes.Count > 0)
            {
                KeyFrames selectedKeyframe = ((KeyFrames)this.KeyFrames.SelectedItem);
                string currName = selectedKeyframe.Name;
                Debug.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>> KeyFrames_SelectionChanged: Selected KF " + currName);
                //((KeyFrames)this.KeyFrames.SelectedItem).Rule = "=";      // Changes the Selected item's rule?
                //Debug.WriteLine("Changed Rule to " + ((KeyFrames)this.KeyFrames.SelectedItem).Rule);

                //Debug.WriteLine("There are " + allKeyframes.Count.ToString() + " keyframes stored. Their names are: ");
                //foreach (KeyFrames keyframe in allKeyframes)
                //{
                //    Debug.WriteLine(keyframe.Name.ToString());          // I need to re-cast every time that I retrieve it! 
                //    // Not casting simply does not print.
                //}

                KeyFrames selectedKeyframes_storedGridItems = ((KeyFrames)this.KeyFrames.SelectedItem);
                string selectedKeyframes_name = (string)selectedKeyframes_storedGridItems.Name;
                Debug.WriteLine("Trying to draw out 5 spots for keyframe ");
                Debug.WriteLine(selectedKeyframes_name);
                //textForTempDebug = "\n\nStored spots for new chosen keyframe " + selectedKeyframes_name + ":\n";

                // Draw according to presence/absence of string content:
                for (int x = 0; x < 5; x++)
                {
                    selectedKeyframes_name = selectedKeyframes_storedGridItems.Content[x];
                    Debug.WriteLine(selectedKeyframes_name);

                    TextBlock exactTextBlock = (TextBlock)textID(x);
                    Path exactPath = (Path)pathID(x);
                    TextBox exactTextBox = (TextBox)textboxID(x);

                    //textForTempDebug = textForTempDebug + "\nSending " + x.ToString() + ", " + selectedKeyframes_name + " to restyleGrid_byKeyframeSelection";

                    restyleGrid_byKeyframeSelection(exactPath, exactTextBlock, x, selectedKeyframes_name, exactTextBox);
                }
                Keyframe_Storage_checker();
                //tempDebug.Text = textForTempDebug;

                // Extra path-drawing for 0, since it's failing..
                TextBlock zeroTextBlock = (TextBlock)textID(0);
                Path zeroPath = (Path)pathID(0);
                TextBox zeroTextBox = (TextBox)textboxID(0);
                restyleGrid_byKeyframeSelection(zeroPath, zeroTextBlock, 0, selectedKeyframes_storedGridItems.Content[0], zeroTextBox);

                // Ensure that drop-down boxes are invisible
                dropdownCell.Visibility = System.Windows.Visibility.Collapsed;
                dropdownVar.Visibility = System.Windows.Visibility.Collapsed;
                cellDropWasOpen = false;
                varDropWasOpen = false;
            }
            else
            {
                // Retrieve correct grid items:
                for (int x = 0; x < 5; x++)
                {
                    TextBlock exactTextBlock = (TextBlock)textID(x);
                    Path exactPath = (Path)pathID(x);
                    TextBox exactTextBox = (TextBox)textboxID(x);

                    //textForTempDebug = textForTempDebug + "\nSending " + x.ToString() + ", " + selectedKeyframes_name + " to restyleGrid_byKeyframeSelection";

                    restyleGrid_byKeyframeSelection(exactPath, exactTextBlock, x, null, exactTextBox);
                }

                //tempDebug.Text = textForTempDebug;

                // Extra path-drawing for 0, since it's failing..
                TextBlock zeroTextBlock = (TextBlock)textID(0);
                Path zeroPath = (Path)pathID(0);
                TextBox zeroTextBox = (TextBox)textboxID(0);
                restyleGrid_byKeyframeSelection(zeroPath, zeroTextBlock, 0, null, zeroTextBox);

                // Ensure that drop-down boxes are invisible
                dropdownCell.Visibility = System.Windows.Visibility.Collapsed;
                dropdownVar.Visibility = System.Windows.Visibility.Collapsed;
                cellDropWasOpen = false;
                varDropWasOpen = false;
            }
        }

        // Manually draw the selected keyframe's grid items
        // Copy of above, just a different signature to allow it to be called.
        private void KeyFrames_SelectionChangeManually()
        {
            KeyFrames selectedKeyframe = ((KeyFrames)this.KeyFrames.SelectedItem);
            string currName = selectedKeyframe.Name;
            //((KeyFrames)this.KeyFrames.SelectedItem).Rule = "=";      // Changes the Selected item's rule?
            //Debug.WriteLine("Changed Rule to " + ((KeyFrames)this.KeyFrames.SelectedItem).Rule);

            //Debug.WriteLine("There are " + allKeyframes.Count.ToString() + " keyframes stored:");
            //foreach (KeyFrames keyframe in allKeyframes)
            //{
            //    Debug.WriteLine(keyframe.Name.ToString());          // I need to re-cast every time that I retrieve it! 
            //    // Not casting simply does not print.
            //}

            KeyFrames selectedKeyframes_storedGridItems = ((KeyFrames)this.KeyFrames.SelectedItem);
            string selectedKeyframes_name = (string)selectedKeyframes_storedGridItems.Name;
            Debug.WriteLine("Drawing out 5 spots for keyframe ");
            Debug.WriteLine(selectedKeyframes_name);
            //textForTempDebug = "\n\nStored spots for new chosen keyframe " + selectedKeyframes_name + ":\n";

            // Draw according to presence/absence of string content:
            for (int x = 0; x < 5; x++)
            {
                selectedKeyframes_name = selectedKeyframes_storedGridItems.Content[x];
                //Debug.WriteLine(selectedKeyframes_name);

                TextBlock exactTextBlock = (TextBlock)textID(x);
                Path exactPath = (Path)pathID(x);
                TextBox exactTextBox = (TextBox)textboxID(x);

                //textForTempDebug = textForTempDebug + "\nSending " + x.ToString() + ", " + selectedKeyframes_name + " to restyleGrid_byKeyframeSelection";

                restyleGrid_byKeyframeSelection(exactPath, exactTextBlock, x, selectedKeyframes_name, exactTextBox);
            }
            //tempDebug.Text = textForTempDebug;

            // Extra path-drawing for 0, since it's failing..
            TextBlock zeroTextBlock = (TextBlock)textID(0);
            Path zeroPath = (Path)pathID(0);
            TextBox zeroTextBox = (TextBox)textboxID(0);
            restyleGrid_byKeyframeSelection(zeroPath, zeroTextBlock, 0, selectedKeyframes_storedGridItems.Content[0], zeroTextBox);

            // Ensure that drop-down boxes are invisible
            dropdownCell.Visibility = System.Windows.Visibility.Collapsed;
            dropdownVar.Visibility = System.Windows.Visibility.Collapsed;
            cellDropWasOpen = false;
            varDropWasOpen = false;
        }

        // Just Keyframe storage debug (write-out)
        private void Keyframe_Storage_checker()
        {
            KeyFrames selectedKeyframe = ((KeyFrames)this.KeyFrames.SelectedItem);
            string currName = selectedKeyframe.Name;

            //Debug.WriteLine("There are " + allKeyframes.Count.ToString() + " keyframes stored:");

            //foreach (KeyFrames keyframe in allKeyframes)
            //{
            //    Debug.WriteLine(keyframe.Name.ToString());          // I need to re-cast every time that I retrieve it! 
            //    // Not casting simply does not print.
            //}

            KeyFrames selectedKeyframes_storedGridItems = ((KeyFrames)this.KeyFrames.SelectedItem);
            string selectedKeyframes_name = (string)selectedKeyframes_storedGridItems.Name;
            string selectedKeyframes_namecontent;
            //Debug.WriteLine("Trying to draw out these 5 spots for keyframe ");
            //Debug.WriteLine(selectedKeyframes_name);
            //textForTempDebug = "\n\nStorage for keyframe " + selectedKeyframes_name + ":\n";

            // Draw according to presence/absence of string content:
            for (int x = 0; x < 5; x++)
            {
                selectedKeyframes_name = selectedKeyframes_storedGridItems.Content[x];
                selectedKeyframes_namecontent = selectedKeyframes_storedGridItems.NameContent[x];
                //Debug.WriteLine(selectedKeyframes_name);
                textForTempDebug = textForTempDebug + x.ToString() + "     " + selectedKeyframes_name + " : " + selectedKeyframes_namecontent + "\n";
            }
            //this.tempDebug.Text = textForTempDebug;
        }

        // Redraw the grid to reflect the stored content for this keyframe.
        private void restyleGrid_byKeyframeSelection(Path pathN, TextBlock textN, int gridInt, string whatContentSign, TextBox boxN)
        {
            textChange_by_Keyframe = true;
            string compositeResourceName = "";
            switch (whatContentSign)
            {
                case "=":
                    compositeResourceName = "path" + gridInt.ToString() + "equals";
                    textN.Text = "";
                    boxN.Text = "";
                    boxN.IsReadOnly = true;
                    boxN.IsHitTestVisible = false;
                    break;
                case "var":
                    compositeResourceName = "path" + gridInt.ToString() + "var";
                    textN.Text = ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridInt];
                    boxN.Text = "";
                    boxN.IsHitTestVisible = false;
                    boxN.IsReadOnly = true;
                    break;
                case "<":
                    compositeResourceName = "path" + gridInt.ToString() + "lt";
                    textN.Text = "";
                    boxN.Text = "";
                    boxN.IsHitTestVisible = false;
                    boxN.IsReadOnly = true;
                    break;
                case ">":
                    compositeResourceName = "path" + gridInt.ToString() + "mt";
                    textN.Text = "";
                    boxN.Text = "";
                    boxN.IsHitTestVisible = false;
                    boxN.IsReadOnly = true;
                    break;
                case "cell":
                    compositeResourceName = "path" + gridInt.ToString() + "cell";
                    textN.Text = ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridInt];
                    boxN.Text = "";
                    boxN.IsHitTestVisible = false;
                    boxN.IsReadOnly = true;
                    break;
                case "N":
                    compositeResourceName = "EmptyStyle";
                    //compositeResourceName = "path" + gridInt.ToString() + "N";
                    textN.Text = "";
                    //textN.Text = ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridInt];
                    boxN.Text = ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridInt];
                    boxN.IsHitTestVisible = true;
                    boxN.IsReadOnly = false;
                    //boxN.Focus();         _____just until I figure out what's going on.
                    break;
                case null:
                    compositeResourceName = "EmptyStyle";
                    textN.Text = "";
                    boxN.Text = "";
                    boxN.IsHitTestVisible = true;
                    boxN.IsReadOnly = false;
                    break;
            }
            pathN.Style = (Style)this.Resources[compositeResourceName];
            pathN.AllowDrop = false;
            //if (gridInt == 0 | gridInt == 1 | gridInt == 2 | gridInt == 3 | gridInt == 4)
            //{
            //    //textForTempDebug = textForTempDebug + "Grid " + gridInt.ToString() + " gets style " + compositeResourceName + " by dropping " + whatContentSign + "\n";
            //}
            //Debug.WriteLine("grid " + gridInt.ToString() + " gets style " + compositeResourceName);
            //textChange_by_Keyframe = false;
        }


        private Random rand = new Random();
        private void Add_keyframe(object sender, RoutedEventArgs e)
        {
            var item = new KeyFrames { Name = rand.Next(1000).ToString(), Content = new string[35] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }, NameContent = new string[35] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }, Rule = null };
            item.Color = Color.FromArgb(255, (byte)rand.Next(255), (byte)rand.Next(255), (byte)rand.Next(255));

            var list = (ObservableCollection<KeyFrames>)this.KeyFrames.ItemsSource;       // The current list!
            string totalList = this.KeyFrames.ItemsSource.ToString();
            // System.Collections.ObjectModel.ObservableCollection`1[SilverlightApplication1.KeyFrames]
            Debug.WriteLine(totalList);
            list.Add(item);             // Adds to the end.
            //list.Insert(2, item);     // Works.
        }

        private void Rm_keyframe(object sender, RoutedEventArgs e)
        {
            if (this.KeyFrames.SelectedIndex == 0)
            {
                this.KeyFrames.SelectedIndex = 1;
            }

            var list = (ObservableCollection<KeyFrames>)this.KeyFrames.ItemsSource;       // Return the full list.
            if (list.Count > 0)
                list.RemoveAt(0);
        }        

        // Delete a right-clicked keyframe
        private void KeyFrames_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine(">>>>>>>>>>>>>>> KeyFrames_MouseRightButtonDown <<<<<<<<<<<<<<<<<<<<<<<<<<<<<");
            e.Handled = true;   // Or Silverlight shows.

            // Find what's underneath
            //var src = (ListBox)sender;
            //var c = src.CaptureMouse();     // Necessary?
            
            // Retrieve the selected item in the Listbox
            string keyframeGoAway = ((BioCheck.Views.KeyFrames)(((System.Windows.FrameworkElement)(sender)).DataContext)).Name.ToString();

            // Got index stored, too?

            // Find its index
            //int kf_index = 0;
            //foreach (KeyFrames keyframe in allKeyframes)
            //{
            //    if (keyframe.Name.ToString() == keyframeGoAway)
            //    {
            //        allKeyframes.RemoveAt(kf_index);        // Only remove keyframe if found
            //        Debug.WriteLine("Deleted keyframe at index " + kf_index.ToString());
            //    }
            //    kf_index++;
            //}

            // Id the chosen keyframe and remove it
            for (int keyframeIndex = 0; keyframeIndex < allKeyframes.Count(); keyframeIndex++)
            {   
                // Was .. == keyframeChosen
                if (allKeyframes[keyframeIndex].Name == keyframeGoAway)
                {

                    // Rm any occurances of this KF in the Formula:
                    // Search the Formula for instances of this keyframe: change this name, too, to keep everything updated.

                    for (int formulaElementIndex = 0; formulaElementIndex < allFormulaElements.Count(); formulaElementIndex++)
                    {
                        if (allFormulaElements[formulaElementIndex].Content != null)
                        {
                            // If a keyframe
                            if (allFormulaElements[formulaElementIndex].Content == "keyframe")
                            {
                                if (allFormulaElements[formulaElementIndex].KeyframeName == keyframeGoAway)
                                {
                                    // Erase the Formula storage and view (if in view).
                                    // Not perfect. If two KFs have the same start name, both will be removed..
                                    Formula_eraseGridContent(formulaElementIndex);
                                }
                            }
                        }
                    }


                    //this.KeyFrames.SelectedItem = allKeyframes[keyframeIndex];

                    // Get index of selected item. If same as keyframeIndex, change selected item pre-delete (or error occurs)
                    if ((this.KeyFrames.SelectedIndex == keyframeIndex) && (allKeyframes.Count() > 1))
                    {
                        if (keyframeIndex != 0)
                        {
                            this.KeyFrames.SelectedItem = allKeyframes[0];      // Change selected keyframe to the 0th index.
                        }
                        else
                        {
                            this.KeyFrames.SelectedItem = allKeyframes[1];
                        }
                    }
                    else
                    { 
                        // No more keyframes will exist after removal of this last one.
                    }

                    allKeyframes.RemoveAt(keyframeIndex);        // Only remove keyframe if found. Immediately changes the SelectedIndex.
                    Debug.WriteLine("Deleted keyframe at index " + keyframeIndex.ToString());

                    // If other keyframes exist, change to zero:th one (must exist)
                    //if (allKeyframes.Count() > 0)
                    //{
                    //    this.KeyFrames.SelectedItem = allKeyframes[0];
                    //}
                }
            }

            //KeyFrames selectedKeyframe = ((KeyFrames)src.SelectedItem);

            //var src2 = (KeyFrames)sender;
            //var c2 = src.CaptureMouse();
            //string boboo = src2.Name;
            //string bibi = src2.Color.ToString();

            //    KeyFrames selectedKeyframe = ((KeyFrames)this.KeyFrames.SelectedItem);
            //string currName = selectedKeyframe.Name;
            //((KeyFrames)this.KeyFrames.SelectedItem).Rule = "=";      // Changes the Selected item's rule?

            textChange_by_Keyframe = false;
        }

        private void KeyFrames_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            //tempDebug.Text = tempDebug.Text + "Keyframes MouseRightButtonUp";
            e.Handled = true;
            Debug.WriteLine(">>>>>>>>>>>>>>>>>>>KeyFrames_MouseRightButtonUp <<<<<<<<<<<<<<<<<<<<<<<<");
            textChange_by_Keyframe = false;
        }
        #endregion

        // --------------------------------------
        //
        //         Keyframe Drag-n-Drop to Formula
        //
        // --------------------------------------
        #region Keyframe Drag-n-Drop

        bool textChange_by_Keyframe = false;
        private void KeyFrames_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine(">>>>>>>>>>>>>>>>> In KeyFrames_MouseLeftButtonDown <<<<<<<<<<<<<<<<<<<<<<<");
            e.Handled = true;   // Or the window moves.
            textChange_by_Keyframe = true;      // To stop Storage from updating just by grid calling N_TextChanged

            // Find what Keyframe was clicked
            string isRectangle = sender.ToString();
            if (isRectangle == "System.Windows.Shapes.Rectangle")
            {
                var src = (Rectangle)sender;    // If it was a path, it would be compatible with the Operator Drag n Drop.
                var c = src.CaptureMouse();
                // Manually change selected Keyframe

                //keyframeChosen = src.Tag.ToString();
                ////string hitKeyframeName = ((BioCheck.Views.KeyFrames)(((System.Windows.FrameworkElement)(sender)).DataContext)).Name.ToString();
                //Debug.WriteLine("KeyFrames_MouseLeftButtonDown: Tag used to ID chosen KF = " + keyframeChosen + " ..whereas KF Names are = ");
                src.MouseMove += Keyframe_MouseMove;
                src.MouseLeftButtonUp += Keyframe_MouseLeftButtonUp;
            }
            else
            {
                var src = (TextBox)sender;    // If it was a path, it would be compatible with the Operator Drag n Drop.
                var c = src.CaptureMouse();
                // Manually change selected Keyframe
                //keyframeChosen = src.Tag.ToString();
                //keyframeChosen = src.Text.ToString();           // New idea. But still stuck with unchanging Tag above. Is Tag NECESSARY here?
                src.MouseMove += Keyframe_MouseMove;
                src.MouseLeftButtonUp += Keyframe_MouseLeftButtonUp;

                // If Textbox clicked
            }

            string hitKeyframeName = ((BioCheck.Views.KeyFrames)(((System.Windows.FrameworkElement)(sender)).DataContext)).Name.ToString();

            // Id the chosen keyframe and make it the selected keyframe
            for (int keyframeIndex = 0; keyframeIndex < allKeyframes.Count(); keyframeIndex++)
            {
                Debug.WriteLine(allKeyframes[keyframeIndex].Name.ToString());
                // Was .. == keyframeChosen
                if (allKeyframes[keyframeIndex].Name == hitKeyframeName)
                {
                    this.KeyFrames.SelectedItem = allKeyframes[keyframeIndex];
                }
            }
            // Now load the data for this keyframe
            KeyFrames_SelectionChangeManually();

            // Enable shadow (for possible movement to the Rule area)
            Point offset = e.GetPosition(this);
            Shadow_initialLocus("Keyframe", offset);
            operatorName = "Keyframe";

        }

        private void KeyFrames_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Keyframes MouseLeftButtonUp");
            //tempDebug.Text = tempDebug.Text + "Keyframes MouseLeftButtonUp";
        }
        #endregion

        // --------------------------------------
        //
        //          Mouse interactivity from the Toolbar
        //
        // --------------------------------------
        #region Mouse interactivity from the Toolbar
        string operatorName;            // So it can be accessed globally for mouse move and mouse 

        // A toolbar icon is clicked
        private void Operator_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;           // Stops the window dragging!

            var src = (Path)sender;
            var c = src.CaptureMouse();
            operatorName = src.Name;     // x:Name of the path defines its type. Eg "Equals" or "Variable"

            Debug.WriteLine("Clicked this from Toolbar: " + operatorName);
            Point offset = e.GetPosition(this);   // was LayoutRoot.

            // Enable shadow: only for the toolbar. For objects within the Target site, move those items.
            Shadow_initialLocus(operatorName, offset);

            src.MouseMove += Operator_MouseMove;
            if (operatorName == "Variable" || operatorName == "Cell" || operatorName == "Equals" || operatorName == "LessThan" || operatorName == "MoreThan" )
            {
                // Keyframe grid objects are being dropped
                src.MouseLeftButtonUp += Operator_MouseLeftButtonUp;
            }
            else
            {
                // A Formula grid symbol is being dropped
                src.MouseLeftButtonUp += Keyframe_MouseLeftButtonUp;
            }
            
        }

        // Draw the shadow that accompanies the clicked icon
        private void Shadow_initialLocus(string whatToMove, Point refCoord)
        {
            switch (whatToMove)
            {
                // For some reason, opacity was set to 1 unless I set it here.
                case "Equals":
                    this.Equals_shadow.Visibility = System.Windows.Visibility.Visible;
                    this.Equals_shadow.Opacity = 0.6;
                    this.Equals_shadow.RenderTransform = new TranslateTransform { X = refCoord.X - Equals_shadow.Width / 2, Y = refCoord.Y - Equals_shadow.Height / 2 };
                    break;
                case "Variable":
                    this.Variable_shadow.Visibility = System.Windows.Visibility.Visible;
                    this.Variable_shadow.Opacity = 0.6;
                    this.Variable_shadow.RenderTransform = new TranslateTransform { X = refCoord.X - Variable_shadow.Width / 2, Y = refCoord.Y - Variable_shadow.Height / 2 };
                    break;
                case "Cell":
                    this.Cell_shadow.Visibility = System.Windows.Visibility.Visible;
                    this.Cell_shadow.Opacity = 0.6;
                    this.Cell_shadow.RenderTransform = new TranslateTransform { X = refCoord.X - Cell_shadow.Width / 2, Y = refCoord.Y - Cell_shadow.Height / 2 };
                    break;
                case "LessThan":
                    this.LessThan_shadow.Visibility = System.Windows.Visibility.Visible;
                    this.LessThan_shadow.Opacity = 0.6;
                    this.LessThan_shadow.RenderTransform = new TranslateTransform { X = refCoord.X - LessThan_shadow.Width / 2, Y = refCoord.Y - LessThan_shadow.Height / 2 };
                    break;
                case "MoreThan":
                    this.MoreThan_shadow.Visibility = System.Windows.Visibility.Visible;
                    this.MoreThan_shadow.Opacity = 0.6;
                    this.MoreThan_shadow.RenderTransform = new TranslateTransform { X = refCoord.X - Variable_shadow.Width / 2, Y = refCoord.Y - Variable_shadow.Height / 2 };
                    break;
                case "Keyframe":
                    this.Keyframe_shadow.Fill = new SolidColorBrush(((KeyFrames)this.KeyFrames.SelectedItem).Color);
                    this.Keyframe_shadow.Visibility = System.Windows.Visibility.Visible;
                    this.Keyframe_shadow.Opacity = 0.6;
                    this.Keyframe_shadow.RenderTransform = new TranslateTransform { X = refCoord.X - Variable_shadow.Width / 2, Y = refCoord.Y - Variable_shadow.Height / 2 };
                    break;
                case "Until":
                    this.Until_shadow.Visibility = System.Windows.Visibility.Visible;
                    this.Until_shadow.Opacity = 0.6;
                    this.Until_shadow.RenderTransform = new TranslateTransform { X = refCoord.X - Until_shadow.Width / 2, Y = refCoord.Y - Until_shadow.Height / 2 };
                    break;
                case "Implies":
                    this.Implies_shadow.Visibility = System.Windows.Visibility.Visible;
                    this.Implies_shadow.Opacity = 0.6;
                    this.Implies_shadow.RenderTransform = new TranslateTransform { X = refCoord.X - Implies_shadow.Width / 2, Y = refCoord.Y - Implies_shadow.Height / 2 };
                    break;
                case "Eventually":
                    this.Eventually_shadow.Visibility = System.Windows.Visibility.Visible;
                    this.Eventually_shadow.Opacity = 0.6;
                    this.Eventually_shadow.RenderTransform = new TranslateTransform { X = refCoord.X - Eventually_shadow.Width / 2, Y = refCoord.Y - Eventually_shadow.Height / 2 };
                    break;
                case "Always":
                    this.Always_shadow.Visibility = System.Windows.Visibility.Visible;
                    this.Always_shadow.Opacity = 0.6;
                    this.Always_shadow.RenderTransform = new TranslateTransform { X = refCoord.X - Always_shadow.Width / 2, Y = refCoord.Y - Always_shadow.Height / 2 };
                    break;
                case "Not":
                    this.Not_shadow.Visibility = System.Windows.Visibility.Visible;
                    this.Not_shadow.Opacity = 0.6;
                    this.Not_shadow.RenderTransform = new TranslateTransform { X = refCoord.X - Not_shadow.Width / 2, Y = refCoord.Y - Not_shadow.Height / 2 };
                    break;
                case "If":
                    this.If_shadow.Visibility = System.Windows.Visibility.Visible;
                    this.If_shadow.Opacity = 0.6;
                    this.If_shadow.RenderTransform = new TranslateTransform { X = refCoord.X - If_shadow.Width / 2, Y = refCoord.Y - If_shadow.Height / 2 };
                    break;
                case "And":
                    this.And_shadow.Visibility = System.Windows.Visibility.Visible;
                    this.And_shadow.Opacity = 0.6;
                    this.And_shadow.RenderTransform = new TranslateTransform { X = refCoord.X - And_shadow.Width / 2, Y = refCoord.Y - And_shadow.Height / 2 };
                    break;
                case "ob":
                    this.ob_shadow.Visibility = System.Windows.Visibility.Visible;
                    this.ob_shadow.Opacity = 0.6;
                    this.ob_shadow.RenderTransform = new TranslateTransform { X = refCoord.X - ob_shadow.Width / 2, Y = refCoord.Y - ob_shadow.Height / 2 };
                    break;
                case "cb":
                    this.cb_shadow.Visibility = System.Windows.Visibility.Visible;
                    this.cb_shadow.Opacity = 0.6;
                    this.cb_shadow.RenderTransform = new TranslateTransform { X = refCoord.X - cb_shadow.Width / 2, Y = refCoord.Y - cb_shadow.Height / 2 };
                    break;
            }
        }

        private void Keyframe_MouseMove(object sender, MouseEventArgs e)
        {
            //var src = (Rectangle)sender;
            var pos = e.GetPosition(this);  

            // Move the shadow
            this.Keyframe_shadow.RenderTransform = new TranslateTransform { X = pos.X - Keyframe_shadow.Width / 2, Y = pos.Y - Keyframe_shadow.Height / 2 };
        }

        // Mouse-up from the Keyframe: make the shadow disappear, 
        // possibly update the Formula grid content (if landed in Formula)
        private void Keyframe_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>>>>>Keyframe_MouseLeftButtonUp <<<<<<<<<<<<<<<<<<<<<<<<<");
            // Make shadow disappear
            Shadow_collapse();
            string whatContent = "logic";

            string isRectangle = sender.ToString();
            if (isRectangle == "System.Windows.Shapes.Rectangle")
            {
                var src = (Rectangle)sender;
                src.MouseLeftButtonUp -= Keyframe_MouseLeftButtonUp;
                src.MouseMove -= Keyframe_MouseMove;
                src.ReleaseMouseCapture();
                src.RenderTransform = null;
                whatContent = "keyframe";
            }
            else 
            {
                var src = (Path)sender;
                src.MouseLeftButtonUp -= Keyframe_MouseLeftButtonUp;
                src.MouseMove -= Operator_MouseMove;
                src.ReleaseMouseCapture();
                src.RenderTransform = null;
            }
            
            //src.MouseLeftButtonUp -= Keyframe_MouseLeftButtonUp;
            //src.MouseMove -= Keyframe_MouseMove;
            //src.ReleaseMouseCapture();
            //src.RenderTransform = null;

            // Update this current keyframe's pathstrings so that the drop is stored


            textForTempDebug = textForTempDebug + "\n Dragged Keyframe";
            // Retrieve all elements under the cursor
            var pos = e.GetPosition(null /*LayoutRoot*/);
            var elementsUnderPointer = VisualTreeHelper.FindElementsInHostCoordinates(pos, this);   // Was LayoutRoot

            // If it landed on a droppable area, make it show.
            if (elementsUnderPointer.Contains(f_rect1) && f_isEmpty(1))
            {
                restylePath(f_path1, 1, operatorName, whatContent, f_N1, f_text1);
            }
            else if (elementsUnderPointer.Contains(f_rect2) && f_isEmpty(2))
            {
                restylePath(f_path2, 2, operatorName, whatContent, f_N2, f_text2);
                tempDebug.Text = "sending operatorName = " + operatorName + " and whatContent = " + whatContent + "to restylePath.";
            }
            else if (elementsUnderPointer.Contains(f_rect3) && f_isEmpty(3))
            {
                restylePath(f_path3, 3, operatorName, whatContent, f_N3, f_text3);
            }
            else if (elementsUnderPointer.Contains(f_rect4) && f_isEmpty(4))
            {
                restylePath(f_path4, 4, operatorName, whatContent, f_N4, f_text4);
            }
            else if (elementsUnderPointer.Contains(f_rect5) && f_isEmpty(5))
            {
                restylePath(f_path5, 5, operatorName, whatContent, f_N5, f_text5);
            }
            else if (elementsUnderPointer.Contains(f_rect6) && f_isEmpty(6))
            {
                restylePath(f_path6, 6, operatorName, whatContent, f_N6, f_text6);
            }
            else if (elementsUnderPointer.Contains(f_rect7) && f_isEmpty(7))
            {
                restylePath(f_path7, 7, operatorName, whatContent, f_N7, f_text7);
            }
            else if (elementsUnderPointer.Contains(f_rect8) && f_isEmpty(8))
            {
                restylePath(f_path8, 8, operatorName, whatContent, f_N8, f_text8);
            }
            else if (elementsUnderPointer.Contains(f_rect9) && f_isEmpty(9))
            {
                restylePath(f_path9, 9, operatorName, whatContent, f_N9, f_text9);
            }
            else if (elementsUnderPointer.Contains(f_rect10) && f_isEmpty(10))
            {
                restylePath(f_path10, 10, operatorName, whatContent, f_N10, f_text10);
            }
            //src.RenderTransform = null;


            // Print out to check change
            //string allStorage = "";
            //Debug.WriteLine("Updated formula: ");
            //foreach (string oneGridsContent in Formula)
            //{
            //    allStorage = allStorage + oneGridsContent + ", ";
            //}
            //Debug.WriteLine(allStorage);
            //textForTempDebug = textForTempDebug + "Updated formula : " + allStorage + "\n";

            //allStorage = "";
            //Debug.WriteLine("Updated varname storage: ");
            //foreach (string oneGridsContent in ((KeyFrames)this.KeyFrames.SelectedItem).NameContent)
            //{
            //    allStorage = allStorage + oneGridsContent + ", ";
            //}
            //Debug.WriteLine(allStorage);
            //textForTempDebug = textForTempDebug + "\n Updated varname storage : " + allStorage;
            //tempDebug.Text = textForTempDebug;
            Keyframe_Storage_checker();
            textChange_by_Keyframe = false;      // Re-initiate Storage-updates by text change in grids
        }


        // A toolbar icon is clicked-and-dragged: move the shadow with the mouse
        private void Operator_MouseMove(object sender, MouseEventArgs e)
        {
            var src = (Path)sender;
            var pos = e.GetPosition(this);      // Was LayoutRoot

            // Move the shadow

            //if (operatorName == "Equals")
            //{
            //    this.Equals_shadow.RenderTransform = new TranslateTransform { X = pos.X - Equals_shadow.Width / 2, Y = pos.Y - Equals_shadow.Height / 2 };
            //}
            //else if (operatorName == "Variable")
            //{
            //    this.Variable_shadow.RenderTransform = new TranslateTransform { X = pos.X - Variable_shadow.Width / 2, Y = pos.Y - Variable_shadow.Height / 2 };
            //}
            //else if (operatorName == "Cell")
            //{
            //    this.Cell_shadow.RenderTransform = new TranslateTransform { X = pos.X - Cell_shadow.Width / 2, Y = pos.Y - Cell_shadow.Height / 2 };
            //}
            //else if (operatorName == "LessThan")
            //{
            //    this.LessThan_shadow.RenderTransform = new TranslateTransform { X = pos.X - LessThan_shadow.Width / 2, Y = pos.Y - LessThan_shadow.Height / 2 };
            //}
            //else if (operatorName == "MoreThan")
            //{
            //    this.MoreThan_shadow.RenderTransform = new TranslateTransform { X = pos.X - MoreThan_shadow.Width / 2, Y = pos.Y - MoreThan_shadow.Height / 2 };
            //}
            //else if (operatorName == "Keyframe")
            //{
            //    this.Keyframe_shadow.RenderTransform = new TranslateTransform { X = pos.X - Keyframe_shadow.Width / 2, Y = pos.Y - Keyframe_shadow.Height / 2 };
            //}

            switch (operatorName)
            {
                // For some reason, opacity was set to 1 unless I set it here.
                case "Equals":
                    this.Equals_shadow.RenderTransform = new TranslateTransform { X = pos.X - Equals_shadow.Width / 2, Y = pos.Y - Equals_shadow.Height / 2 };
                    break;
                case "Variable":
                    this.Variable_shadow.RenderTransform = new TranslateTransform { X = pos.X - Variable_shadow.Width / 2, Y = pos.Y - Variable_shadow.Height / 2 };
                    break;
                case "Cell":
                    this.Cell_shadow.RenderTransform = new TranslateTransform { X = pos.X - Cell_shadow.Width / 2, Y = pos.Y - Cell_shadow.Height / 2 };
                    break;
                case "LessThan":
                    this.LessThan_shadow.RenderTransform = new TranslateTransform { X = pos.X - LessThan_shadow.Width / 2, Y = pos.Y - LessThan_shadow.Height / 2 };
                    break;
                case "MoreThan":
                    this.MoreThan_shadow.RenderTransform = new TranslateTransform { X = pos.X - MoreThan_shadow.Width / 2, Y = pos.Y - MoreThan_shadow.Height / 2 };
                    break;
                case "Keyframe":
                    this.Keyframe_shadow.RenderTransform = new TranslateTransform { X = pos.X - Keyframe_shadow.Width / 2, Y = pos.Y - Keyframe_shadow.Height / 2 };
                    break;
                case "Until":
                    this.Until_shadow.RenderTransform = new TranslateTransform { X = pos.X - Until_shadow.Width / 2, Y = pos.Y - Until_shadow.Height / 2 };
                    break;
                case "Implies":
                    this.Implies_shadow.RenderTransform = new TranslateTransform { X = pos.X - Implies_shadow.Width / 2, Y = pos.Y - Implies_shadow.Height / 2 };
                    break;
                case "Eventually":
                    this.Eventually_shadow.RenderTransform = new TranslateTransform { X = pos.X - Eventually_shadow.Width / 2, Y = pos.Y - Eventually_shadow.Height / 2 };                
                    break;
                case "Always":
                    this.Always_shadow.RenderTransform = new TranslateTransform { X = pos.X - Always_shadow.Width / 2, Y = pos.Y - Always_shadow.Height / 2 };
                    break;
                case "Not":
                    this.Not_shadow.RenderTransform = new TranslateTransform { X = pos.X - Not_shadow.Width / 2, Y = pos.Y - Not_shadow.Height / 2 };
                    break;
                case "If":
                    this.If_shadow.RenderTransform = new TranslateTransform { X = pos.X - If_shadow.Width / 2, Y = pos.Y - If_shadow.Height / 2 };
                    break;
                case "And":
                    this.And_shadow.RenderTransform = new TranslateTransform { X = pos.X - And_shadow.Width / 2, Y = pos.Y - And_shadow.Height / 2 };
                    break;
                case "ob":
                    this.ob_shadow.RenderTransform = new TranslateTransform { X = pos.X - ob_shadow.Width / 2, Y = pos.Y - ob_shadow.Height / 2 };
                    break;
                case "cb":
                    this.cb_shadow.RenderTransform = new TranslateTransform { X = pos.X - cb_shadow.Width / 2, Y = pos.Y - cb_shadow.Height / 2 };
                    break;
            }
        }

        // Make the shadow disappear
        private void Shadow_collapse()
        {
            switch (operatorName)
            {
                case "Equals":
                    this.Equals_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "Variable":
                    this.Variable_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "Cell":
                    this.Cell_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "LessThan":
                    this.LessThan_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "MoreThan":
                    this.MoreThan_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "Keyframe":
                    this.Keyframe_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "Until":
                    this.Until_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "Implies":
                    this.Implies_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "Eventually":
                    this.Eventually_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "Always":
                    this.Always_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "Not":
                    this.Not_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "If":
                    this.If_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "And":
                    this.And_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "ob":
                    this.ob_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "cb":
                    this.cb_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
            }
        }

        // Update the grid index' Path AND (some) storage - make separate function for storage?
        private void restylePath(Path pathN, int rectInt, string whatResourceName, string whatContentSign, TextBox boxN, TextBlock textN)
        {
            //restylePath(f_pathID(whatVisIndexToDraw), whatVisIndexToDraw, allFormulaElements[formulaElementIndex - 1].Content, "storageDone", f_textboxID(whatVisIndexToDraw), f_textID(whatVisIndexToDraw));
            string compositeResourceName = "";

            // Delete or create visuals at this grid?
            if (whatContentSign == null)
            {
                // Delete visuals
                compositeResourceName = "EmptyStyle";
                pathN.Style = (Style)this.Resources[compositeResourceName];
                textN.Text = "";
                boxN.Text = "";
                boxN.IsHitTestVisible = true;
                boxN.IsReadOnly = false;                
            }
            else
            {
                // Create visuals
                // Draw the correct path in the correct place
                compositeResourceName = "path" + rectInt.ToString() + whatResourceName.ToLower();
                pathN.Style = (Style)this.Resources[compositeResourceName];
                tempDebug.Text += "Tried to draw path " + compositeResourceName + " at visual element " + rectInt.ToString();

                // Keyframe storage only
                if (whatContentSign == "var" || whatContentSign == "cell" || whatContentSign == "N" || whatContentSign == "<" || whatContentSign == ">" || whatContentSign == "=")
                {
                    ((KeyFrames)this.KeyFrames.SelectedItem).Content[rectInt] = whatContentSign;
                }
                else if (whatContentSign == "storageDone")
                {
                    // Scrolling within the Formula grid. Storage is done, and rectangle color and text is retrievable from curr storage.
                    if (whatResourceName == "keyframe")
                    {
                        pathN.Fill = new SolidColorBrush(allFormulaElements[rectInt + N_rightScrollClicks].KeyframeColor);
                        textN.Text = allFormulaElements[rectInt + N_rightScrollClicks].KeyframeName;
                        tempDebug.Text += "Drew visuals at element " + rectInt + ": Put storage name " + allFormulaElements[rectInt + N_rightScrollClicks].KeyframeName + " onto color " + allFormulaElements[rectInt + N_rightScrollClicks].KeyframeColor.ToString();
                    }
                    else
                    {
                        // Logics was moved by scroll. 
                        // Use the rule to retrieve what path to draw.
                        whatResourceName = allFormulaElements[rectInt + N_rightScrollClicks].Rule;
                        compositeResourceName = "path" + rectInt.ToString() + whatResourceName.ToLower();
                        pathN.Style = (Style)this.Resources[compositeResourceName];
                        tempDebug.Text += "Tried to draw path " + compositeResourceName + " at visual element " + rectInt.ToString();
                        if (whatResourceName == "Not")
                        {
                            pathN.Fill = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            pathN.Fill = new SolidColorBrush(Colors.Black);
                        }
                    }
                }
                else
                {
                    // Keyframe newly dropped into the Formula grid. Storage needs editing.
                    allFormulaElements[rectInt + N_rightScrollClicks].Content = whatContentSign;
                    
                    if (whatResourceName == "Keyframe")
                    {
                        pathN.Fill = new SolidColorBrush(((KeyFrames)this.KeyFrames.SelectedItem).Color);
                        textN.Text = ((KeyFrames)this.KeyFrames.SelectedItem).Name;
                        allFormulaElements[rectInt + N_rightScrollClicks].Rule = ((KeyFrames)this.KeyFrames.SelectedItem).Rule;
                        allFormulaElements[rectInt + N_rightScrollClicks].KeyframeName = ((KeyFrames)this.KeyFrames.SelectedItem).Name;
                        allFormulaElements[rectInt + N_rightScrollClicks].KeyframeColor = ((KeyFrames)this.KeyFrames.SelectedItem).Color;
                    }
                    else
                    {
                        // Logics was dragged.
                        allFormulaElements[rectInt + N_rightScrollClicks].Rule = whatResourceName;   // "Until", "If", "Eventually", "Always", "cb", "ob" et.c.
                        if (whatResourceName == "Not")
                        {
                            pathN.Fill = new SolidColorBrush(Colors.Red);
                        }
                        else
                        {
                            pathN.Fill = new SolidColorBrush(Colors.Black);
                        }
                    }
                }

                // Make values not enterable.
                boxN.IsHitTestVisible = false;
                boxN.IsReadOnly = true;
                pathN.AllowDrop = false;

                // Debug messages
                Debug.WriteLine("Landed on a droppable area! Using path (whatResourceName) = " + whatResourceName + ", for Formula element = " + rectInt.ToString());
                //textForTempDebug = textForTempDebug + "\n" + "Landed on a droppable area! Storing " + whatResourceName + " in Rectangle " + rectInt.ToString() + "\n";
            }            
        }

        private void restylePath_byScroll(Path rectN, int rectInt, int storageIndex, string whatResourceName, string whatContentSign, TextBox boxN, TextBlock textN)
        {

            //restylePath(f_pathID(whatVisIndexToDraw), whatVisIndexToDraw, allFormulaElements[formulaElementIndex - 1].Content, "storageDone", f_textboxID(whatVisIndexToDraw), f_textID(whatVisIndexToDraw));
            string compositeResourceName = "";
           
            // Create visuals
            // Draw the correct path in the correct place
            compositeResourceName = "path" + rectInt.ToString() + whatResourceName.ToLower();
            rectN.Style = (Style)this.Resources[compositeResourceName];

            // Keyframe storage only
            if (whatContentSign == "var" || whatContentSign == "cell" || whatContentSign == "N" || whatContentSign == "<" || whatContentSign == ">" || whatContentSign == "=")
            {
                ((KeyFrames)this.KeyFrames.SelectedItem).Content[rectInt] = whatContentSign;
            }
            else if (whatContentSign == "storageDone")
            {
                // Scrolling within the Formula grid. Storage is done, and rectangle color and text is retrievable from curr storage.
                if (whatResourceName == "keyframe")
                {
                    rectN.Fill = new SolidColorBrush(allFormulaElements[rectInt].KeyframeColor);
                    textN.Text = allFormulaElements[rectInt].KeyframeName;
                }
                tempDebug.Text += "allFormulaElements index number " + rectInt + ": Trying to put name " + allFormulaElements[rectInt].KeyframeName + " onto color " + allFormulaElements[rectInt].KeyframeColor.ToString();
            }
            else
            {
                // Keyframe newly dropped into the Formula grid. Storage needs editing.
                allFormulaElements[rectInt].Content = whatContentSign;

                if (whatResourceName == "Keyframe")
                {
                    rectN.Fill = new SolidColorBrush(((KeyFrames)this.KeyFrames.SelectedItem).Color);
                    textN.Text = ((KeyFrames)this.KeyFrames.SelectedItem).Name;
                    allFormulaElements[rectInt].Rule = ((KeyFrames)this.KeyFrames.SelectedItem).Rule;
                    allFormulaElements[rectInt].KeyframeName = ((KeyFrames)this.KeyFrames.SelectedItem).Name;
                    allFormulaElements[rectInt].KeyframeColor = ((KeyFrames)this.KeyFrames.SelectedItem).Color;
                }
                else
                {
                    // Logics was dragged.
                    allFormulaElements[rectInt].Rule = whatResourceName;   // "Until", "If", "Eventually", "Always", "cb", "ob" et.c.
                }
            }

            // Make values not enterable.
            boxN.IsHitTestVisible = false;
            boxN.IsReadOnly = true;
            rectN.AllowDrop = false;

            // Debug messages
            Debug.WriteLine("Landed on a droppable area! Using path = " + rectN.ToString() + ", rectInt = " + rectInt.ToString() + ", whatResourceName = " + whatResourceName + ", for boxN = " + boxN.ToString());
            //textForTempDebug = textForTempDebug + "\n" + "Landed on a droppable area! Storing " + whatResourceName + " in Rectangle " + rectInt.ToString() + "\n";
        }

        // Mouse-up from the toolbar: make the shadow disappear
        private void Operator_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Make shadow disappear
            Shadow_collapse();

            var src = (Path)sender;
            src.MouseLeftButtonUp -= Operator_MouseLeftButtonUp;
            src.MouseMove -= Operator_MouseMove;

            src.ReleaseMouseCapture();

            // If there are no keyframes, no movement/ dragndrop is allowed.
            if (allKeyframes.Count() > 0)
            {

                // Update this current keyframe's pathstrings so that the drop is stored
                string whatResourceName = "equals";
                string whatContentSign = "=";
                switch (operatorName)
                {
                    case "Equals":
                        whatResourceName = "equals";
                        whatContentSign = "=";
                        break;
                    case "Variable":
                        whatResourceName = whatContentSign = "var";
                        break;
                    case "LessThan":
                        whatResourceName = "lt";
                        whatContentSign = "<";
                        break;
                    case "MoreThan":
                        whatResourceName = "mt";
                        whatContentSign = whatContentSign = ">";
                        break;
                    case "Cell":
                        whatResourceName = whatContentSign = "cell";
                        break;
                }
                textForTempDebug = textForTempDebug + "\n Dragged " + operatorName;
                // Retrieve all elements under the cursor
                var pos = e.GetPosition(null /*LayoutRoot*/);
                var elementsUnderPointer = VisualTreeHelper.FindElementsInHostCoordinates(pos, this);   // Was LayoutRoot

                // If it landed on a droppable area, make it show.
                if ((elementsUnderPointer.Contains(rect0)) && isEmpty(0))
                {
                    restylePath(path0, 0, whatResourceName, whatContentSign, N0, text0);
                }
                else if (elementsUnderPointer.Contains(rect1) && isEmpty(1))
                {
                    restylePath(path1, 1, whatResourceName, whatContentSign, N1, text1);
                }
                else if (elementsUnderPointer.Contains(rect2) && isEmpty(2))
                {
                    restylePath(path2, 2, whatResourceName, whatContentSign, N2, text2);
                }
                else if (elementsUnderPointer.Contains(rect3) && isEmpty(3))
                {
                    restylePath(path3, 3, whatResourceName, whatContentSign, N3, text3);
                }
                else if (elementsUnderPointer.Contains(rect4) && isEmpty(4))
                {
                    restylePath(path4, 4, whatResourceName, whatContentSign, N4, text4);
                }
                src.RenderTransform = null;

                // Print out to check change
                string allStorage = "";
                Debug.WriteLine("Updated storage: ");
                foreach (string oneGridsContent in ((KeyFrames)this.KeyFrames.SelectedItem).Content)
                {
                    allStorage = allStorage + oneGridsContent + ", ";
                }
                Debug.WriteLine(allStorage);
                textForTempDebug = textForTempDebug + "Updated storage : " + allStorage + "\n";

                allStorage = "";
                Debug.WriteLine("Updated varname storage: ");
                foreach (string oneGridsContent in ((KeyFrames)this.KeyFrames.SelectedItem).NameContent)
                {
                    allStorage = allStorage + oneGridsContent + ", ";
                }
                Debug.WriteLine(allStorage);
                textForTempDebug = textForTempDebug + "\n Updated varname storage : " + allStorage;
                //tempDebug.Text = textForTempDebug;
                Keyframe_Storage_checker();
            }
        }
        #endregion

        // ------------------------------------------------
        //
        //              Grid: Moving items about
        //              ..and renaming them.
        //
        // ------------------------------------------------

        #region Keyframe Grid move elements around
        string objectClicked;
        string styleClicked;
        int gridLocusClicked;

        private void What_style()
        {
            switch (objectClicked)
            {
                case "=":
                    styleClicked = "path" + gridLocusClicked.ToString() + "equals";
                    break;
                case ">":
                    styleClicked = "path" + gridLocusClicked.ToString() + "mt";
                    break;
                case "<":
                    styleClicked = "path" + gridLocusClicked.ToString() + "lt";
                    break;
                default:
                    styleClicked = "path" + gridLocusClicked.ToString() + objectClicked;
                    break;
            }
        }

        // Click a path placed in a gridbox
        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;           // Stops the window dragging! Works only for Equals.

            // If there are no keyframes, no movement is allowed.
            if (allKeyframes.Count() > 0)
            {

                var src = (Path)sender;


                // Is anything under the cursor?
                // If nothing or just a number, do not start dragging shadows around.
                gridLocusClicked = Int32.Parse((string)src.Tag);       // The grid index
                if (((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusClicked] != null && ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusClicked] != "N")
                {

                    objectClicked = ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusClicked];
                    Debug.WriteLine("Clicked the item: " + objectClicked);
                    What_style();

                    var c = src.CaptureMouse();
                    src.MouseMove += Grid_MouseMove;
                    src.MouseLeftButtonUp += Grid_MouseLeftButtonUp;

                    //Point offset = e.GetPosition(null);
                    Point offset = e.GetPosition(this);       // Original!
                    //Point offset2 = e.GetPosition(this.Topgrid);

                    // What's underneath?
                    //var pos = e.GetPosition(this);
                    //var elementsUnderPointer0 = VisualTreeHelper.FindElementsInHostCoordinates(offset0, this.Topgrid);   // Was LayoutRoot
                    //var elementsUnderPointer = VisualTreeHelper.FindElementsInHostCoordinates(offset, this.Topgrid);   // Was LayoutRoot
                    //var elementsUnderPointer2 = VisualTreeHelper.FindElementsInHostCoordinates(offset2, this.Topgrid);   // Was LayoutRoot
                    //var stuff = SilverlightVisualTreeHelper.FindVisualObjectByTypeVisualHelper(rect0);

                    Path shadowToEdit = Equals_shadow;

                    switch (objectClicked)
                    {
                        case "=":
                            shadowToEdit = Equals_shadow;
                            break;
                        case ">":
                            shadowToEdit = MoreThan_shadow;
                            break;
                        case "<":
                            shadowToEdit = LessThan_shadow;
                            break;
                        case "var":
                            shadowToEdit = Variable_shadow;
                            break;
                        case "cell":
                            shadowToEdit = Cell_shadow;
                            break;
                    }
                    shadowToEdit.RenderTransform = new TranslateTransform { X = offset.X - shadowToEdit.Width / 2, Y = offset.Y - shadowToEdit.Height / 2 };
                    shadowToEdit.Opacity = 1.0;
                    shadowToEdit.Visibility = System.Windows.Visibility.Visible;

                    // Make the original path disappear
                    TextBlock exactTextBlock = (TextBlock)textID(gridLocusClicked);
                    Path exactPath = (Path)pathID(gridLocusClicked);
                    TextBox exactTextBox = (TextBox)textboxID(gridLocusClicked);
                    Rectangle exactRect = (Rectangle)rectID(gridLocusClicked);

                    exactPath.Style = (Style)this.Resources["EmptyStyle"];
                    exactTextBlock.Text = "";
                    exactRect.AllowDrop = true;
                    exactTextBox.IsHitTestVisible = true;
                    exactTextBox.IsReadOnly = false;


                    //switch (gridLocusClicked)
                    //{
                    //    case 0:
                    //        path0.Style = (Style)this.Resources["EmptyStyle"];
                    //        text0.Text = "";
                    //        rect0.AllowDrop = true;
                    //        N0.IsHitTestVisible = true;
                    //        break;
                    //    case 1:
                    //        path1.Style = (Style)this.Resources["EmptyStyle"];
                    //        text1.Text = "";
                    //        rect1.AllowDrop = true;
                    //        N1.IsHitTestVisible = true;
                    //        break;
                    //    case 2:
                    //        path2.Style = (Style)this.Resources["EmptyStyle"];
                    //        text2.Text = "";
                    //        rect2.AllowDrop = true;
                    //        N2.IsHitTestVisible = true;
                    //        break;
                    //    case 3:
                    //        path3.Style = (Style)this.Resources["EmptyStyle"];
                    //        text3.Text = "";
                    //        rect3.AllowDrop = true;
                    //        N3.IsHitTestVisible = true;
                    //        break;
                    //    case 4:
                    //        path4.Style = (Style)this.Resources["EmptyStyle"];
                    //        text4.Text = "";
                    //        rect4.AllowDrop = true;
                    //        N4.IsHitTestVisible = true;
                    //        break;
                    //}

                    // ID the object being clicked, to make the correct shadow..  No shadow! Just move the object clicked.
                    // Make the grid object empty and have a completely opaque shadow move about.
                    // Need to know what path to move, and move any contained detail: Name or Number too.

                    // Enable shadow: only for the toolbar. For objects within the Target site, move those items.
                    //this.Equals_shadow.Visibility = System.Windows.Visibility.Visible;
                    //this.Equals_shadow.RenderTransform = new TranslateTransform { X = offset.X - Equals_shadow.Width / 2, Y = offset.Y - Equals_shadow.Height / 2 };

                    // Only one thing can be moved at a time,
                    // So I could populate variables according to the object clicked.
                    // ID: by tag (if a string can hold that much)
                    // or by curr keyframe and 
                }


            }
            
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            var src = (Path)sender;
            var pos = e.GetPosition(this);      // Was LayoutRoot

            // Move the shadow don shadow object type
            switch (objectClicked)
            {
                case "=":
                    this.Equals_shadow.RenderTransform = new TranslateTransform { X = pos.X - Equals_shadow.Width / 2, Y = pos.Y - Equals_shadow.Height / 2 };
                    break;
                case ">":
                    this.MoreThan_shadow.RenderTransform = new TranslateTransform { X = pos.X - MoreThan_shadow.Width / 2, Y = pos.Y - MoreThan_shadow.Height / 2 };
                    break;
                case "<":
                    this.LessThan_shadow.RenderTransform = new TranslateTransform { X = pos.X - LessThan_shadow.Width / 2, Y = pos.Y - LessThan_shadow.Height / 2 };
                    break;
                case "var":
                    this.Variable_shadow.RenderTransform = new TranslateTransform { X = pos.X - Variable_shadow.Width / 2, Y = pos.Y - Variable_shadow.Height / 2 };
                    break;
                case "cell":
                    this.Cell_shadow.RenderTransform = new TranslateTransform { X = pos.X - Cell_shadow.Width / 2, Y = pos.Y - Cell_shadow.Height / 2 };
                    break;
            }
        }

        // For rects to make renaming available, too
        private void Rect_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var src = (Rectangle)sender;
            int gridLocusEntered = Int32.Parse((string)src.Tag);        // The grid index
            //var xy = e.GetPosition(this);         // Original
            //var xy = e.GetPosition(null);             // Just used. Can't open dropdown
            Point xy = e.GetPosition(this /*LayoutRoot*/);        // Original?
            //var yx = e.GetPosition(this.Topgrid);  
            //var xz = e.GetPosition(null);                               // Mouse loci wrt null            

            // If there are no keyframes, no movement is allowed.
            if (allKeyframes.Count() > 0)
            {

                if (!isEmpty(gridLocusEntered))
                {
                    // Only call rename if a relevant object occupies the cell
                    //Transform transforms loci wrt start-loci!
                    //ComboBox whichBox = dropdownCell;

                    //switch (((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered])
                    //{
                    //    case "cell":
                    //        whichBox = this.dropdownCell;
                    //        break;
                    //    case "var":
                    //        whichBox = this.dropdownVar;
                    //        break;
                    //    case "N":
                    //        break;
                    //}
                    //whichBox.RenderTransform = new TranslateTransform { X = xy.X - whichBox.Width * 6, Y = xy.Y = whichBox.Height };
                    //whichBox.Visibility = System.Windows.Visibility.Visible;

                    //case "cell":
                    //    this.dropdownCell.RenderTransform = new TranslateTransform { X = xy.X - dropdownCell.Width * 6, Y = xy.Y = dropdownCell.Height };
                    //    this.dropdownCell.Visibility = System.Windows.Visibility.Visible;
                    //    break;
                    //case "var":
                    //    this.dropdownVar.RenderTransform = new TranslateTransform { X = xy.X - dropdownVar.Width * 6, Y = xy.Y - dropdownVar.Height };
                    //    this.dropdownVar.Visibility = System.Windows.Visibility.Visible;
                    //    break;
                    //case "N":
                    //    break;
                    // Encapsulating this made the box appear at the top of the screen, for some reason.
                    switch (((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered])
                    {
                        case "cell":
                            this.dropdownCell.RenderTransform = new TranslateTransform { X = xy.X - dropdownCell.Width, Y = xy.Y - dropdownCell.Height * 10 };
                            this.dropdownCell.Visibility = System.Windows.Visibility.Visible;
                            nameChangeGridN = gridLocusEntered;
                            break;
                        case "var":
                            this.dropdownVar.RenderTransform = new TranslateTransform { X = xy.X - dropdownVar.Width, Y = xy.Y - dropdownVar.Height * 10 };
                            Debug.WriteLine("Put the Combobox at " + xy.X.ToString() + " and " + xy.Y.ToString());
                            this.dropdownVar.Visibility = System.Windows.Visibility.Visible;
                            nameChangeGridN = gridLocusEntered;
                            break;
                    }
                    Keyframe_Storage_checker();
                }
            }
        }

        private void Shadow_collapseByMove()
        {
            switch (objectClicked)
            {
                case "=":
                    this.Equals_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case ">":
                    this.MoreThan_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "<":
                    this.LessThan_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "var":
                    this.Variable_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
                case "cell":
                    this.Cell_shadow.Visibility = System.Windows.Visibility.Collapsed;
                    break;
            }
        }

        private void updateMovedGrid(Rectangle rectN, Path pathN, TextBlock textN, int beforeGrid, int nowGrid, TextBox boxN)
        {
            rectN.AllowDrop = false;

            string resourceStyleToUse = "";
            // Original object; what's being moved.
            // NB Default covers cell, var and N.
            switch (objectClicked)
            {
                case "=":
                    resourceStyleToUse = "path" + nowGrid.ToString() + "equals";
                    break;
                case ">":
                    resourceStyleToUse = "path" + nowGrid.ToString() + "mt";
                    break;
                case "<":
                    resourceStyleToUse = "path" + nowGrid.ToString() + "lt";
                    break;
                default:
                    resourceStyleToUse = "path" + nowGrid.ToString() + objectClicked;
                    break;
            }

            // Update this grid's visuals and stored data
            pathN.Style = (Style)this.Resources[resourceStyleToUse];
            textN.Text = ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[beforeGrid];
            boxN.IsHitTestVisible = false;
            boxN.IsReadOnly = true;
            ((KeyFrames)this.KeyFrames.SelectedItem).Content[nowGrid] = objectClicked;      // e.g "="
            ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[nowGrid] = (string)textN.Text;

            //Erase the former grid's stored data (visuals are already gone)
            ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[beforeGrid] = null;
            ((KeyFrames)this.KeyFrames.SelectedItem).Content[beforeGrid] = null;
            Keyframe_Storage_checker(); 
        }

        

        int nameChangeGridN = 0;
        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

            var src = (Path)sender;
            var pos = e.GetPosition(null);
            //var pos = e.GetPosition(this);            // Original
            src.MouseLeftButtonUp -= Grid_MouseLeftButtonUp;
            src.MouseMove -= Grid_MouseMove;

            src.ReleaseMouseCapture();
            // End default

            // Make shadow disappear don type
            Shadow_collapseByMove();

            //this.Equals_shadow.Visibility = System.Windows.Visibility.Collapsed;


            // If there are no keyframes, no movement is allowed.
            if (allKeyframes.Count() > 0)
            {
                Point xy = e.GetPosition(this /*LayoutRoot*/);        // Original?
                //Point xy = e.GetPosition(null /*LayoutRoot*/);
                var elementsUnderPointer = VisualTreeHelper.FindElementsInHostCoordinates(pos, this);   // Was LayoutRoot
                //var elementsUnderPointer = VisualTreeHelper.FindElementsInHostCoordinates(pos, this);   // Was LayoutRoot
            

                // Note the spot where drop occurred, 
                // and update this current keyframe's pathstrings so that the drop is stored

                //bool droppedOnTarget = elementsUnderPointer.Contains(rect0);
                //Debug.WriteLine(droppedOnTarget);

                // Store former ContentName
                string exContentName = ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridLocusClicked];

                if (elementsUnderPointer.Contains(rect0))
                {
                    if (isEmpty(0))
                    {
                        updateMovedGrid(rect0, path0, text0, gridLocusClicked, 0, N0);
                    }
                    else
                    {
                        // If current grid == former grid (nothing was ever moved acros a grid), 
                        // you probably want to rename the var or cell, not move it.
                        if (gridLocusClicked == 0)
                        {
                            nameChangeGridN = 0;        // To update the currect grid site's textblock
                            // Nothing was ever moved across a grid boundary. The host control was just clicked: Rename!
                            openComboBoxes(xy);
                        }
                        // Whether renamed or forbidden; need to revisible the variable.
                        forbiddenSite(exContentName);
                    }

                }
                else if (elementsUnderPointer.Contains(rect1))
                {
                    if (isEmpty(1))
                    {
                        updateMovedGrid(rect1, path1, text1, gridLocusClicked, 1, N1);
                    }
                    else
                    {
                        if (gridLocusClicked == 1 && (((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusClicked] == "var" || ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusClicked] == "cell"))
                        {
                            nameChangeGridN = 1;        // To update the currect grid site's textblock
                            // Nothing was ever moved across a grid boundary. The host control was just clicked: Rename!
                            openComboBoxes(xy);
                        }

                        forbiddenSite(exContentName);
                    }

                }
                else if (elementsUnderPointer.Contains(rect2))
                {
                    if (isEmpty(2))
                    {
                        updateMovedGrid(rect2, path2, text2, gridLocusClicked, 2, N2);
                    }
                    else
                    {
                        if (gridLocusClicked == 2)
                        {
                            nameChangeGridN = 2;        // To update the currect grid site's textblock
                            // Nothing was ever moved across a grid boundary. The host control was just clicked: Rename!
                            openComboBoxes(xy);
                        }
                        forbiddenSite(exContentName);
                    }
                }
                else if (elementsUnderPointer.Contains(rect3))
                {
                    if (isEmpty(3))
                    {
                        // Default
                        updateMovedGrid(rect3, path3, text3, gridLocusClicked, 3, N3);
                    }
                    else
                    {
                        if (gridLocusClicked == 3)
                        {
                            nameChangeGridN = 3;        // To update the currect grid site's textblock
                            // Nothing was ever moved across a grid boundary. The host control was just clicked: Rename!
                            openComboBoxes(xy);
                        }
                        forbiddenSite(exContentName);
                    }
                }
                else if (elementsUnderPointer.Contains(rect4))
                {
                    if (isEmpty(4))
                    {
                        updateMovedGrid(rect4, path4, text4, gridLocusClicked, 4, N4);
                    }
                    else
                    {
                        if (gridLocusClicked == 4)
                        {
                            nameChangeGridN = 4;        // To update the currect grid site's textblock
                            // Nothing was ever moved across a grid boundary. The host control was just clicked: Rename!
                            openComboBoxes(xy);
                        }
                        forbiddenSite(exContentName);
                    }
                }
                else
                {
                    // There was no valid dropspot for the object.
                    // Do not update the drop spot's content,
                    // and re-instate the pathstyle of the original locus.
                    forbiddenSite(exContentName);
                }

                string allStorage = "";
                Debug.WriteLine("1341: Updated storage by move: ");
                foreach (string oneGridsContent in ((KeyFrames)this.KeyFrames.SelectedItem).Content)
                {
                    allStorage = allStorage + ", " + oneGridsContent;
                }
                Debug.WriteLine(allStorage);

                allStorage = "";
                Debug.WriteLine("1349: Updated varname storage by move: ");
                foreach (string oneGridsContent in ((KeyFrames)this.KeyFrames.SelectedItem).NameContent)
                {
                    allStorage = allStorage + ", " + oneGridsContent;
                }
                Debug.WriteLine(allStorage);

                src.RenderTransform = null;
            }            
        }

        // Restore previous grid visuals if nothing was moved
        private void forbiddenSite(string exContentName)
        {
            TextBlock exactTextBlock = (TextBlock)textID(gridLocusClicked);
            Path exactPath = (Path)pathID(gridLocusClicked);
            TextBox exactTextBox = (TextBox)textboxID(gridLocusClicked);
            Rectangle exactRect = (Rectangle)rectID(gridLocusClicked);

            exactPath.Style = (Style)this.Resources[styleClicked];
            exactTextBlock.Text = exContentName;
            exactRect.AllowDrop = false;
            exactTextBox.IsHitTestVisible = false;
            exactTextBox.IsReadOnly = true;

            //switch (gridLocusClicked)
            //{
            //    case 0:
            //        path0.Style = (Style)this.Resources[styleClicked];
            //        text0.Text = exContentName;
            //        rect0.AllowDrop = false;
            //        N0.IsHitTestVisible = false;
            //        break;
            //    case 1:
            //        path1.Style = (Style)this.Resources[styleClicked];
            //        text1.Text = exContentName;
            //        this.rect1.AllowDrop = false;
            //        N1.IsHitTestVisible = false;
            //        break;
            //    case 2:
            //        path2.Style = (Style)this.Resources[styleClicked];
            //        text2.Text = exContentName;
            //        rect2.AllowDrop = false;
            //        N2.IsHitTestVisible = false;
            //        break;
            //    case 3:
            //        path3.Style = (Style)this.Resources[styleClicked];
            //        text3.Text = exContentName;
            //        rect3.AllowDrop = false;
            //        N3.IsHitTestVisible = false;
            //        break;
            //    case 4:
            //        path4.Style = (Style)this.Resources[styleClicked];
            //        text4.Text = exContentName;
            //        rect4.AllowDrop = false;
            //        N4.IsHitTestVisible = false;
            //        break;
            //}
            Debug.WriteLine("Tried dropping at a forbidden site. Restoring grid " + gridLocusClicked.ToString() + "to " + styleClicked.ToString());
        }

        private bool isEmpty(int gridN)
        {
            return (((KeyFrames)this.KeyFrames.SelectedItem).Content[gridN] == null);
        }

        private bool f_isEmpty(int gridN)
        {
            return (allFormulaElements[gridN + N_rightScrollClicks].Content == null);
        }

        // Hover: not clicked down
        private void Rect_MouseEnter(object sender, MouseEventArgs e)
        {
            var src = (Rectangle)sender;
            int gridLocusEntered = Int32.Parse((string)src.Tag);       // The grid index

            // If there are no keyframes, no query is allowed.
            if (allKeyframes.Count() > 0)
            {
                // Find out if anything exists in this Keyframe, this grid
                if (((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] != null)
                {
                    src.Fill = myGrayBrush;
                }
            }
        }

        // When the mouse leaves, get the normal color back (whether the grid had content or not)
        private void Rect_MouseLeave(object sender, MouseEventArgs e)
        {
            var src = (Rectangle)sender;
            //int gridLocusEntered = Int32.Parse((string)src.Tag);       // The grid index
            
            src.Fill = myWhiteBrush;
        }

        // MouseClicked -- DOES NOT WORK --
        private void Rect_DragEnter(object sender, DragEventArgs e)
        {
            var src = (Rectangle)sender;
            int gridLocusEntered = Int32.Parse((string)src.Tag);       // The grid index

            // Find out if anything exists in this Keyframe, this grid
            if (((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] != null)
            {
                src.Fill = myBlueBrush;
            }
            Debug.WriteLine("got to rect_dragenter.");
        }

        private void Rect_DragLeave(object sender, DragEventArgs e)
        {
            var src = (Rectangle)sender;
            //int gridLocusEntered = Int32.Parse((string)src.Tag);       // The grid index

            src.Fill = myRedBrush;           // Should be original fill, irrespective of content.
            Debug.WriteLine("got to rect_dragleave.");
        }

        

        // What grid N has focus?
        int gridWithFocus; 
        private void N_GotFocus(object sender, RoutedEventArgs e)
        {
            var src = (TextBox)sender;
            gridWithFocus = Int32.Parse((string)src.Tag);
            //tempDebug.Text = "Grid number " + gridWithFocus.ToString() + " has focus.";
        }

        private void N_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Avoid the situation when the grid calls 'Text change!' just because the Keyframe changed.
            if (!textChange_by_Keyframe)
            {
                tempDebug.Text = "N_TextChanged is called";

                var src = (TextBox)sender;
                int gridLocusEntered = Int32.Parse((string)src.Tag);       // The grid index
                string newText = src.Text;
                string noWhite = newText.Trim();                            // Strip start and end whitespace
                int containedN;
                bool correctInput = true;
                Rectangle exactRect = (Rectangle)rectID(gridLocusEntered);

                if (gridWithFocus == gridLocusEntered && ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] != "var" && ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] != "cell")
                {

                    if (noWhite == null || noWhite == "")
                    {
                        // No number was entered
                        ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] = null;
                        ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridLocusEntered] = null;
                        exactRect.Fill = myWhiteBrush;
                    }
                    else if (noWhite == "=" || noWhite == "<" || noWhite == ">")
                    {
                        // Accepted non-numeric input
                        ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] = noWhite;
                        ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridLocusEntered] = null;  // Was noWhite. Fixed bug?
                        exactRect.Fill = myWhiteBrush;
                    }
                    else
                    {
                        // Number input. Test that the contained string converts successfully to an int
                        try
                        {
                            containedN = Int32.Parse(noWhite);
                        }
                        catch (FormatException)
                        {
                            //not an integer. 
                            correctInput = false;
                        }
                        catch (OverflowException)
                        {
                            //in case the number is too big/small for an int.
                            correctInput = false;
                        }
                        if (correctInput)
                        {
                            ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] = "N";
                            ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridLocusEntered] = noWhite;
                            exactRect.Fill = myWhiteBrush;
                        }
                        else
                        {
                            ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] = null;
                            ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridLocusEntered] = null;
                            exactRect.Style = (Style)this.Resources["rectError"];            // ..but when I leave, it recolors to white anyways.
                            exactRect.Fill = myRedBrush;        // Works.. Above does not.
                        }
                    }
                    Debug.WriteLine("Textbox was changed. Content is now = " + ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] + " NameContent is now = " + ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridLocusEntered]);
                    //tempDebug.Text = "Textbox " + gridLocusEntered.ToString() + " was changed. Content is now = " + ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] + " NameContent is now = " + ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridLocusEntered];
                }
            }
            else 
            {
                tempDebug.Text = "Textbox change prevented by Keyframe push.";
            }
        }
        #endregion

        // --------------------------------------
        //
        //          Simplifying code! 
        //          Retrieve the correct object in the Grid
        //
        // --------------------------------------
        #region Simplifying code to retrieve the correct clicked things (less, more versatile code is good code)
        private TextBox textboxID (int gridN)
        {
            switch(gridN)
            {
                case 0:
                    return this.N0;
                case 1:
                    return this.N1;
                case 2:
                    return this.N2;
                case 3:
                    return this.N3;
                case 4:
                    return this.N4;
            }
            return this.N0;
        }

        private Path pathID(int gridN)
        {
            switch (gridN)
            {
                case 0:
                    return this.path0;
                case 1:
                    return this.path1;
                case 2:
                    return this.path2;
                case 3:
                    return this.path3;
                case 4:
                    return this.path4;
            }
            return this.path0;
        }

        private Rectangle rectID(int gridN)
        {
            switch (gridN)
            {
                case 0:
                    return this.rect0;
                case 1:
                    return this.rect1;
                case 2:
                    return this.rect2;
                case 3:
                    return this.rect3;
                case 4:
                    return this.rect4;
            }
            return this.rect0;
        }

        private TextBlock textID(int gridN)
        {
            switch (gridN)
            {
                case 0:
                    return this.text0;
                case 1:
                    return this.text1;
                case 2:
                    return this.text2;
                case 3:
                    return this.text3;
                case 4:
                    return this.text4;
            }
            return this.text0;
        }

        // Retrieve the correct object in the FormulaGrid
        private TextBox f_textboxID(int gridN)
        {
            switch (gridN)
            {
                case 1:
                    return this.f_N1;
                case 2:
                    return this.f_N2;
                case 3:
                    return this.f_N3;
                case 4:
                    return this.f_N4;
                case 5:
                    return this.f_N5;
                case 6:
                    return this.f_N6;
                case 7:
                    return this.f_N7;
                case 8:
                    return this.f_N8;
                case 9:
                    return this.f_N9;
                case 10:
                    return this.f_N10;
            }
            return this.f_N1;
        }

        private Path f_pathID(int gridN)
        {
            switch (gridN)
            {
                case 1:
                    return this.f_path1;
                case 2:
                    return this.f_path2;
                case 3:
                    return this.f_path3;
                case 4:
                    return this.f_path4;
                case 5:
                    return this.f_path5;
                case 6:
                    return this.f_path6;
                case 7:
                    return this.f_path7;
                case 8:
                    return this.f_path8;
                case 9:
                    return this.f_path9;
                case 10:
                    return this.f_path10;
            }
            return this.f_path1;
        }

        private Rectangle f_rectID(int gridN)
        {
            switch (gridN)
            {
                case 1:
                    return this.f_rect1;
                case 2:
                    return this.f_rect2;
                case 3:
                    return this.f_rect3;
                case 4:
                    return this.f_rect4;
                case 5:
                    return this.f_rect5;
                case 6:
                    return this.f_rect6;
                case 7:
                    return this.f_rect7;
                case 8:
                    return this.f_rect8;
                case 9:
                    return this.f_rect9;
                case 10:
                    return this.f_rect10;
            }
            return this.f_rect1;
        }

        private TextBlock f_textID(int gridN)
        {
            switch (gridN)
            {
                case 1:
                    return this.f_text1;
                case 2:
                    return this.f_text2;
                case 3:
                    return this.f_text3;
                case 4:
                    return this.f_text4;
                case 5:
                    return this.f_text5;
                case 6:
                    return this.f_text6;
                case 7:
                    return this.f_text7;
                case 8:
                    return this.f_text8;
                case 9:
                    return this.f_text9;
                case 10:
                    return this.f_text10;
            }
            return this.f_text1;
        }

        // Colors
        SolidColorBrush myBlueBrush = new SolidColorBrush(Colors.Blue);
        SolidColorBrush myRedBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush myGrayBrush = new SolidColorBrush(Colors.LightGray);
        SolidColorBrush myDarkGrayBrush = new SolidColorBrush(Colors.Gray);
        SolidColorBrush myWhiteBrush = new SolidColorBrush(Colors.White);

        #endregion

        // Change opacity when entering/leaving the window (so underlying Model is easier to view)
        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            this.Opacity = 0.7;
        }
        protected override void OnMouseEnter(System.Windows.Input.MouseEventArgs e)
        {
            base.OnMouseEnter(e);
            this.Opacity = 1;
        }

        void TimeView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (timeVM != null)
            {
                //timeVM.RemoveHandler("State", OnStateChanged);
            }
            timeVM = (TimeViewModel)this.DataContext;           // Gives me access to the data in the VM.
            //timeVM.AddHandler("State", OnStateChanged);
            //this.State = timeVM.State;
            Debug.WriteLine("this.DataContext changed.");
           
        }

        void TimeView_MouseMove(object sender, MouseEventArgs e)
        {
            var mouseLocus = e.GetPosition(this);                   // this refers to the element wrt which the coordinates are reported. Here = the LTL window.     
            this.timeVM.LTLInput = mouseLocus.ToString();
        }

        //private void SuperstateDragGrid_Drop(object sender, DragEventArgs e)        // This may actually not do anything.. / Gavin
        //{
        //    var mouseLocus = e.GetPosition(this);
        //    this.timeVM.LTLInput = "The button was dragged to " + mouseLocus.ToString();
        //}

        //private void SuperstateDragGrid_Enter(object sender, DragEventArgs e)
        //{
        //    //DragDroptextBlock.FontWeight = FontWeights.ExtraBold;
        //}

        //private void SuperstateDragGrid_DragLeave(object sender, DragEventArgs e)       // This may actually not do anything.. / Gavin
        //{
        //    //DragDroptextBlock.FontWeight = FontWeights.Normal;              // x:Name and a function.
        //}

        private void Make_popup_notMove (object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;           // Stops the window dragging! Rest of functionality intact?
        }

        // -----------------------------------------------------------
        //
        //              Deleting Grid elements by RC
        //                  (Grid or Formula)
        //
        // -----------------------------------------------------------
        // Delete grid elements upon right-clicking _any_ part of the Grid element
        #region Delete Grid (Formula and/or Grid) content by RC

        // Grid erasing:
        private void rect_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var src = (Rectangle)sender;
            int gridLocusEntered = Int32.Parse((string)src.Tag);       // The grid index

            // Send off grid to be deleted:
            deleteGrid_visuals_andStorage(gridLocusEntered);
            e.Handled = true;
        }
        private void textblock_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var src = (TextBlock)sender;
            int gridLocusEntered = Int32.Parse((string)src.Tag);       // The grid index

            // Send off grid to be deleted:
            deleteGrid_visuals_andStorage(gridLocusEntered);
            e.Handled = true;
        }

        private void path_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var src = (Path)sender;
            int gridLocusEntered = Int32.Parse((string)src.Tag);       // The grid index

            // Send off grid to be deleted:
            deleteGrid_visuals_andStorage(gridLocusEntered);
            e.Handled = true;
        }

        private void textbox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var src = (TextBox)sender;
            int gridLocusEntered = Int32.Parse((string)src.Tag);       // The grid index

            // Send off grid to be deleted:
            deleteGrid_visuals_andStorage(gridLocusEntered);
            e.Handled = true;
        }

        private void deleteGrid_visuals_andStorage(int gridN)
        {
            // Just in case editing is prevented.
            //textChange_by_Keyframe = false;       // SHould be no need.

            // Delete storage
            ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridN] = null;
            ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridN] = null;

            // Delete visuals
            TextBlock exactTextBlock = (TextBlock)textID(gridN);
            Path exactPath = (Path)pathID(gridN);
            TextBox exactTextBox = (TextBox)textboxID(gridN);
            KeyFrames selectedKeyframes_storedGridItems = ((KeyFrames)this.KeyFrames.SelectedItem);
            string selectedKeyframes_name = (string)selectedKeyframes_storedGridItems.Name;

            restylePath(exactPath, gridN, null, null, exactTextBox, exactTextBlock);
        }

        // Formula erasing:
        private void f_rect_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var src = (Rectangle)sender;
            int gridLocusEntered = Int32.Parse((string)src.Tag);       // The grid index

            // Correct for scroller clicks
            int actual_storageIndex = gridLocusEntered + N_rightScrollClicks;
            
            // Send off the Formula storage index to be deleted:
            Formula_eraseGridContent(actual_storageIndex);
            e.Handled = true;
        }
        private void f_textblock_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var src = (TextBlock)sender;
            int gridLocusEntered = Int32.Parse((string)src.Tag);       // The grid index

            // Correct for scroller clicks
            int actual_storageIndex = gridLocusEntered + N_rightScrollClicks;

            // Send off the Formula storage index to be deleted:
            Formula_eraseGridContent(actual_storageIndex);
            e.Handled = true;
        }

        private void f_path_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var src = (Path)sender;
            int gridLocusEntered = Int32.Parse((string)src.Tag);       // The grid index

            // Correct for scroller clicks
            int actual_storageIndex = gridLocusEntered + N_rightScrollClicks;

            // Send off the Formula storage index to be deleted:
            Formula_eraseGridContent(actual_storageIndex);
            e.Handled = true;
        }

        private void f_textbox_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var src = (TextBox)sender;
            int gridLocusEntered = Int32.Parse((string)src.Tag);       // The grid index

            // Correct for scroller clicks
            int actual_storageIndex = gridLocusEntered + N_rightScrollClicks;

            // Send off the Formula storage index to be deleted:
            Formula_eraseGridContent(actual_storageIndex);
            e.Handled = true;
        }

        // Delete a right-clicked (or where a linked KF is erased) Formula Grid element from visuals and storage
        private void Formula_eraseGridContent(int gridN)
        {
            // NB gridN == Storage index, not the visual Formula grid index
            // Delete Formula grid content
            allFormulaElements[gridN].Content = null;
            allFormulaElements[gridN].KeyframeName = null;
            allFormulaElements[gridN].KeyframeColor = Colors.White;
            allFormulaElements[gridN].Rule = null;

            // Storage directs visual appearance, so must be offset by [- N_rightScrollClicks]
            int whatVisIndexToDraw = gridN - N_rightScrollClicks;

            // If the Formula storage's grid is in view, delete its content:
            if (whatVisIndexToDraw >= 1 && whatVisIndexToDraw <= 10)
            {
                f_pathID(whatVisIndexToDraw).Fill = new SolidColorBrush(Colors.Transparent);
                restylePath(f_pathID(whatVisIndexToDraw), whatVisIndexToDraw, null, null, f_textboxID(whatVisIndexToDraw), f_textID(whatVisIndexToDraw));
            }
        }

        #endregion

        //------------------------------------------------------------------------------
        //
        //           Evaluate drawn rules for the backend
        //
        //------------------------------------------------------------------------------
        #region Evaluate KF and Formula rules for the backend
        // Evaluate current keyframe rules
        private void Evaluate_expressions(object sender, RoutedEventArgs e)
        {
            tempDebug.Text = "Evaluating...";
            
            // Clicked to start evaluation.
            if (allKeyframes.Count > 0)
            {
                // Per KeyFrame:
                int kf_index = 0;
                int removeAmount;
                foreach (KeyFrames indivKF in allKeyframes)
                {
                    // Reset storage
                    allKeyframes[kf_index].Rule = null;

                    string tripletN = "";
                    string tripletOperator = "";
                    List<string> varNameList = new List<string>();      // Store varNames according to the collection wanted.
                    //string tripletVariable = "";

                    int contentIndex = 0;
                    int foundNonNullGrid = 0;                           // To count triplets.

                    // Per Storage Content:
                    // Check Content for non-null entries
                    foreach (string indivContent in indivKF.Content)
                    {
                        if (indivContent != null)
                        {
                            foundNonNullGrid++;

                            // Found Variable
                            if (indivContent == "var")
                            {
                                // Check the respective NameContent
                                if (indivKF.NameContent[contentIndex] == "All" || indivKF.NameContent[contentIndex] == null || indivKF.NameContent[contentIndex].Trim() == "")
                                {
                                    // Use every loaded system variable.

                                    if (allVariables.Count == 0)
                                    {
                                        // Error. No var to use in the KF rule.
                                    }
                                    else if (allVariables.Count == 1)
                                    {
                                        // One variable. Straight to Rule.
                                        if (foundNonNullGrid == 3)
                                        {
                                            // Complete triplet-set. Make a minibracket.
                                            // NB Var is last, so Not in the correct order.

                                            if (tripletOperator != "=")
                                            {
                                                // Swap the sign around.
                                                switch (tripletOperator)
                                                {
                                                    case "<":
                                                        tripletOperator = ">";
                                                        break;
                                                    case "<=":
                                                        tripletOperator = ">=";
                                                        break;
                                                    case ">":
                                                        tripletOperator = "<";
                                                        break;
                                                    case ">=":
                                                        tripletOperator = "<=";
                                                        break;
                                                }
                                            }

                                            // Send off to minibracket-machine
                                            minibracketMachine(allVariables[0].ToString(), tripletOperator, tripletN);

                                            // Reset other vars
                                            foundNonNullGrid = 1;
                                            tripletN = "";
                                            tripletOperator = "";
                                        }
                                        // Store the varname for use in minibracketMachine when I found a N / next Triplet-round
                                        varNameList.Add(allVariables[0].ToString());
                                    }
                                    else
                                    {
                                        // Multiple vars exist in the loaded system. Find each variable's name (if ex-cell) and cell.names, as listed in allVariables
                                        foreach(BMAvars indivVar in allVariables)
                                        {
                                            string[] varName = indivVar.Name.Split(' ');

                                            // Only use non-"All"-starting variable names
                                            if (varName[0] != "All")
                                            {
                                                // Use it! Unique varname that should be used in a minibracket (eventually if not yet triplet)
                                                if (foundNonNullGrid == 3)
                                                {
                                                    // Complete triplet-set. Make a minibracket.
                                                    // NB Var is last, so Not in the correct order.

                                                    if (tripletOperator != "=")
                                                    {
                                                        // Swap the sign around.
                                                        switch (tripletOperator)
                                                        {
                                                            case "<":
                                                                tripletOperator = ">";
                                                                break;
                                                            case "<=":
                                                                tripletOperator = ">=";
                                                                break;
                                                            case ">":
                                                                tripletOperator = "<";
                                                                break;
                                                            case ">=":
                                                                tripletOperator = "<=";
                                                                break;
                                                        }
                                                    }

                                                    // Send off to minibracket-machine
                                                    minibracketMachine(varName[0], tripletOperator, tripletN);

                                                    // Reset other vars
                                                    foundNonNullGrid = 1;
                                                    tripletN = "";
                                                    tripletOperator = "";
                                                }
                                                // Store the varnames for use in minibracketMachine when I found a N
                                                varNameList.Add(indivVar.Name);
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    // Could be "All varname" OR "varname"
                                    string[] splitAtSpace = indivKF.NameContent[contentIndex].Split(' ');

                                    // A collection of at least 2 non-unique variable names
                                    if (splitAtSpace[0] == "All")
                                    {
                                        // Seek out additional unique variables from other cells
                                        // In all varnames, split at '.'. If last part is the sought varname, use the full cell.varname

                                        foreach (BMAvars indivVar in allVariables)
                                        {
                                            string[] varName = indivVar.Name.Split('.');

                                            // If the varname matches the one sought, use it!
                                            if (varName[varName.Length - 1] == splitAtSpace[1])
                                            {
                                                // Use it! Should be used in a minibracket (eventually if not yet triplet)
                                                if (foundNonNullGrid == 3)
                                                {
                                                    // Complete triplet-set. Make a minibracket.
                                                    // NB Var is last, so Not in the correct order.

                                                    if (tripletOperator != "=")
                                                    {
                                                        // Swap the sign around.
                                                        switch (tripletOperator)
                                                        {
                                                            case "<":
                                                                tripletOperator = ">";
                                                                break;
                                                            case "<=":
                                                                tripletOperator = ">=";
                                                                break;
                                                            case ">":
                                                                tripletOperator = "<";
                                                                break;
                                                            case ">=":
                                                                tripletOperator = "<=";
                                                                break;
                                                        }
                                                    }

                                                    // Send off to minibracket-machine
                                                    minibracketMachine(indivVar.Name, tripletOperator, tripletN);

                                                    // Reset other vars
                                                    foundNonNullGrid = 1;
                                                    tripletN = "";
                                                    tripletOperator = "";
                                                }
                                                // Store the varnames for use in minibracketMachine when I found a N
                                                varNameList.Add(indivVar.Name);
                                            }
                                        }
                                    }
                                    else
                                    { 
                                        // Just an individual, named variable
                                        if (foundNonNullGrid == 3)
                                        {
                                            // Complete triplet-set. Make a minibracket.
                                            // NB Var is last, so Not in the correct order.

                                            if (tripletOperator != "=")
                                            {
                                                // Swap the sign around.
                                                switch (tripletOperator)
                                                {
                                                    case "<":
                                                        tripletOperator = ">";
                                                        break;
                                                    case "<=":
                                                        tripletOperator = ">=";
                                                        break;
                                                    case ">":
                                                        tripletOperator = "<";
                                                        break;
                                                    case ">=":
                                                        tripletOperator = "<=";
                                                        break;
                                                }
                                            }
                                            // Send off to minibracket-machine
                                            minibracketMachine(indivKF.NameContent[contentIndex], tripletOperator, tripletN);

                                            // Reset other vars
                                            foundNonNullGrid = 1;
                                            tripletN = "";
                                            tripletOperator = "";
                                        }
                                        // Store the varnames for use in minibracketMachine when I (found a N and) complete the (next) triplet
                                        varNameList.Add(indivKF.NameContent[contentIndex]);
                                    }
                                }
                            }
                            else if (indivContent == "cell")
                            {
                                // Find out if "All"

                                string maybeAll = "";
                                if (indivKF.NameContent[contentIndex] != null)
                                {
                                    string[] cellNameParts = indivKF.NameContent[contentIndex].Split(' ');
                                    maybeAll = cellNameParts[0];
                                }

                                if (maybeAll == "All" || indivKF.NameContent[contentIndex] == null)
                                {
                                    // Retrieve all in-cell names
                                    var modelVM = ApplicationViewModel.Instance.ActiveModel;

                                    // Add cell-contained vars:
                                    string variableName;
                                    for (int i = 0; i < modelVM.ContainerViewModels.Count; i++)
                                    {
                                        for (int j = 0; j < modelVM.ContainerViewModels[i].VariableViewModels.Count; j++)
                                        {
                                            // If the cell name is specified, use it
                                            if (modelVM.ContainerViewModels[i].Name != null && modelVM.ContainerViewModels[i].Name != "")
                                            {
                                                // Cellname.Varname
                                                variableName = modelVM.ContainerViewModels[i].Name + "." + modelVM.ContainerViewModels[i].VariableViewModels[j].Name;
                                            }
                                            else
                                            {
                                                //Varname
                                                variableName = modelVM.ContainerViewModels[i].VariableViewModels[j].Name;
                                            }
                                            varNameList.Add(variableName);
                                        }
                                    }
                                }
                                else
                                {
                                    // Retrieve all vars from a particular cell
                                    // Retrieve all in-cell names
                                    var modelVM = ApplicationViewModel.Instance.ActiveModel;

                                    // Add cell-contained vars:
                                    string variableName;
                                    for (int i = 0; i < modelVM.ContainerViewModels.Count; i++)
                                    {
                                        if (modelVM.ContainerViewModels[i].Name == indivKF.NameContent[contentIndex])
                                        {
                                            for (int j = 0; j < modelVM.ContainerViewModels[i].VariableViewModels.Count; j++)
                                            {
                                                // If the cell name is specified, use it
                                                if (modelVM.ContainerViewModels[i].Name != null && modelVM.ContainerViewModels[i].Name != "")
                                                {
                                                    // Cellname.Varname
                                                    variableName = modelVM.ContainerViewModels[i].Name + "." + modelVM.ContainerViewModels[i].VariableViewModels[j].Name;
                                                }
                                                else
                                                {
                                                    //Varname
                                                    variableName = modelVM.ContainerViewModels[i].VariableViewModels[j].Name;
                                                }
                                                varNameList.Add(variableName);
                                            }
                                        }
                                    }
                                }
                                // Just an individual, named variable
                                if (foundNonNullGrid == 3)
                                {
                                    // Complete triplet-set. Make a minibracket.
                                    // NB Var is last, so Not in the correct order.
                                    if (tripletOperator != "=")
                                    {
                                        // Swap the sign around.
                                        switch (tripletOperator)
                                        {
                                            case "<":
                                                tripletOperator = ">";
                                                break;
                                            case "<=":
                                                tripletOperator = ">=";
                                                break;
                                            case ">":
                                                tripletOperator = "<";
                                                break;
                                            case ">=":
                                                tripletOperator = "<=";
                                                break;
                                        }
                                    }
                                    // Send off to minibracket-machine
                                    foreach (string indivVarName in varNameList)
                                    {
                                        minibracketMachine(indivVarName, tripletOperator, tripletN);
                                    }

                                    // Reset other vars
                                    foundNonNullGrid = 1;
                                    tripletN = "";
                                    tripletOperator = "";
                                }
                                // NB variable names are already being saved for future use as I retrieve them, above.
                            }
                            else if (indivContent == "N")
                            {
                                // Easiest!
                                // Get the value straight away
                                tripletN = indivKF.NameContent[contentIndex];

                                if (foundNonNullGrid == 3)
                                {
                                    // Complete triplet-set. 
                                    // And N is last, so it's in the correct order!
                                    // Make a minibracket(s) based on collected varnames
                                    foreach (string idVarName in varNameList)
                                    {
                                        minibracketMachine(idVarName, tripletOperator, tripletN);
                                    }

                                    // Reset other vars
                                    foundNonNullGrid = 1;
                                    tripletOperator = "";
                                    removeAmount = Math.Max(0, varNameList.Count);
                                    varNameList.RemoveRange(0, removeAmount);
                                }
                            }
                            else
                            {
                                tripletOperator = indivKF.Content[contentIndex];
                                // Use the operator: =, <, <=, >, >=
                                // Depending on the current triplet order, it may need swapping around. Done once I hit a varName at the end of a triplet.
                                if (foundNonNullGrid == 3)
                                {
                                    tempDebug.Text = "There is a grammatical error in keyframe " + indivKF.Name + ".";
                                }
                            }
                        }
                        contentIndex++;

                        // Only do this once I'm at a new 'grid-line'  _____Implement once more grid-rows work
                        //varNameList.RemoveRange(0,varNameList.Count);
                    }

                    // MAKE THE KF's RULE!---------------------------------------------------
                    // Use stored minibrackets to create the final KF's Rule

                    if (minibracketList.Count == 0)
                    {
                        // Nothing inputted in this KF's Rulegrid
                        allKeyframes[kf_index].Rule = null;
                    }
                    else if (minibracketList.Count == 1)
                    {
                        // Only a single minibracket. Store as Rule straight away.
                        allKeyframes[kf_index].Rule = minibracketList[0];
                    }
                    else
                    {
                        // At least 2 minibrackets. Use the first two to start off the Rule EQn
                        string bigbracket = "(And " + minibracketList[0] + " " + minibracketList[1] + ")";

                        // If >2, keep editing bigbracket, else store as Rule straight away.
                        if (minibracketList.Count > 2)
                        {
                            // More minibrackets to take into account
                            for (int bracketIndex = 2; bracketIndex < minibracketList.Count; bracketIndex++)
                            {
                                bigbracket = "(And " + bigbracket + " " + minibracketList[bracketIndex] + ")";
                            }
                        }
                        allKeyframes[kf_index].Rule = bigbracket;
                    }
                    // Debug
                    tempDebug.Text = "Rule for KF " + allKeyframes[kf_index].Name + " = " + allKeyframes[kf_index].Rule;

                    // Delete minibrackets storage to reset it for the next KF
                    removeAmount = Math.Max(0, minibracketList.Count);
                    minibracketList.RemoveRange(0, removeAmount);

                    kf_index++;
                }
                // Write to the Terminal input field
                tempKF.Text = "";
                foreach (KeyFrames avail_kf in allKeyframes)
                {
                    tempKF.Text = tempKF.Text + "Rule for the keyframe \"" + avail_kf.Name + "\": \n" + avail_kf.Rule + "\n\n";
                }
            }
            else
            {
                // No keyframes need evaluating
                tempDebug.Text = "There is nothing to evaluate.";
            }
            createFormulaRule();           
        }

        private void createFormulaRule()
        {
            tempKF.Text += "\nThe final formula will be composed of the following components: ";
            // Just debug: Get all details to the Formula.
            foreach (FormulaType formulaElement in allFormulaElements)
            { 
                if (formulaElement.Content != null)
                {
                    // If a keyframe, get its rule and store it in the formula element
                    if (formulaElement.Content == "keyframe")
                    {
                        foreach (KeyFrames avail_kf in allKeyframes)
                        {
                            if (avail_kf.Name == formulaElement.KeyframeName)
                            {
                                // Poss evaluate the rule here, prior to using it. ______________
                                formulaElement.Rule = avail_kf.Rule;    // Sets the Rule!
                            }
                        }
                    }
                    else
                    {
                        // Logics. The formulaElement.Rule string below IS the logical rule.
                    }
                    tempKF.Text += "\nContent = " + formulaElement.Content + " and Rule = " + formulaElement.Rule;
                }
            }

            // Tackle the final formula
            //List<NonNull_Formula> noNullElements = new List<NonNull_Formula>();     // Create list for parsed Formula (at top, global now)
            string debugText = "";

            if (noNullElements.Count > 0)
            {
                noNullElements.RemoveRange(0, noNullElements.Count);
            }

            // Remove spaces (nulls)
            foreach (FormulaType formulaElement in allFormulaElements)
            {
                if (formulaElement.Content != null)
                {
                    noNullElements.Add(new NonNull_Formula{Content = formulaElement.Content, Rule = formulaElement.Rule});
                }
            }

            debugText += "noNullElements = \n";
            foreach (NonNull_Formula nnf in noNullElements)
            {
                debugText += nnf.Content + " " + nnf.Rule + "\n";
            }

            // Detect brackets (ob and cb)
            // "Until", "If", "Eventually", "Always", "cb", "ob" et.c.
            int formulaIndex = 0;
            int[] ob_boolArray = new int[noNullElements.Count()];
            int ob_boolArrayIndex = 0;
            int[] cb_boolArray = new int[noNullElements.Count()];
            int cb_boolArrayIndex = 0;
            
            int openingBracketHere = 0;
            int closingBracketHere = 0;
           
            foreach (NonNull_Formula noNullElement in noNullElements)
            {
                if (noNullElement.Rule == "ob")
                {
                    // Opening bracket
                    ob_boolArray[ob_boolArrayIndex] = formulaIndex; // [3,5,9]
                    ob_boolArrayIndex++;
                    debugText += "\nob at " + formulaIndex;
                }
                else if (noNullElement.Rule == "cb")
                {
                    // Closing bracket
                    cb_boolArray[cb_boolArrayIndex] = formulaIndex; // [3,5,9]
                    cb_boolArrayIndex++;
                    debugText += "\ncb at " + formulaIndex;
                }
                formulaIndex++;
            }

            debugText += "Detected " + ob_boolArrayIndex + " obs and " + cb_boolArrayIndex + " cbs.";

            // Deal with brackets (if any were detected): de-nest expressions
            if (ob_boolArrayIndex != 0)
            {
                int bracketSeparation = 5000;
                if (ob_boolArrayIndex == cb_boolArrayIndex)
                {
                    // Balanced numbers of brackets. 
                    // Pair up each cb with an ob:

                    // Do this iteratively until no brackets remain:
                    // Per closing bracket, detect the belonging opening bracket
                    for (int cb_index = 0; cb_index < cb_boolArrayIndex; cb_index++)
                    {
                        int rm_this_obIndex = 0;
                        for (int ob_index = 0; ob_index < ob_boolArrayIndex; ob_index++)
                        {
                            if (ob_boolArray[ob_index] != -1)
                            {
                                // Get smallest positive bracket separation
                                if (((cb_boolArray[cb_index] - ob_boolArray[ob_index]) > 0) && ((cb_boolArray[cb_index] - ob_boolArray[ob_index]) < bracketSeparation))
                                {
                                    // If > 0 and the smallest separation so far, set as [new] smallest separation
                                    bracketSeparation = cb_boolArray[cb_index] - ob_boolArray[ob_index];
                                    // Save the bracket pair: (ob index, cb index)
                                    openingBracketHere = ob_boolArray[ob_index];
                                    rm_this_obIndex = ob_index;
                                    closingBracketHere = cb_boolArray[cb_index];
                                }
                            }
                        }
                        bracketSeparation = 5000; // Lazy reset

                        // This bracket is necessarily un-nested.
                        // Send bracket content for bracketing (make into a function eventually)

                        // Edit the nonNullFormula from ob to cb, exclusive of brackets:
                        editNonNull_Formula(openingBracketHere + 1, closingBracketHere - 1);

                        // Bracketed content done. Rm the brackets
                        noNullElements[openingBracketHere].Content = null;
                        noNullElements[openingBracketHere].Rule = null;

                        noNullElements[closingBracketHere].Content = null;
                        noNullElements[closingBracketHere].Rule = null;

                        ob_boolArray[rm_this_obIndex] = -1; // Not used again.
                    }; // Per closing bracket
                }
                else
                { 
                    // Unbalanced N of brackets: error.
                }
            }
            // Whether no brackets to begin with or de-bracketed from above; go ahead and parse logics L to R.
            editNonNull_Formula(0, (noNullElements.Count -1));
                        
            debugText += "\n\nParsed the Formula! Per element, it is:\n";
            Debug.WriteLine("Parsed the Formula! Per element, it is:");
            for (int formIndex = 0; formIndex < noNullElements.Count; formIndex++)
            {
                debugText += formIndex + " = Content: " + noNullElements[formIndex].Content + " Rule: " + noNullElements[formIndex].Rule + "\n";
                if (noNullElements[formIndex].Rule != null)
                {
                    tempKF.Text = noNullElements[formIndex].Rule;
                    Debug.WriteLine(noNullElements[formIndex].Rule);
                }
            }
            tempDebug.Text = debugText;
            tempDebug.Text = "";
        }

        // Inputs the start and end index to edit
        private void editNonNull_Formula(int start, int end)
        {
            string mergedRule = "";
            for (int formulaIndex = start; formulaIndex <= end; formulaIndex++)
            {
                // Check what logical expressions are here:
                if (noNullElements[formulaIndex].Content == "logic")
                {
                    // Logical expressions that expect one unit afterwards:
                    if (noNullElements[formulaIndex].Rule == "Eventually" || noNullElements[formulaIndex].Rule == "Always" || noNullElements[formulaIndex].Rule == "Not")
                    {
                        // Piece together this logics expression and next non-null element (whatever it is)
                        bool foundNonNull = false;
                        for (int seekingNonNull = (formulaIndex + 1); seekingNonNull <= end; seekingNonNull++)
                        {
                            if (!foundNonNull && noNullElements[seekingNonNull].Content != null)
                            {
                                // Piece together the .Rules
                                mergedRule = "(" + noNullElements[formulaIndex].Rule + " "+ noNullElements[seekingNonNull].Rule + ")";

                                // Edit the noNullElements content to reflect new merged Rules
                                noNullElements[formulaIndex].Content = "Merged";
                                noNullElements[formulaIndex].Rule = mergedRule;

                                noNullElements[seekingNonNull].Content = null;
                                noNullElements[seekingNonNull].Rule = null;

                                foundNonNull = true;
                            }
                        }
                        if (!foundNonNull)
                        {
                            // Report error.
                        }
                    }
                    else if (noNullElements[formulaIndex].Rule == "Until" || noNullElements[formulaIndex].Rule == "Implies" || noNullElements[formulaIndex].Rule == "And")
                    {
                        // Piece together this logics expression, the previous non-null element and the next non-null element (whatever they are)

                        // Find previous:
                        bool foundPreNonNull = false;
                        for (int seekingNonNull = (formulaIndex - 1); seekingNonNull >= start; seekingNonNull--)
                        {
                            if (!foundPreNonNull && noNullElements[seekingNonNull].Content != null)
                            {
                                // Piece together the .Rules
                                mergedRule = "(" + noNullElements[formulaIndex].Rule + " " + noNullElements[seekingNonNull].Rule;

                                // Edit the noNullElements content to reflect new merged Rules
                                noNullElements[seekingNonNull].Content = null;
                                noNullElements[seekingNonNull].Rule = null;

                                foundPreNonNull = true;
                            }
                        }
                        if (foundPreNonNull)
                        {
                            // Find next:
                            bool foundPostNonNull = false;
                            for (int seekingNonNull = (formulaIndex + 1); seekingNonNull <= end; seekingNonNull++)
                            {
                                if (!foundPostNonNull && noNullElements[seekingNonNull].Content != null)
                                {
                                    // Piece together the .Rules
                                    mergedRule += " " + noNullElements[seekingNonNull].Rule + ")";

                                    // Edit the noNullElements content to reflect new merged Rules
                                    noNullElements[formulaIndex].Content = "Merged";
                                    noNullElements[formulaIndex].Rule = mergedRule;

                                    noNullElements[seekingNonNull].Content = null;
                                    noNullElements[seekingNonNull].Rule = null;

                                    foundPostNonNull = true;
                                }
                            }
                            if (!foundPostNonNull)
                            {
                                // Report error.
                            }
                        }
                    }; // Unary: Eventually, Always, Not     or    Binary: Until, Implies, And
                }; // logics element found in Formula
            }; // Per Formula element
        }


        // Where minibrackets are made, based on individual KF triplets.
        List<string> minibracketList = new List<string>();

        private void minibracketMachine(string var, string op, string N)
        {
            if (op == "=")
            {
                // Make double bracket with < and >
                if (N == "0")
                {
                    // Just one bracket needed
                    minibracketList.Add("(< " + var + " 1)");
                }
                else 
                { 
                    int Nmax = Int32.Parse(N) + 1;
                    int Nmin = Nmax -2;
                    minibracketList.Add("(And (> " + var + " " + Nmin.ToString() + ") (< " + var + " " + Nmax.ToString() + "))");
                }
            }
            else 
            {
                minibracketList.Add("(" + op + " " + var + " " + N + ")");
            }
            
            Debug.WriteLine("Test");
        }
        #endregion

        // --------------------------------------
        //
        //          The Formula Scoller
        //
        // --------------------------------------
        #region Formula scroller code
        private void ScrollLeft_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            // Change color to indicate click
            var src = (Path)sender;
            src.Fill = myDarkGrayBrush;
            src.MouseLeftButtonUp += ScrollLeft_MouseLeftButtonUp; // Explicitly cause the below function to be called, or it doesn't happen.

            // Make a new Formula element. Add it to the end.
            allFormulaElements.Add(new FormulaType { KeyframeName = null, KeyframeColor = Colors.White, Rule = null, Content = null }); // Added to the end.

            // Move contents. Start from next to last (the formula element that was just added doesn't contain anything)
            for (int formulaElementIndex = allFormulaElements.Count()-2 ; formulaElementIndex >= 0; formulaElementIndex--)
            {
                if (allFormulaElements[formulaElementIndex].Content != null)
                {
                    tempDebug.Text = "allFormulaElements index " + formulaElementIndex + " is non-null: " + allFormulaElements[formulaElementIndex].Content;
                    // Found something in the Formula grid. 
                    // Move all grid content forwards one element, where there's nothing. 
                    // Then erase the current Formula grid element's storage.
                    allFormulaElements[formulaElementIndex +1].Content = allFormulaElements[formulaElementIndex].Content;
                    allFormulaElements[formulaElementIndex].Content = null;

                    allFormulaElements[formulaElementIndex + 1].KeyframeColor = allFormulaElements[formulaElementIndex].KeyframeColor;
                    allFormulaElements[formulaElementIndex].KeyframeColor = Colors.White;

                    allFormulaElements[formulaElementIndex + 1].KeyframeName = allFormulaElements[formulaElementIndex].KeyframeName;
                    allFormulaElements[formulaElementIndex].KeyframeName = null;

                    allFormulaElements[formulaElementIndex + 1].Rule = allFormulaElements[formulaElementIndex].Rule;
                    allFormulaElements[formulaElementIndex].Rule = null;

                    // Storage directs visual appearance, so must be offset by [- N_rightScrollClicks]
                    // Edit visuals: edit current+1 -- ONLY IF 1 to 10 --
                    int vis_from_storageElement = formulaElementIndex - N_rightScrollClicks;
                    //tempDebug.Text += "\nWhere to erase = " + vis_from_storageElement + " and where to draw is that + 1. Is it within 1 to 10?";
                    if (vis_from_storageElement > 0 && vis_from_storageElement <= 10 )
                    {
                        // Edit visuals: delete current (works)
                        f_pathID(vis_from_storageElement).Fill = new SolidColorBrush(Colors.Transparent);
                        restylePath(f_pathID(vis_from_storageElement), vis_from_storageElement, null, null, f_textboxID(vis_from_storageElement), f_textID(vis_from_storageElement));
                    }
                    //tempDebug.Text += "\nDrawing if the place to draw, (" + vis_from_storageElement.ToString() + " + 1) is <= 10.";
                    if (vis_from_storageElement >= 0 && vis_from_storageElement <= 9 )
                    {
                        // Add visuals: 
                        // "storageDone" causes Formula grid editor not to try to edit Formula storage using selected keyframe data (which is irrelevant)
                        restylePath(f_pathID(vis_from_storageElement + 1), vis_from_storageElement + 1, allFormulaElements[formulaElementIndex + 1].Content, "storageDone", f_textboxID(vis_from_storageElement + 1), f_textID(vis_from_storageElement + 1));
                    }
                }
            }
        }

        private void ScrollLeft_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Change color back to gray.
            var src = (Path)sender;
            src.Fill = myGrayBrush;
            src.MouseLeftButtonUp -= ScrollLeft_MouseLeftButtonUp;
        }

        int N_rightScrollClicks = 0; // Correct storage wrt visuals
        private void ScrollRight_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            // Change color to indicate click
            var src = (Path)sender;
            src.Fill = myDarkGrayBrush;
            src.MouseLeftButtonUp += ScrollRight_MouseLeftButtonUp; // Explicitly cause the below function to be called, or it doesn't happen.
            N_rightScrollClicks++;

            //// Make a new Formula element
            var item = new FormulaType { KeyframeName = null, KeyframeColor = Colors.White, Rule = null, Content = null };
            allFormulaElements.Insert(0, item); // Add to start

            // Move storage contents. Start from 1 (the formula element that was just added to index 0 doesn't contain anything)
            for (int formulaElementIndex = 1; formulaElementIndex <= allFormulaElements.Count - 1; formulaElementIndex++)
            {
                if (allFormulaElements[formulaElementIndex].Content != null)
                {
                    tempDebug.Text += "\nallFormulaElements index " + formulaElementIndex + " is non-null: " + allFormulaElements[formulaElementIndex].Content;
                    // Found something. Move it forwards one element, where there's nothing. Then nullify the current element's storage.
                    allFormulaElements[formulaElementIndex - 1].Content = allFormulaElements[formulaElementIndex].Content;
                    allFormulaElements[formulaElementIndex].Content = null;

                    allFormulaElements[formulaElementIndex - 1].KeyframeColor = allFormulaElements[formulaElementIndex].KeyframeColor;
                    allFormulaElements[formulaElementIndex].KeyframeColor = Colors.White;

                    allFormulaElements[formulaElementIndex - 1].KeyframeName = allFormulaElements[formulaElementIndex].KeyframeName;
                    allFormulaElements[formulaElementIndex].KeyframeName = null;

                    allFormulaElements[formulaElementIndex - 1].Rule = allFormulaElements[formulaElementIndex].Rule;
                    allFormulaElements[formulaElementIndex].Rule = null;

                    // Find out which visual element to edit
                    // How storage relates to visuals:
                    int whatVisIndexToDraw = formulaElementIndex - N_rightScrollClicks;
                    //tempDebug.Text += "\nWhere to erase = " + whatVisIndexToDraw + " and where to draw is that -1. Is it within 1 to 10?";

                    // Edit visuals: edit ONLY IF 1 to 10
                    if (whatVisIndexToDraw > 0 && whatVisIndexToDraw <= 10)
                    {
                        // Edit visuals: delete current (works)
                        f_pathID(whatVisIndexToDraw).Fill = new SolidColorBrush(Colors.Transparent);
                        restylePath(f_pathID(whatVisIndexToDraw), whatVisIndexToDraw, null, null, f_textboxID(whatVisIndexToDraw), f_textID(whatVisIndexToDraw));
                    }
                    //tempDebug.Text += "\nDrawing if the place to draw, (" + whatVisIndexToDraw.ToString() + " -1) is > 1." ;
                    if (whatVisIndexToDraw > 1 && whatVisIndexToDraw <= 11)
                    {
                        // Draw new visuals
                        restylePath(f_pathID(whatVisIndexToDraw - 1), whatVisIndexToDraw - 1, allFormulaElements[formulaElementIndex - 1].Content, "storageDone", f_textboxID(whatVisIndexToDraw - 1), f_textID(whatVisIndexToDraw - 1));
                    }
                }
            }
        }

        private void ScrollRight_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Change color back to gray.
            var src = (Path)sender;
            src.Fill = myGrayBrush;
            src.MouseLeftButtonUp -= ScrollRight_MouseLeftButtonUp;
        }
        #endregion
    }
}
