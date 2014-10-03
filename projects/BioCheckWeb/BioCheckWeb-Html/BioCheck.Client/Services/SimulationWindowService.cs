using System;
using System.Windows;
using System.Windows.Controls;
using BioCheck.ViewModel.Proof;
using BioCheck.ViewModel.Simulation;
using BioCheck.Views;
using Microsoft.Expression.Interactivity.Layout;

namespace BioCheck.Services
{
    public interface ISimulationWindowService
    {
        void Show(SimulationViewModel simulationVM);

        void Close();

        Point Position { get; }

        Size Size { get; }
    }

    public class SimulationWindowService : ISimulationWindowService
    {
        const int DefaultHeight = 650;
        const int DefaultWidth = 850;

        private readonly Grid layoutRoot;
        private readonly Shell shell;

        private SimulationViewModel simulationVM;
        private SimulationView view;

        private bool isShowing;

        public SimulationWindowService(Shell shell)
        {
            this.shell = shell;
            this.layoutRoot = shell.LayoutRoot;
        }

        public Point Position
        {
            get
            {
                return new Point(Canvas.GetLeft(view), Canvas.GetTop(view));
            }
        }

        public Size Size
        {
            get
            {
                return new Size(view.ActualWidth, view.ActualHeight);
            }
        }

        public void Show(SimulationViewModel simVM)
        {
            if (view == null)
            {
                view = new SimulationView();

                // Default to the left of the screen
                view.HorizontalAlignment = HorizontalAlignment.Left;
                view.VerticalAlignment = VerticalAlignment.Center;

                Grid.SetRow(view, 2);
                Grid.SetColumnSpan(view, 3);
                Grid.SetRowSpan(view, 4);

                view.Margin = new Thickness(20, -40, 0, 0);

                var beh = new MouseDragElementBehavior();
                beh.ConstrainToParentBounds = true;
                beh.Attach(view);
            }

            if (this.simulationVM != null)
            {
                this.simulationVM.Dispose();
            }

            this.simulationVM = simVM;
            view.DataContext = simVM;

            if (!isShowing)
            {
                // Adjust the height and width if the window is too small for the default
                view.Height = Math.Min(DefaultHeight, layoutRoot.ActualHeight - 60);
                view.Width = Math.Min(DefaultWidth, layoutRoot.ActualWidth - 60);

                layoutRoot.Children.Add(view);
                
                isShowing = true;
            }
        }

        public void Close()
        {
            if (isShowing)
            {
                if (this.simulationVM != null)
                {
                    this.simulationVM.Dispose();
                }

                layoutRoot.Children.Remove(view);

                isShowing = false;
            }
        }
    }
}
