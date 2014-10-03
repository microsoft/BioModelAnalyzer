using System;
using System.Windows;
using System.Windows.Controls;
using BioCheck.ViewModel.Proof;
using BioCheck.ViewModel.Time;
using BioCheck.Views;
using Microsoft.Expression.Interactivity.Layout;

// Time edit
namespace BioCheck.Services
{
    public interface ITimeWindowService
    {
        // These MUST be implemented by a child class, i.e. below
        void Show(TimeViewModel timeVM);
        void Close();
    }

    public class TimeWindowService : ITimeWindowService
    {
        const int DefaultHeight = 650;
        const int DefaultWidth = 850;

        private readonly Canvas canvas;
        private readonly Shell shell;
        private readonly Grid layoutRoot;
        private TimeViewModel timeVM;

        private TimeView view;

        private bool isShowing;

        public TimeWindowService(Shell shell)
        {
            this.shell = shell;
            this.canvas = shell.PopupCanvas;
            this.layoutRoot = shell.LayoutRoot;
        }       

        public void Show(TimeViewModel timeVM)
        {
            if (view == null)
            {
                view = new TimeView();

                // Default to the Centre of the screen
                view.HorizontalAlignment = HorizontalAlignment.Center;
                view.VerticalAlignment = VerticalAlignment.Center;

                Grid.SetRow(view, 2);
                Grid.SetColumnSpan(view, 3);
                Grid.SetRowSpan(view, 4);

                view.Margin = new Thickness(10, -40, 10, 0);

                var beh = new MouseDragElementBehavior();
                beh.ConstrainToParentBounds = true;                 // Check: may prevent variable dragnDrop            
                beh.Attach(view);
            }

            if (this.timeVM != null)
            {
                this.timeVM.Dispose();              // Not entered at first startup, pre-run.
            }

            this.timeVM = timeVM;
            view.DataContext = timeVM;

            if (!isShowing)
            {
                // Adjust the height if the window is too small for the default of 800
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
                if (this.timeVM != null)
                {
                    this.timeVM.Dispose();
                }

                layoutRoot.Children.Remove(view);

                isShowing = false;
            }
        }
    }
}
