using System;
using System.Windows;
using System.Windows.Controls;
using BioCheck.ViewModel.Proof;
using BioCheck.ViewModel.Simulation;
using BioCheck.Views;
using Microsoft.Expression.Interactivity.Layout;
using BioCheck.ViewModel;
using Microsoft.Practices.Unity;
using System.Diagnostics;

namespace BioCheck.Services
{
    public interface IGraphWindowService
    {
        void Show(GraphViewModel graphVM);

        void Close();

        void Update(Func<GraphViewModel> updateGraphFunc);
    }

    public class GraphWindowService : IGraphWindowService
    {
        const int DefaultHeight = 400;
        const int DefaultWidth = 640;

        private readonly Grid layoutRoot;
        private readonly Shell shell;

        private GraphViewModel graphVM;
        private GraphView view;

        private bool isShowing;

        public GraphWindowService(Shell shell)
        {
            this.shell = shell;
            this.layoutRoot = shell.LayoutRoot;
        }

        public void Show(GraphViewModel simVM)
        {
            if (view == null)
            {
                view = new GraphView();

                // Default to the left of the screen
                view.HorizontalAlignment = HorizontalAlignment.Left;
                view.VerticalAlignment = VerticalAlignment.Center;

                Grid.SetRow(view, 2);
                Grid.SetColumnSpan(view, 3);
                Grid.SetRowSpan(view, 4);

                var beh = new MouseDragElementBehavior();
                beh.ConstrainToParentBounds = true;
                beh.Attach(view);
            }

            if (this.graphVM != null)
            {
                this.graphVM.Dispose();
            }

            this.graphVM = simVM;
            view.DataContext = simVM;

            if (!isShowing)
            {
                var simWindow = ApplicationViewModel.Instance.Container
                                    .Resolve<ISimulationWindowService>();

                var leftMargin = 20 + simWindow.Size.Width + 10;
                if ((leftMargin + view.Width) > layoutRoot.ActualWidth)
                {
                    leftMargin = layoutRoot.ActualWidth - view.Width - 10;
                }

                view.Margin = new Thickness(leftMargin, -40, 0, 0);

                // var left = simWindow.Position.X + simWindow.Size.Width + 20;
                // var top = simWindow.Position.Y;

                layoutRoot.Children.Add(view);
                isShowing = true;
            }
        }

        public void Close()
        {
            if (isShowing)
            {
                if (this.graphVM != null)
                {
                    this.graphVM.Dispose();
                }

                layoutRoot.Children.Remove(view);

                isShowing = false;
            }
        }

        public void Update(Func<GraphViewModel> updateGraphFunc)
        {
            if (isShowing)
            {
                var graphVM = updateGraphFunc();
                Show(graphVM);
            }
        }
    }
}
