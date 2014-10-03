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
using BioCheck.ViewModel.SCM;
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
    public partial class SCMView : UserControl
    {
        private struct VisualStates
        {
            public const string SCMStateGroup = "SCMStateGroup";            
        }

        //private SCMViewModel scmVM;// Gavin edit
        private SCMViewModel scmVM;//// = new SCMViewModel();  // Gavin edit

        public SCMView()
        {
            InitializeComponent(); // above had         public SCMView(SCMViewModel scmVM1)     before.

            this.DataContextChanged += SCMView_DataContextChanged;// Gavin edit
            //this.DataContext = this.scmVM = scmVM1;       //Gavinedit
        }

        void SCM_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (scmVM != null)
            {
                //timeVM.RemoveHandler("State", OnStateChanged);
            }
            scmVM = (SCMViewModel)this.DataContext;           // Gives me access to the data in the VM.
            //timeVM.AddHandler("State", OnStateChanged);
            //this.State = timeVM.State;

        }

        // Colors
        SolidColorBrush myBlueBrush = new SolidColorBrush(Colors.Blue);
        SolidColorBrush myRedBrush = new SolidColorBrush(Colors.Red);
        SolidColorBrush myGrayBrush = new SolidColorBrush(Colors.LightGray);
        SolidColorBrush myWhiteBrush = new SolidColorBrush(Colors.White);

        void SCMView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (scmVM != null)
            {
                //scmVM.RemoveHandler("State", OnStateChanged);
            }
            scmVM = (SCMViewModel)this.DataContext;           // Gives me access to the data in the VM.
            //scmVM.AddHandler("State", OnStateChanged);
            //this.State = scmVM.State;
           
        }

        private void Make_popup_notMove (object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;           // Stops the window dragging! Rest of functionality intact?
        }
    }
}
