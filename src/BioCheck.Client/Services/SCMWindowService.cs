using System;
using System.Windows;
using System.Windows.Controls;
using BioCheck.ViewModel.Proof;
using BioCheck.ViewModel.SCM;
using BioCheck.Views;
using Microsoft.Expression.Interactivity.Layout;

// SCM edit
namespace BioCheck.Services
{
    public interface ISCMWindowService
    {
        // These MUST be implemented by a child class, i.e. below
        void Show(SCMViewModel scmVM);
        void Close();
    }

    public class SCMWindowService : ISCMWindowService
    {
        const int DefaultHeight = 350;
        const int DefaultWidth = 550;

        private readonly Canvas canvas;
        private readonly Shell shell;
        private readonly Grid layoutRoot;
        private SCMViewModel scmVM;

        private SCMView view;

        private bool isShowing;

        public SCMWindowService(Shell shell)
        {
            this.shell = shell;
            this.canvas = shell.PopupCanvas;
            this.layoutRoot = shell.LayoutRoot;
        }       

        public void Show(SCMViewModel scmVM)
        {
            if (view == null)
            {
                view = new SCMView(); // view = new SCMView(scmVM);

                // Default to the Centre of the screen
                view.HorizontalAlignment = HorizontalAlignment.Center;
                view.VerticalAlignment = VerticalAlignment.Center;

                // Absolute requirement for the popup to show (!).
                Grid.SetRow(view, 2);
                Grid.SetColumnSpan(view, 3);
                Grid.SetRowSpan(view, 4);

                view.Margin = new Thickness(10, -40, 10, 0);

                var beh = new MouseDragElementBehavior();
                beh.ConstrainToParentBounds = true;                 // Check: may prevent variable dragnDrop            
                beh.Attach(view);
            }

            if (this.scmVM != null)
            {
                this.scmVM.Dispose();              // Not entered at first startup, pre-run.
            }

            this.scmVM = scmVM;
            view.DataContext = scmVM;

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
                if (this.scmVM != null)
                {
                    this.scmVM.Dispose();
                }

                layoutRoot.Children.Remove(view);

                isShowing = false;
            }
        }
    }
}
