using System;
using System.Windows;
using System.Windows.Controls;
using BioCheck.ViewModel.Proof;
using BioCheck.Views;
using Microsoft.Expression.Interactivity.Layout;

namespace BioCheck.Services
{
    public interface IProofWindowService
    {
        void Show(ProofViewModel proofVM);

        void Close();
    }

    public class ProofWindowService : IProofWindowService
    {
        private readonly Canvas canvas;
        private readonly Shell shell;
        private readonly Grid layoutRoot;
        private ProofViewModel proofVM;

        private ProofView view;

        private bool isShowing;

        public ProofWindowService(Shell shell)
        {
            this.shell = shell;
            this.canvas = shell.PopupCanvas;
            this.layoutRoot = shell.LayoutRoot;
        }

        public void Show(ProofViewModel proofVM)
        {
            if (view == null)
            {
                view = new ProofView();

                // Default to the right of the screen
                view.HorizontalAlignment = HorizontalAlignment.Right;
                view.VerticalAlignment = VerticalAlignment.Center;

                Grid.SetRow(view, 2);
                Grid.SetColumnSpan(view, 2);
                Grid.SetRowSpan(view, 2);

                view.Margin = new Thickness(0, -40, 20, 0);

                var beh = new MouseDragElementBehavior();
                beh.ConstrainToParentBounds = true;
                beh.Attach(view);
            }

            if (this.proofVM != null)
            {
                this.proofVM.Dispose();
            }

            this.proofVM = proofVM;
            view.DataContext = proofVM;

            if (!isShowing)
            {
                // Adjust the height if the window is too small for the default of 800
                view.Height = Math.Min(800, layoutRoot.ActualHeight - 60);

                layoutRoot.Children.Add(view);
                isShowing = true;
            }
        }

        public void Close()
        {
            if (isShowing)
            {
                if (this.proofVM != null)
                {
                    this.proofVM.Dispose();
                }

                layoutRoot.Children.Remove(view);

                isShowing = false;
            }
        }
    }
}
