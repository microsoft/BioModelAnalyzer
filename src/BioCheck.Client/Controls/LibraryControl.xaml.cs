using System;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using BioCheck.ViewModel;

namespace BioCheck.Controls
{
    public partial class LibraryControl : UserControl
    {
        public LibraryControl()
        {
            InitializeComponent();
        }

        private void DataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CheckDoubleClick();
        }

        private TimeSpan doubleClickTime = TimeSpan.FromMilliseconds(1000);
        private DateTime lastClick = DateTime.MinValue;
        private object lastModel = null;

        private void OnRowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CheckDoubleClick();
        }

        private void CheckDoubleClick()
        {
            var selectedModel = this.ModelsGrid.SelectedItem;
            if (selectedModel == null)
                return;

            var clickTime = DateTime.Now;
            bool isDoubleClick = (selectedModel == lastModel) &&
                                 (clickTime.Subtract(this.lastClick) <= this.doubleClickTime);

            if (isDoubleClick)
            {
                ApplicationViewModel.Instance.Library.OpenCommand.Execute();
            }

            this.lastClick = clickTime;
            this.lastModel = selectedModel;
        }

        private void ModelsGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            //row is visible in grid, wire up double click event
            e.Row.MouseLeftButtonUp += OnRowMouseLeftButtonDown;
        }

        private void ModelsGrid_UnloadingRow(object sender, DataGridRowEventArgs e)
        {
            //row is no longer visible so remove double click event otherwise
            //row events will miss fire
            e.Row.MouseLeftButtonUp -= OnRowMouseLeftButtonDown;
        }

        private void TextBox_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CheckDoubleClick();
        }
    }
}
