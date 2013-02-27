using System.Windows;
using System.Windows.Input;
using BioCheck.Services;
using BioCheck.ViewModel;
using BioCheck.ViewModel.Cells;
using MvvmFx.Common.ExtensionMethods;

namespace BioCheck.States
{
    public class DefaultState : UIState
    {
        private Point? dragOrigin;

        protected override void OnLeftMouseDown(MouseButtonEventArgs e)
        {
            var mouseContext = e.Context();

            if (mouseContext.IsOnVariable())
            {
                mouseContext.VariableVM.IsChecked = !mouseContext.VariableVM.IsChecked;

                dragOrigin = mouseContext.Position;
            }
            else if (mouseContext.IsOnMembraneReceptor())
            {
                mouseContext.VariableVM.IsChecked = !mouseContext.VariableVM.IsChecked;
            }
            else if (mouseContext.IsOnContainerRing())
            {
                mouseContext.ContainerVM.IsChecked = !mouseContext.ContainerVM.IsChecked;

                dragOrigin = mouseContext.Position;
            }
            else
            {
                ApplicationViewModel.Instance.ActiveVariable = null;
                ApplicationViewModel.Instance.ActiveContainer = null;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (dragOrigin.HasValue)
            {
                var mouseContext = e.Context();

                if (mouseContext.IsOnVariable())
                {
                    if (mouseContext.Position.Distance(dragOrigin.Value) > 5)
                    {
                        if (this.dragDropService != null)
                        {
                            this.dragDropService.Dispose();
                        }

                        this.dragDropService = new DragDropService<VariableViewModel>(mouseContext.VariableView, dragOrigin);

                        dragOrigin = null;
                    }
                }
                else if (mouseContext.IsOnContainerRing())
                {
                    if (mouseContext.Position.Distance(dragOrigin.Value) > 5)
                    {
                        if (this.dragDropService != null)
                        {
                            this.dragDropService.Dispose();
                        }

                        this.dragDropService = new DragDropService<ContainerViewModel>(mouseContext.ContainerView, dragOrigin);

                        dragOrigin = null;
                    }
                }
            }
        }

        protected override void OnLeftMouseUp(MouseButtonEventArgs e)
        {
            dragOrigin = null;

            var mouseContext = e.Context();

            if (mouseContext.IsOnVariable() || mouseContext.IsOnMembraneReceptor())
            {
                if (mouseContext.VariableVM.IsChecked)
                {
                    ApplicationViewModel.Instance.ActiveVariable = mouseContext.VariableVM;
                }
                else
                {
                    ApplicationViewModel.Instance.ActiveVariable = null;
                }

                var toolbarVM = ApplicationViewModel.Instance.ToolbarViewModel;
                toolbarVM.MouseDownIsHandled = false;
            }
            else if (mouseContext.IsOnContainerRing())
            {
                if (mouseContext.ContainerVM.IsChecked)
                {
                    ApplicationViewModel.Instance.ActiveContainer = mouseContext.ContainerVM;
                }
                else
                {
                    ApplicationViewModel.Instance.ActiveContainer = null;
                }

                var toolbarVM = ApplicationViewModel.Instance.ToolbarViewModel;
                toolbarVM.MouseDownIsHandled = false;
            }
        }

        protected override void OnRightMouseUp(MouseButtonEventArgs e)
        {
            ApplicationViewModel.Instance.ToolbarViewModel.IsSelectionActive = true;
        }
    }
}