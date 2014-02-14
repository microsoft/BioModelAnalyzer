using System;
using System.Collections.Generic;
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
    public class KeyFrames
    {
        // Keyframe features
        public string Name { get; set; }            // name (set by direct typing in the box)
        public string Rule { get; set; }
        public Color Color { get; set; }
        public string[] Content { get; set; }       // "var", "cell", "N", "<", "=" or ">". Feeds into the Rule OR allows retrieval of full name from NameContent
        public string[] NameContent { get; set; }   // Stores actual cell and variable names, and ints? Feeds into the Rule
    }


    public class FormulaType
    {
        // Formula grid content
        public string[] KeyframeName { get; set; }      // null or the keyframe's name
        public string[] Rule { get; set; }              // The logical expression, or the keyframe's rule
        public Color[] KeyframeColor { get; set; }      // The color of the keyframe rectangle, or unset (Color can't be null)
        public string[] Content { get; set; }           // "logics", "keyframe" or null
    }


    public class BMAvars
    {
        public string Name { get; set; }       // name (set by popup)
    }

    public class BMAcells
    {
        public string Name { get; set; }       // name (set by popup)
    }

    public partial class TimeView : UserControl
    {
        private struct VisualStates
        {
            public const string TimeStateGroup = "TimeStateGroup";            
        }

        private TimeViewModel timeVM;
        public ObservableCollection<KeyFrames> allKeyframes;            // Now it's accessible throughout.
        public ObservableCollection<BMAvars> allVariables;
        public ObservableCollection<BMAcells> allCells;

        public string textForTempDebug = "";

        public FormulaType Formula = new FormulaType
        {
            KeyframeName =  new string[12] { null, null, null, null, null, null, null, null, null, null, null, null },
            Rule =          new string[12] { null, null, null, null, null, null, null, null, null, null, null, null },
            KeyframeColor = new Color[12],
            Content =       new string[12] { null, null, null, null, null, null, null, null, null, null, null, null }
        };

        public TimeView()
        {
            InitializeComponent();

            this.DataContextChanged += TimeView_DataContextChanged;
            this.AllowDrop = true;          // Still have not tested this.
            Debug.WriteLine("Opening TimeView.");
                        
            // ObservableCollection makes it visible.
            allKeyframes = new ObservableCollection<KeyFrames>();

            allKeyframes.Add(new KeyFrames { Name = "Initial", Content = new string[35] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }, NameContent = new string[35] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }, Rule = "(var5 < var6)", Color = Colors.Green });
            allKeyframes.Add(new KeyFrames { Name = "Cell2", Content = new string[35] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }, NameContent = new string[35] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }, Color = Colors.Blue });
            allKeyframes.Add(new KeyFrames { Name = "CellDeath", Content = new string[35] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }, NameContent = new string[35] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }, Color = Colors.Red });
            

            this.KeyFrames.ItemsSource = allKeyframes;         // KeyFrames is the XAML x:Name for the top listbox
            this.KeyFrames.SelectedItem = allKeyframes[0];    // Works!
            /// Just Debug above --------------------------------------------

            // If a model is loaded, set available variables
            if (ApplicationViewModel.Instance.HasActiveModel)
            {
                addCells2ComboBox();
                addVars2ComboBox();
            }
        }

        // -----------------------------------------------------------------------------------------------
        //
        //      Combobox events: Populate with loaded model cell and variable names, open and close
        //
        // ------------------------------------------------------------------------------------------------

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
                case "N":
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
                // Set the relevant textblock to cellName_choice
            }
        }

        string cellName_choice;
        private void dropdownCell_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var src = (ComboBox)sender;
            //cellName_choice = (string)src.SelectedValue;     // But set to default value if nothing chosen actively.
            //Debug.WriteLine(cellName_choice);

            object selectedItem = this.dropdownCell.SelectedItem;
            cellName_choice = (selectedItem == null)
                                ? string.Empty
                                : ((BMAcells)selectedItem).Name.ToString();

            Debug.WriteLine(">>>>>>>> CellName DropDown!: You chose " + cellName_choice);
            // Update the current grid's textblock to reflect it.

            TextBlock exactTextBlock = (TextBlock)textID(nameChangeGridN);
            exactTextBlock.Text = cellName_choice;
            ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[nameChangeGridN] = cellName_choice;

            //switch (nameChangeGridN)
            //{
            //    case 0:
            //        this.text0.Text = cellName_choice;
            //        ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[0] = cellName_choice;
            //        break;
            //    case 1:
            //        this.text1.Text = cellName_choice;
            //        ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[1] = cellName_choice;
            //        break;
            //    case 2:
            //        this.text2.Text = cellName_choice;
            //        ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[2] = cellName_choice;
            //        break;
            //    case 3:
            //        this.text3.Text = cellName_choice;
            //        ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[3] = cellName_choice;
            //        break;
            //    case 4:
            //        this.text4.Text = cellName_choice;
            //        ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[4] = cellName_choice;
            //        break;
            //}

            string allStorage = "";
            Debug.WriteLine("1172: Updated var/cellnames in storage by combobox selection change: ");
            foreach (string oneGridsContent in ((KeyFrames)this.KeyFrames.SelectedItem).NameContent)
            {
                allStorage = allStorage + ", " + oneGridsContent;
            }
            Debug.WriteLine(allStorage);

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

            //switch (nameChangeGridN)
            //{
            //    case 0:
            //        this.text0.Text = varName_choice;
            //        ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[0] = varName_choice;
            //        break;
            //    case 1:
            //        this.text1.Text = varName_choice;
            //        ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[1] = varName_choice;
            //        break;
            //    case 2:
            //        this.text2.Text = varName_choice;
            //        ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[2] = varName_choice;
            //        break;
            //    case 3:
            //        this.text3.Text = varName_choice;
            //        ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[3] = varName_choice;
            //        break;
            //    case 4:
            //        this.text4.Text = varName_choice;
            //        ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[4] = varName_choice;
            //        break;
            //}

            string allStorage = "";
            Debug.WriteLine("1525: Updated varnames in storage by combobox selection change: ");
            foreach (string oneGridsContent in ((KeyFrames)this.KeyFrames.SelectedItem).NameContent)
            {
                allStorage = allStorage + ", " + oneGridsContent;
            }
            Debug.WriteLine(allStorage);
        }

        // --------------------------------------
        //
        //       Keyframe listbox events
        //
        // --------------------------------------

        // What happens when I make any, or a different selection in the Listbox
        private void KeyFrames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            KeyFrames selectedKeyframe = ((KeyFrames)this.KeyFrames.SelectedItem);
            string currName = selectedKeyframe.Name;
            ((KeyFrames)this.KeyFrames.SelectedItem).Rule = "=";      // Changes the Selected item's rule?
            Debug.WriteLine("Changed Rule to " + ((KeyFrames)this.KeyFrames.SelectedItem).Rule);

            Debug.WriteLine("There are " + allKeyframes.Count.ToString() + " keyframes stored:");
            foreach (KeyFrames keyframe in allKeyframes)
            {
                Debug.WriteLine(keyframe.Name.ToString());          // I need to re-cast every time that I retrieve it! 
                // Not casting simply does not print.
            }

            KeyFrames selectedKeyframes_storedGridItems = ((KeyFrames)this.KeyFrames.SelectedItem);
            string selectedKeyframes_name = (string)selectedKeyframes_storedGridItems.Name;
            Debug.WriteLine("Trying to draw out these 5 spots for keyframe: ");
            Debug.WriteLine(selectedKeyframes_name);
            //textForTempDebug = "\n\nStored spots for new chosen keyframe " + selectedKeyframes_name + ":\n";

            // Draw according to presence/absence of string content:
            for (int x = 0; x < selectedKeyframes_storedGridItems.Content.Length; x++)
            {
                selectedKeyframes_name = selectedKeyframes_storedGridItems.Content[x];
                Debug.WriteLine(selectedKeyframes_name);

                TextBlock exactTextBlock = (TextBlock)textID(x);
                Path exactPath = (Path)pathID(x);
                TextBox exactTextBox = (TextBox)textboxID(x);

                //textForTempDebug = textForTempDebug + "\nSending " + x.ToString() + ", " + selectedKeyframes_name + " to restyleGrid_byKeyframeSelection";

                restyleGrid_byKeyframeSelection(exactPath, exactTextBlock, x, selectedKeyframes_name, exactTextBox);
            }
            tempDebug.Text = textForTempDebug;

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

        // What happens when I make any, or a different selection in the Listbox. Copy of above, just a different signature to allow it to be called.
        private void KeyFrames_SelectionChangeManually()
        {
            KeyFrames selectedKeyframe = ((KeyFrames)this.KeyFrames.SelectedItem);
            string currName = selectedKeyframe.Name;
            ((KeyFrames)this.KeyFrames.SelectedItem).Rule = "=";      // Changes the Selected item's rule?
            Debug.WriteLine("Changed Rule to " + ((KeyFrames)this.KeyFrames.SelectedItem).Rule);

            Debug.WriteLine("There are " + allKeyframes.Count.ToString() + " keyframes stored:");
            foreach (KeyFrames keyframe in allKeyframes)
            {
                Debug.WriteLine(keyframe.Name.ToString());          // I need to re-cast every time that I retrieve it! 
                // Not casting simply does not print.
            }

            KeyFrames selectedKeyframes_storedGridItems = ((KeyFrames)this.KeyFrames.SelectedItem);
            string selectedKeyframes_name = (string)selectedKeyframes_storedGridItems.Name;
            Debug.WriteLine("Trying to draw out these 5 spots for keyframe: ");
            Debug.WriteLine(selectedKeyframes_name);
            //textForTempDebug = "\n\nStored spots for new chosen keyframe " + selectedKeyframes_name + ":\n";

            // Draw according to presence/absence of string content:
            for (int x = 0; x < selectedKeyframes_storedGridItems.Content.Length; x++)
            {
                selectedKeyframes_name = selectedKeyframes_storedGridItems.Content[x];
                Debug.WriteLine(selectedKeyframes_name);

                TextBlock exactTextBlock = (TextBlock)textID(x);
                Path exactPath = (Path)pathID(x);
                TextBox exactTextBox = (TextBox)textboxID(x);

                //textForTempDebug = textForTempDebug + "\nSending " + x.ToString() + ", " + selectedKeyframes_name + " to restyleGrid_byKeyframeSelection";

                restyleGrid_byKeyframeSelection(exactPath, exactTextBlock, x, selectedKeyframes_name, exactTextBox);
            }
            tempDebug.Text = textForTempDebug;

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

        // Redraw the grid to reflect the stored content for this keyframe.
        private void restyleGrid_byKeyframeSelection(Path pathN, TextBlock textN, int gridInt, string whatContentSign, TextBox boxN)
        {   
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
                    boxN.IsHitTestVisible = false;
                    boxN.IsReadOnly = true;
                    break;
                case ">":
                    compositeResourceName = "path" + gridInt.ToString() + "mt";
                    textN.Text = "";
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
                    boxN.Focus();
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
            if (gridInt == 0 | gridInt == 1 | gridInt == 2 | gridInt == 3 | gridInt == 4)
            {
                //textForTempDebug = textForTempDebug + "Grid " + gridInt.ToString() + " gets style " + compositeResourceName + " by dropping " + whatContentSign + "\n";
            }
            Debug.WriteLine("grid " + gridInt.ToString() + " gets style " + compositeResourceName);
        }


        private Random rand = new Random();
        private void Add_keyframe(object sender, RoutedEventArgs e)
        {
            var item = new KeyFrames { Name = "New" + rand.Next(), Content = new string[35] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }, NameContent = new string[35] { null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null }, Rule = null };
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

        // --------------------------------------
        //
        //         Keyframe Drag-n-Drop
        //
        // --------------------------------------

        private void KeyFrames_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            tempDebug.Text = tempDebug.Text + "Keyframes MouseRightButtonDown";
            e.Handled = true;   // Or Silverlight shows.

            // Find what's underneath
            var src = (ListBox)sender;
            var c = src.CaptureMouse();
            operatorName = src.SelectedValue.ToString();     // x:Name of the path defines its type. Eg "Equals" or "Variable"
            string what = src.SelectedItem.ToString();
            var what2 = src.SelectedItem.GetType();

            KeyFrames selectedKeyframe = ((KeyFrames)src.SelectedItem);

            //var src2 = (KeyFrames)sender;
            //var c2 = src.CaptureMouse();
            //string boboo = src2.Name;
            //string bibi = src2.Color.ToString();

            //    KeyFrames selectedKeyframe = ((KeyFrames)this.KeyFrames.SelectedItem);
            //string currName = selectedKeyframe.Name;
            //((KeyFrames)this.KeyFrames.SelectedItem).Rule = "=";      // Changes the Selected item's rule?

            string breakpointTest = "hej";
        }

        private void KeyFrames_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            tempDebug.Text = tempDebug.Text + "Keyframes MouseRightButtonUp";
            e.Handled = true;
        }

      

        private void KeyFrames_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Keyframes MouseLeftButtonDown");
            tempDebug.Text = "Keyframes MouseLeftButtonDown";

            e.Handled = true;   // Or the window moves.
            string keyframeChosen;

            // Find what's underneath
            string isRectangle = sender.ToString();
            if (isRectangle == "System.Windows.Shapes.Rectangle")
            {
                var src = (Rectangle)sender;    // If it was a path, it would be compatible with the Operator Drag n Drop.
                var c = src.CaptureMouse();
                // Manually change selected Keyframe
                keyframeChosen = src.Tag.ToString();
                src.MouseMove += Keyframe_MouseMove;
                src.MouseLeftButtonUp += Keyframe_MouseLeftButtonUp;
            }
            else
            {
                var src = (TextBox)sender;    // If it was a path, it would be compatible with the Operator Drag n Drop.
                var c = src.CaptureMouse();
                // Manually change selected Keyframe
                keyframeChosen = src.Tag.ToString();
                src.MouseMove += Keyframe_MouseMove;
                src.MouseLeftButtonUp += Keyframe_MouseLeftButtonUp;
            }
            
            //this.KeyFrames.SelectedItem = allKeyframes[0];

            for (int keyframeIndex = 0; keyframeIndex < allKeyframes.Count(); keyframeIndex++)
            {
                if (allKeyframes[keyframeIndex].Name == keyframeChosen)
                {
                    this.KeyFrames.SelectedItem = allKeyframes[keyframeIndex];
                }
            }
            // Now load the data for this keyframe
            KeyFrames_SelectionChangeManually();

            Point offset = e.GetPosition(this);   // was LayoutRoot.

            // Enable shadow: only for the toolbar. For objects within the Target site, move those items.
            Shadow_initialLocus("Keyframe", offset);
            operatorName = "Keyframe";

        }

        private void KeyFrames_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Debug.WriteLine("Keyframes MouseLeftButtonUp");
            tempDebug.Text = tempDebug.Text + "Keyframes MouseLeftButtonUp";
        }

        // --------------------------------------
        //
        //          Mouse interactivity from the Toolbar
        //
        // --------------------------------------
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
                src.MouseLeftButtonUp += Operator_MouseLeftButtonUp;
            }
            else
            {
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
            var src = (Rectangle)sender;
            var pos = e.GetPosition(this);  

            // Move the shadow
            this.Keyframe_shadow.RenderTransform = new TranslateTransform { X = pos.X - Keyframe_shadow.Width / 2, Y = pos.Y - Keyframe_shadow.Height / 2 };
        }

        // Mouse-up from the toolbar: make the shadow disappear
        private void Keyframe_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
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
            if ((elementsUnderPointer.Contains(f_rect0)) && f_isEmpty(0))
            {
                restylePath(f_path0, 0, operatorName, whatContent, f_N0, f_text0);
                //TextBlock f_text0;
                //f_text0.Text = ((KeyFrames)this.KeyFrames.SelectedItem).Name;
            }
            else if (elementsUnderPointer.Contains(f_rect1) && f_isEmpty(1))
            {
                restylePath(f_path1, 1, operatorName, whatContent, f_N1, f_text1);
            }
            else if (elementsUnderPointer.Contains(f_rect2) && f_isEmpty(2))
            {
                restylePath(f_path2, 2, operatorName, whatContent, f_N2, f_text2);
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
            else if (elementsUnderPointer.Contains(f_rect11) && f_isEmpty(11))
            {
                restylePath(f_path11, 11, operatorName, whatContent, f_N11, f_text11);
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

        private void restylePath(Path rectN, int rectInt, string whatResourceName, string whatContentSign, TextBox boxN, TextBlock textN)
        {
            string compositeResourceName = "path" + rectInt.ToString() + whatResourceName.ToLower();
            rectN.Style = (Style)this.Resources[compositeResourceName];

            // Keyframe storage only
            if (whatContentSign == "var" || whatContentSign == "cell" || whatContentSign == "N" || whatContentSign == "<" || whatContentSign == ">" || whatContentSign == "=")
            {
                ((KeyFrames)this.KeyFrames.SelectedItem).Content[rectInt] = whatContentSign;
            }
            else
            {
                Formula.Content[rectInt] = whatContentSign;    
                if (whatResourceName == "Keyframe")
                {
                    rectN.Fill = new SolidColorBrush(((KeyFrames)this.KeyFrames.SelectedItem).Color);
                    textN.Text = ((KeyFrames)this.KeyFrames.SelectedItem).Name;
                    Formula.Rule[rectInt] = ((KeyFrames)this.KeyFrames.SelectedItem).Rule;
                    Formula.KeyframeName[rectInt] = ((KeyFrames)this.KeyFrames.SelectedItem).Name;
                    Formula.KeyframeColor[rectInt] = ((KeyFrames)this.KeyFrames.SelectedItem).Color;
                }
                else
                {
                    Formula.Rule[rectInt] = whatResourceName;   // "Until", "If", "Eventually", "Always", "cb", "ob" et.c.
                }

                // Just debug
                Debug.WriteLine("\nFormula content is now:");
                string formulaContent = "";
                for (int i = 0; i < 12; i++ )
                {
                    formulaContent = formulaContent + Formula.Content[i] + ", ";
                }
                Debug.WriteLine(formulaContent);
            }

            // Make values not enterable.
            boxN.IsHitTestVisible = false;
            boxN.IsReadOnly = true;
            rectN.AllowDrop = false;

            // Debug messages
            Debug.WriteLine("Landed on a droppable area! Using path = " + rectN.ToString() + ", rectInt = " + rectInt.ToString() + ", whatResourceName = " + whatResourceName + ", for boxN = " + boxN.ToString());
            textForTempDebug = textForTempDebug + "\n" + "Landed on a droppable area! Storing " + whatResourceName + " in Rectangle " + rectInt.ToString() + "\n";
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
            tempDebug.Text = textForTempDebug;
        }

        


        // ------------------------------------------------
        //
        //              Grid: Moving items about
        //              ..and renaming them.
        //
        // ------------------------------------------------

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

            var src = (Path)sender;

            // Is anything under the cursor?
            gridLocusClicked = Int32.Parse((string)src.Tag);       // The grid index
            if (((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusClicked] != null)
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
                    case "N":
                        shadowToEdit = Number_shadow;
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
                case "N":
                    this.Number_shadow.RenderTransform = new TranslateTransform { X = pos.X - Number_shadow.Width / 2, Y = pos.Y - Number_shadow.Height / 2 };
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
            nameChangeGridN = gridLocusEntered;

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
                        break;
                    case "var":
                        this.dropdownVar.RenderTransform = new TranslateTransform { X = xy.X - dropdownVar.Width, Y = xy.Y - dropdownVar.Height * 10 };
                        Debug.WriteLine("Put the Combobox at " + xy.X.ToString() + " and " + xy.Y.ToString());
                        this.dropdownVar.Visibility = System.Windows.Visibility.Visible;
                        break;
                    case "N":
                        break;

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
                case "N":
                    this.Number_shadow.Visibility = System.Windows.Visibility.Collapsed;
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
        }

        

        int nameChangeGridN = 0;
        private void Grid_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Default
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
                    if (gridLocusClicked == 1)
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
            return (Formula.Content[gridN] == null);
        }

        // Hover: not clicked down
        private void Rect_MouseEnter(object sender, MouseEventArgs e)
        {
            var src = (Rectangle)sender;
            int gridLocusEntered = Int32.Parse((string)src.Tag);       // The grid index

            // Find out if anything exists in this Keyframe, this grid
            if (((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] != null)
            {
                src.Fill = myGrayBrush;
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

        

        int gridWithFocus; 
        private void N_GotFocus(object sender, RoutedEventArgs e)
        {
            var src = (TextBox)sender;
            gridWithFocus = Int32.Parse((string)src.Tag);
            tempDebug.Text = "Grid number " + gridWithFocus.ToString() + " has focus.";
        }

        private void N_TextChanged(object sender, TextChangedEventArgs e)
        {
            var src = (TextBox)sender;
            int gridLocusEntered = Int32.Parse((string)src.Tag);       // The grid index
            string newText = src.Text;
            string noWhite = newText.Trim();                            // Strip start and end whitespace
            int containedN;
            bool correctInput = true;
            Rectangle exactRect = (Rectangle)rectID(gridLocusEntered);

            if (gridWithFocus == gridLocusEntered && ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] != "var" && ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] != "cell") {

                if ( noWhite == null || noWhite == "")
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
                    ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridLocusEntered] = noWhite;
                    exactRect.Fill = myWhiteBrush;
                } 
                else
                {
                    // Number input. Test that the contained string converts successfully to an int
                    try {
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
                    else {
                        ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] = null;
                        ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridLocusEntered] = null;
                        exactRect.Style = (Style)this.Resources["rectError"];            // ..but when I leave, it recolors to white anyways.
                        exactRect.Fill = myRedBrush;        // Works.. Above does not.
                    }
                }
                Debug.WriteLine("Textbox was changed. Content is now = " + ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] + " NameContent is now = " + ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridLocusEntered]);
                tempDebug.Text = "Textbox " + gridLocusEntered.ToString() + " was changed. Content is now = " + ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] + " NameContent is now = " + ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridLocusEntered];
            }
        }
        
        // Retrieve the correct object in the Grid
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

        // Colors
        SolidColorBrush myBlueBrush = new SolidColorBrush(Colors.Blue);
        SolidColorBrush myRedBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush myGrayBrush = new SolidColorBrush(Colors.LightGray);
        SolidColorBrush myWhiteBrush = new SolidColorBrush(Colors.White);

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
           
        }

        void TimeView_MouseMove(object sender, MouseEventArgs e)
        {
            var mouseLocus = e.GetPosition(this);                   // this refers to the element wrt which the coordinates are reported. Here = the LTL window.     
            this.timeVM.LTLInput = mouseLocus.ToString();

        }

        private void SuperstateDragGrid_Drop(object sender, DragEventArgs e)        // This may actually not do anything.. / Gavin
        {
            var mouseLocus = e.GetPosition(this);
            this.timeVM.LTLInput = "The button was dragged to " + mouseLocus.ToString();
        }

        private void SuperstateDragGrid_Enter(object sender, DragEventArgs e)
        {
            //DragDroptextBlock.FontWeight = FontWeights.ExtraBold;
        }

        private void SuperstateDragGrid_DragLeave(object sender, DragEventArgs e)       // This may actually not do anything.. / Gavin
        {
            //DragDroptextBlock.FontWeight = FontWeights.Normal;              // x:Name and a function.
        }

        private void Make_popup_notMove (object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;           // Stops the window dragging! Rest of functionality intact?
        }

        // Delete grid elements upon right-clicking
        private void rect_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            var src = (Rectangle)sender;
            int gridLocusEntered = Int32.Parse((string)src.Tag);       // The grid index

            // Send off grid to be deleted:
            deleteGrid_visuals_andStorage(gridLocusEntered);

            //// Delete storage
            //((KeyFrames)this.KeyFrames.SelectedItem).Content[gridLocusEntered] = null;
            //((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridLocusEntered] = null;

            //// Delete visuals
            //TextBlock exactTextBlock = (TextBlock)textID(gridLocusEntered);
            //Path exactPath = (Path)pathID(gridLocusEntered);
            //TextBox exactTextBox = (TextBox)textboxID(gridLocusEntered);
            //KeyFrames selectedKeyframes_storedGridItems = ((KeyFrames)this.KeyFrames.SelectedItem);
            //string selectedKeyframes_name = (string)selectedKeyframes_storedGridItems.Name;

            //restyleGrid_byKeyframeSelection(exactPath, exactTextBlock, gridLocusEntered, null, exactTextBox);
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
            // Delete storage
            ((KeyFrames)this.KeyFrames.SelectedItem).Content[gridN] = null;
            ((KeyFrames)this.KeyFrames.SelectedItem).NameContent[gridN] = null;

            // Delete visuals
            TextBlock exactTextBlock = (TextBlock)textID(gridN);
            Path exactPath = (Path)pathID(gridN);
            TextBox exactTextBox = (TextBox)textboxID(gridN);
            KeyFrames selectedKeyframes_storedGridItems = ((KeyFrames)this.KeyFrames.SelectedItem);
            string selectedKeyframes_name = (string)selectedKeyframes_storedGridItems.Name;

            restyleGrid_byKeyframeSelection(exactPath, exactTextBlock, gridN, null, exactTextBox);
        }

       
    }
}
