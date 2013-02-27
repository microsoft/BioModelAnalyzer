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
using BioCheck.Helpers;
using BioCheck.ViewModel.Proof;
using BioCheck.ViewModel.Simulation;
using System.Windows.Controls.Primitives;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;

namespace BioCheck.Views
{
    public partial class SimulationView : UserControl
    {
        private struct VisualStates
        {
            public const string SimulationStateGroup = "SimulationStateGroup";
        }

        private SimulationViewModel proofVM;

        private ScrollBar scrollVariables;
        private ScrollBar scrollSteps;
        private AutomationPeer peerVariables, peerSteps;
        private bool ignoreScrollEvents;

        public SimulationView()
        {
            InitializeComponent();

            this.DataContextChanged += ProofView_DataContextChanged;
            this.ProgressionGrid.LoadingRow += ProgressionGrid_RowLoaded;
            this.Loaded += new RoutedEventHandler(SimulationView_Loaded);
        }

        void SimulationView_Loaded(object sender, RoutedEventArgs e)
        {
            if (peerVariables != null && peerSteps != null)
                return;

            Dispatcher.BeginInvoke(() =>
            {
                scrollVariables = GetVerticalScrollBar(VariablesGrid);
                if (scrollVariables != null)
                {
                    scrollVariables.Scroll += OnScrollVariablesGrid;
                }
                scrollSteps = GetVerticalScrollBar(ProgressionGrid);
                if (scrollSteps != null)
                {
                    scrollSteps.Scroll += OnScrollStepsGrid;
                }

                peerVariables = FrameworkElementAutomationPeer.CreatePeerForElement(VariablesGrid);
                peerSteps = FrameworkElementAutomationPeer.CreatePeerForElement(ProgressionGrid);
            });
        }

        private ScrollBar GetVerticalScrollBar(DataGrid parentGrid)
        {
            return parentGrid.Descendents().OfType<ScrollBar>().FirstOrDefault(sb => sb.Name == "VerticalScrollbar");
        }

        private void OnScrollStepsGrid(object sender, ScrollEventArgs e)
        {
            if (!ignoreScrollEvents)
            {
                SyncVerticalScroll(peerSteps, peerVariables);
            }
        }

        private void OnScrollVariablesGrid(object sender, ScrollEventArgs e)
        {
            if (!ignoreScrollEvents)
            {
                SyncVerticalScroll(peerVariables, peerSteps);
            }
        }

        private void SyncVerticalScroll(AutomationPeer source, AutomationPeer copy)
        {
            IScrollProvider sourceProvider = null;
            if (source != null)
            {
                sourceProvider = (IScrollProvider)source.GetPattern(PatternInterface.Scroll);
            }
            IScrollProvider copyProvider = null;
            if (copy != null)
            {
                copyProvider = (IScrollProvider)copy.GetPattern(PatternInterface.Scroll);
            }

            if (sourceProvider != null && copyProvider != null)
            {
                ignoreScrollEvents = true;

                // scroll copy at horizontal position of source, and keep vertical position
                copyProvider.SetScrollPercent(copyProvider.HorizontalScrollPercent, sourceProvider.VerticalScrollPercent);

                ignoreScrollEvents = false;
            }
        }

        void ProofView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (proofVM != null)
            {
                proofVM.RemoveHandler("NumberOfSteps", OnStepsChanged);
                proofVM.RemoveHandler("CurrentStep", OnCurrentStepChanged);
            }
            proofVM = (SimulationViewModel)this.DataContext;
            proofVM.AddHandler("NumberOfSteps", OnStepsChanged);
            proofVM.AddHandler("CurrentStep", OnCurrentStepChanged);

            LoadSteps();
        }

        private void OnStepsChanged(object sender, EventArgs e)
        {
            LoadSteps();

            //Dispatcher.BeginInvoke(() =>
            //  {
            //      peerSteps = FrameworkElementAutomationPeer.CreatePeerForElement(ProgressionGrid);
            //      var scroller = peerSteps.GetPattern(PatternInterface.Scroll) as IScrollProvider;
            //      if (scroller != null)
            //      {
            //          try
            //          {

            //              scroller.SetScrollPercent(100, scroller.VerticalScrollPercent);
            //          }
            //          catch (Exception ex)
            //          {
            //              // Swallow an exception here
            //          }
            //      }
            //  });
        }

        private void OnCurrentStepChanged(object sender, EventArgs e)
        {
            if (proofVM.CurrentStep == 0)
            {
                while (ProgressionGrid.Columns.Count > 1)
                {
                    ProgressionGrid.Columns.RemoveAt(1);
                }
            }
            else if (proofVM.CurrentStep == proofVM.NumberOfSteps)
            {
                ProgressionGrid.ItemsSource = null;

                var variableInfo = proofVM.Variables.FirstOrDefault();

                var colTemplate = this.Resources["ProgressionInfoColumnTemplate"] as DataTemplate;
                var colStyle = this.Resources["SlimColumnHeaderStyle"] as Style;

                var col = new DataGridTemplateColumn();
                col.IsReadOnly = true;
                col.Header = proofVM.CurrentStep;
                col.Width = new DataGridLength(32);
                col.CellTemplate = colTemplate;
                col.HeaderStyle = colStyle;
                ProgressionGrid.Columns.Add(col);

                ProgressionGrid.ItemsSource = proofVM.Variables;
            }
            else
            {
                var variableInfo = proofVM.Variables.FirstOrDefault();

                var colTemplate = this.Resources["ProgressionInfoColumnTemplate"] as DataTemplate;
                var colStyle = this.Resources["SlimColumnHeaderStyle"] as Style;

                var col = new DataGridTemplateColumn();
                col.IsReadOnly = true;
                col.Header = proofVM.CurrentStep;
                col.Width = new DataGridLength(32);
                col.CellTemplate = colTemplate;
                col.HeaderStyle = colStyle;
                ProgressionGrid.Columns.Add(col);
            }
        }

        private void LoadSteps()
        {
            ProgressionGrid.ItemsSource = null;

            // Create the Progression Info grid columns
            var variableInfo = proofVM.Variables.FirstOrDefault();
            if (variableInfo == null)
                return;

            while (ProgressionGrid.Columns.Count > 1)
            {
                ProgressionGrid.Columns.RemoveAt(1);
            }

            var colTemplate = this.Resources["ProgressionInfoColumnTemplate"] as DataTemplate;
            var colStyle = this.Resources["SlimColumnHeaderStyle"] as Style;

            foreach (var step in variableInfo.Steps)
            {
                var col = new DataGridTemplateColumn();
                col.IsReadOnly = true;
                col.Header = step.Name;
                col.Width = new DataGridLength(32);
                col.CellTemplate = colTemplate;
                col.HeaderStyle = colStyle;
                ProgressionGrid.Columns.Add(col);
            }

            ProgressionGrid.ItemsSource = proofVM.Variables;
        }

        private void ProgressionGrid_RowLoaded(object sender, DataGridRowEventArgs e)
        {
            var grid = sender as DataGrid;

            int rowIndex = e.Row.GetIndex();
            var variableInfo = proofVM.Variables[rowIndex];

            for (int i = 0; i < variableInfo.Steps.Count; i++)
            {
                var column = grid.Columns[i + 1];
                var cell = column.GetCellContent(e.Row);
                cell.DataContext = variableInfo.Steps[i];
            }
        }
    }
}
