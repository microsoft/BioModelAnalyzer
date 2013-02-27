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
using BioCheck.ViewModel.Proof;
using BioCheck.ViewModel.Simulation;
using MvvmFx.Common.ViewModels.States;
using System.Windows.Controls.DataVisualization.Charting;
using System.Linq;
using System.Windows.Data;

namespace BioCheck.Views
{
    public partial class GraphView : UserControl
    {
        private struct VisualStates
        {
            public const string SimulationStateGroup = "SimulationStateGroup";
        }

        private GraphViewModel proofVM;

        public GraphView()
        {
            InitializeComponent();

            this.DataContextChanged += ProofView_DataContextChanged;
        }

        const int xUnitWidth = 32;

        void ProofView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (proofVM != null)
            {
                chart.Series.Clear();
            }

            proofVM = (GraphViewModel)this.DataContext;

            if (proofVM.Lines.Count < 1)
                return;

            // Set the width of the chart, using the xUnitWidth and the number of points.
            // This allows the chart to scroll horizontally, instead of having fixing all the x intervals to the visible width of the parent control.
            var xUnits = proofVM.Lines.First().Points.Count;
            var width = (xUnits * xUnitWidth) + 40;
            chart.Width = width;

            foreach (var line in proofVM.Lines)
            {
                LineSeries columnSeries = new LineSeries();
                columnSeries.DataContext = line;
                columnSeries.TransitionDuration = TimeSpan.Zero;

                columnSeries.DependentValueBinding = new Binding("Y");
                columnSeries.IndependentValueBinding = new Binding("X");

                columnSeries.ItemsSource = line.Points;

                var pointStyle = new Style(typeof(Control));
                pointStyle.Setters.Add(new Setter(Control.BackgroundProperty, line.LineColour));
                pointStyle.Setters.Add(new Setter(Control.HeightProperty, 0));
                pointStyle.Setters.Add(new Setter(Control.WidthProperty, 0));
                columnSeries.DataPointStyle = pointStyle;

                var lineStyle = new Style(typeof(Polyline));
                lineStyle.Setters.Add(new Setter(Polyline.StrokeThicknessProperty, 5));
                columnSeries.PolylineStyle = lineStyle;

                chart.Series.Add(columnSeries);
            }
        }

        private void LegendItem_MouseEnter(object sender, MouseEventArgs e)
        {
            var line = ((FrameworkElement)sender).DataContext as LineData;

            chart.Series.Cast<LineSeries>()
                .ToList()
                .ForEach(s =>
                        s.Opacity = s.DataContext == line ? 1.0 : 0.15);
        }

        private void LegendItem_MouseLeave(object sender, MouseEventArgs e)
        {
            chart.Series.Cast<LineSeries>()
                 .ToList()
                 .ForEach(s =>
                         s.Opacity = 1.0);
        }
    }
}
