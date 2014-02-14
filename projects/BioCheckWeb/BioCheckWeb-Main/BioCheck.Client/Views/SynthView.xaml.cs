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
using BioCheck.ViewModel.Synth;
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

namespace BioCheck.Views
{
    public partial class SynthView : UserControl
    {
        private struct VisualStates
        {
            public const string SynthStateGroup = "SynthStateGroup";            
        }

        //private SynthViewModel synthVM;// Gavin edit
        private SynthViewModel synthVM;//// = new SynthViewModel();  // Gavin edit

        public SynthView(SynthViewModel synthVM1)
        {
            InitializeComponent();

            //this.DataContextChanged += SynthView_DataContextChanged;// Gavin edit
            this.DataContext = this.synthVM = synthVM1;       //Gavinedit
            this.listXname.SelectionChanged += listXname_SelectionChanged;
            this.taggedXMLlist.SelectionChanged += taggedXMLlist_SelectionChanged;
        }
        public void taggedXMLlist_SelectionChanged(object sender, SelectionChangedEventArgs e) 
        {
            int chosenIndex = this.taggedXMLlist.SelectedIndex;
            this.synthVM.runFunWhenXMLListBoxSelChanges(chosenIndex);
        }
        // To access the selected item
        private void listXname_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int chosenXMLIndex = this.listXname.SelectedIndex;
            this.synthVM.runFunWhenListBoxSelChanges(chosenXMLIndex);
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

        void SynthView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (synthVM != null)
            {
                //synthVM.RemoveHandler("State", OnStateChanged);
            }
            synthVM = (SynthViewModel)this.DataContext;           // Gives me access to the data in the VM.
            //synthVM.AddHandler("State", OnStateChanged);
            //this.State = synthVM.State;
           
        }

        private void Make_popup_notMove (object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;           // Stops the window dragging! Rest of functionality intact?
        }

        
    }
}
