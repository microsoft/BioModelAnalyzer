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
using System.Linq;

namespace BioCheck.ViewModel.Simulation
{
    public static class GraphFactory
    {
        public static GraphViewModel Create(SimulationViewModel simulationVM)
        {
            var graphVM = new GraphViewModel();

            var variablesToGraph = from v in simulationVM.Variables
                                   where v.IsGraphed
                                   select v;

            foreach (var variable in variablesToGraph)
            {
                var line = new LineData();

                line.Name = variable.Name + " " + variable.CellName;
                line.LineColour = variable.GraphColor;

                int index = 0;
                var points = from s in variable.Steps
                             let x = ++index
                             let y = s.Value
                             select new PointData(x, y);

                line.Points.AddRange(points);

                graphVM.Lines.Add(line);
            }

            return graphVM;
        }
    }
}
