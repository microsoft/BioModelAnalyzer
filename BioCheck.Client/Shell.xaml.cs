using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.ComponentModel;
using BioCheck.Controls;
using BioCheck.Services;
using BioCheck.ViewModel;
using MvvmFx.Common.Helpers;
using Microsoft.Practices.Unity;

namespace BioCheck
{
    public partial class Shell : UserControl
    {
        private BusyWindow loadingWindow;

        public Shell()
        {
            // Required to initialize variables
            InitializeComponent();

            this.Loaded += Shell2_Loaded;

            this.DataContext = ApplicationViewModel.Instance;
        }

        void Shell2_Loaded(object sender, RoutedEventArgs e)
        {
            loadingWindow = new BusyWindow();
            loadingWindow.Closed += (s, args) => Application.Current.RootVisual.SetValue(Control.IsEnabledProperty, true);
            loadingWindow.BusyText = "Loading model...";
            loadingWindow.Show();

            ApplicationViewModel.Instance.AddHandler("IsLoading", OnIsLoadingChanged);

            containerGrid.Focus();

            DispatcherHelper.BeginInvoke(ApplicationViewModel.Instance.Load);
        }

        private void OnIsLoadingChanged(object sender, EventArgs e)
        {
            if (!ApplicationViewModel.Instance.IsLoading)
            {
                loadingWindow.Close();
            }
        }

         public void ShowProof(string modelXml)
         {
             
         }
    }
}
