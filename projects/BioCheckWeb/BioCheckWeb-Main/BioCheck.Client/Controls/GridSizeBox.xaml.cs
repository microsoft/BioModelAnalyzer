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
using BioCheck.ViewModel;
using MvvmFx.Common.Helpers;

namespace BioCheck.Controls
{
    public partial class GridSizeBox : UserControl
    {
        public GridSizeBox()
        {
            InitializeComponent();
        }

        private void OnColumnsChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Gets round a bug in the NumericUpDown that doesn't re-get the value of the property from the data source anymore.
            DispatcherHelper.DoubleBeginInvoke(() =>
                                                   {
                                                       var modelVM = ApplicationViewModel.Instance.ActiveModel;
                                                       if (modelVM != null)
                                                       {
                                                           NumColumns.Value = modelVM.Columns;
                                                       }
                                                   });
        }

        private void OnRowsChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            // Gets round a bug in the NumericUpDown that doesn't re-get the value of the property from the data source anymore.
            DispatcherHelper.DoubleBeginInvoke(() =>
            {
                var modelVM = ApplicationViewModel.Instance.ActiveModel;
                if (modelVM != null)
                {
                    NumRows.Value = modelVM.Rows;
                }
            });
        }
    }
}
