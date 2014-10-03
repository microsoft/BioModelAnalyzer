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

         /// <summary>
         /// Silverlight has a limited local storage quota which can only be
         /// extended as a direct result of a user action. There is no easy and
         /// way to determine when we've run out of storage to trigger a
         /// request for more, nor even an obvious means of determining when
         /// we're close. Instead, this routine should be called periodically
         /// <b>from a button handler</b> and, if we're getting tight, ask the
         /// user for more space. Note that the explicit user interaction is
         /// required here because quota extension must be triggered by some
         /// user action.
         /// </summary>
         public static void CheckLocalStoreQuota()
         {
             int bytesToAllowFor = 128 * 1024, bytesToRequest = 1024 * 1024;
             using (var storage = System.IO.IsolatedStorage.IsolatedStorageFile.GetUserStoreForApplication())
             {
                 if (storage.AvailableFreeSpace < bytesToAllowFor)
                 {
                     bool r = storage.IncreaseQuotaTo(storage.Quota + bytesToRequest);
                     MessageBox.Show(r ? "Quota increased" : "Increase failed");
                 }
             }
         }

         private void ButtonClick_CheckMemory(object sender, RoutedEventArgs e)
         {
             CheckLocalStoreQuota();
         }
    }
}
