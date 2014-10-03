using System;
using System.Windows;
using System.Windows.Controls;
using BioCheck.ViewModel.Proof;
using BioCheck.ViewModel.Synth;
using BioCheck.Views;
using Microsoft.Expression.Interactivity.Layout;

// Synth edit
namespace BioCheck.Services
{
    public interface ISynthWindowService
    {
        // These MUST be implemented by a child class, i.e. below
        void Show(SynthViewModel synthVM);
        void Close();
    }

    public class SynthWindowService : ISynthWindowService
    {
        const int DefaultHeight = 500;
        const int DefaultWidth = 800;

        private readonly Canvas canvas;
        private readonly Shell shell;
        private readonly Grid layoutRoot;
        private SynthViewModel synthVM;

        private SynthView view;

        private bool isShowing;

        public SynthWindowService(Shell shell)
        {
            this.shell = shell;
            this.canvas = shell.PopupCanvas;
            this.layoutRoot = shell.LayoutRoot;
        }       

        public void Show(SynthViewModel synthVM)
        {
            if (view == null)
            {
                view = new SynthView(synthVM);

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

            if (this.synthVM != null)
            {
                this.synthVM.Dispose();              // Not entered at first startup, pre-run.
            }

            this.synthVM = synthVM;
            view.DataContext = synthVM;

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
                if (this.synthVM != null)
                {
                    this.synthVM.Dispose();
                }

                layoutRoot.Children.Remove(view);

                isShowing = false;
            }
        }
    }
}
