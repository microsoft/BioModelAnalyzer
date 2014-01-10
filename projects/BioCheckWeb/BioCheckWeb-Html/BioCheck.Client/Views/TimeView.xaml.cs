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

// Time edit
namespace BioCheck.Views
{
    public partial class TimeView : UserControl
    {
        private struct VisualStates
        {
            public const string TimeStateGroup = "TimeStateGroup";            
        }

        private TimeViewModel timeVM;

        public TimeView()
        {
            InitializeComponent();

            this.DataContextChanged += TimeView_DataContextChanged;
        }

        
        void TimeView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (timeVM != null)
            {
                //timeVM.RemoveHandler("State", OnStateChanged);
            }
            timeVM = (TimeViewModel)this.DataContext;
            //timeVM.AddHandler("State", OnStateChanged);
            //this.State = timeVM.State;
           
        }
    }
}
