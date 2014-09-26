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
using MvvmFx.Common.ViewModels.States;

namespace BioCheck.Views
{
    [TemplateVisualState(Name = VisualStates.StableState, GroupName = VisualStates.ProofStateGroup)]
    [TemplateVisualState(Name = VisualStates.NotStableState, GroupName = VisualStates.ProofStateGroup)]
    [TemplateVisualState(Name = VisualStates.CounterExamplesState, GroupName = VisualStates.ProofStateGroup)]
    [TemplateVisualState(Name = VisualStates.StableByExclusionState, GroupName = VisualStates.ProofStateGroup)] // StableByExclusion

    public partial class ProofView : UserControl
    {
        private struct VisualStates
        {
            public const string ProofStateGroup = "ProofStateGroup";
            public const string StableState = "StableState";
            public const string NotStableState = "NotStableState";
            public const string CounterExamplesState = "CounterExamplesState";
            public const string StableByExclusionState = "StableByExclusionState"; // StableByExclusion
        }

        private ProofViewModel proofVM;

        public ProofView()
        {
            InitializeComponent();

            this.DataContextChanged += ProofView_DataContextChanged;
            this.ProgressionGrid.LoadingRow += ProgressionGrid_RowLoaded;
        }


        void ProofView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (proofVM != null)
            {
                proofVM.RemoveHandler("State", OnStateChanged);
            }
            proofVM = (ProofViewModel)this.DataContext;
            proofVM.AddHandler("State", OnStateChanged);
            this.State = proofVM.State;

            // Create the Progression Info grid columns
            var progressionInfo = proofVM.ProgressionInfos.FirstOrDefault();
            if (progressionInfo == null)
                return;

            while (ProgressionGrid.Columns.Count > 1)
            {
                ProgressionGrid.Columns.RemoveAt(1);
            }

            var colTemplate = this.Resources["ProgressionInfoColumnTemplate"] as DataTemplate;
            foreach (var step in progressionInfo.Steps)
            {
                var col = new DataGridTemplateColumn();
                col.IsReadOnly = true;
                col.Header = step.Name;
                col.Width = DataGridLength.Auto;
                col.CellTemplate = colTemplate;
                ProgressionGrid.Columns.Add(col);
            }
        }

        private void ProgressionGrid_RowLoaded(object sender, DataGridRowEventArgs e)
        {
            var grid = sender as DataGrid;

            int rowIndex = e.Row.GetIndex();
            var progressionInfo = proofVM.ProgressionInfos[rowIndex];

            for (int i = 0; i < progressionInfo.Steps.Count; i++)
            { 
                var column = grid.Columns[i + 1];
                var cell = column.GetCellContent(e.Row);
                cell.DataContext = progressionInfo.Steps[i];
            }
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
            this.State = proofVM.State;
        }

        /// <summary>
        /// Gets or sets the value of the <see cref="State"/> property.
        /// </summary>
        public ProofViewState State
        {
            get { return (ProofViewState)GetValue(StateProperty); }
            set { SetValue(StateProperty, value); }
        }

        /// <summary>
        /// The <see cref="StateProperty" /> dependency property registered with the 
        /// Microsoft Presentation Foundation (WPF) property system.
        /// </summary>
        public static readonly DependencyProperty StateProperty =
            DependencyProperty.Register("State", typeof(ProofViewState), typeof(ProofView), new PropertyMetadata(ProofViewState.None, OnStateChanged));

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProofView)d).OnStateChanged(e);
        }

        private void OnStateChanged(DependencyPropertyChangedEventArgs e)
        {
            switch (State)
            {
                case ProofViewState.Stable:
                    VisualStateManager.GoToState(this, VisualStates.StableState, true);
                    break;
                case ProofViewState.NotStable:
                    VisualStateManager.GoToState(this, VisualStates.NotStableState, true);
                    break;
                case ProofViewState.CounterExamples:
                    VisualStateManager.GoToState(this, VisualStates.CounterExamplesState, true);
                    break;
                case ProofViewState.StableByExclusion:
                    VisualStateManager.GoToState(this, VisualStates.StableByExclusionState, true);
                    break;
            }
        }
    }
}
