using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using MvvmFx.Common.ViewModels.Behaviors.Observable;
using System.Collections.Generic;
using BioCheck.Services;
using MvvmFx.Common.ViewModels.Commands;
using Microsoft.Practices.Unity;

namespace BioCheck.ViewModel.Simulation
{
    /// <summary>
    /// A value object used to present the data to the chart
    /// </summary>
    public class PointData
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        public PointData(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    public class LineData
    {
        private List<PointData> points;

        public LineData()
        {
            points = new List<PointData>();

        }

        public List<PointData> Points
        {
            get { return this.points; }
        }

        public string Name { get; set; }

        public Brush LineColour { get; set; }
    }

    public class GraphViewModel : ObservableViewModel
    {
        private List<LineData> lines;
        private readonly DelegateCommand closeCommand;

        public GraphViewModel()
        {
            lines = new List<LineData>();
            this.closeCommand = new DelegateCommand(OnCloseExecuted);
        }

        public List<LineData> Lines
        {
            get { return this.lines; }
        }

        public DelegateCommand CloseCommand
        {
            get { return this.closeCommand; }
        }

        private void OnCloseExecuted()
        {
            ApplicationViewModel.Instance.Container
                .Resolve<IGraphWindowService>().
                Close();
        }
    }
}
