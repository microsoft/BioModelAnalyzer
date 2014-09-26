using System.Windows.Input;
using BioCheck.ViewModel;

namespace BioCheck.States
{
    public class SelectedRelationshipState : UIState
    {
        protected override void OnLeftMouseDown(MouseButtonEventArgs e)
        {
            var mouseContext = e.Context();

            if (mouseContext.IsOnRelationshipTarget())
            {
                e.Handled = true;
                ApplicationViewModel.Instance.ToolbarViewModel
                    .MouseDownIsHandled = true;

                this.relationshipService.StartDrawing(mouseContext.RelationshipTarget, e);

                Context.State = new DrawingRelationshipState();
            }
        }
    }
}