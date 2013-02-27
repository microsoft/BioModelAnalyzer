using System.Windows.Input;
using BioCheck.Views;

namespace BioCheck.States
{
    public class PaintingVariableState : UIState
    {
        public PaintingVariableState()
        {

        }

        protected override void OnLeftMouseDown(MouseButtonEventArgs e)
        {
            var mouseContext = e.Context();

            if (mouseContext.IsOnContainer())
            {
                e.Handled = true;

                var cursorPositionInContainer = e.GetPosition(mouseContext.ContainerView);
                var containerVM = mouseContext.ContainerVM;
                var variableVM = containerVM.NewVariable(cursorPositionInContainer.X - (VariableView.DefaultFixedWidth / 2) - 20, cursorPositionInContainer.Y - (VariableView.DefaultFixedHeight / 2) - 20);
            }
        }
    }
}