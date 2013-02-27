using System.Windows.Input;
using BioCheck.ViewModel;
using BioCheck.ViewModel.Cells;

namespace BioCheck.States
{
    public class DrawingRelationshipState : UIState
    {
        private RelationshipTypes type;
        private bool mouseHasMoved;

        public DrawingRelationshipState()
        {
            if (ApplicationViewModel.Instance.ToolbarViewModel.IsActivatorActive)
            {
                type = RelationshipTypes.Activator;
            }
            else if (ApplicationViewModel.Instance.ToolbarViewModel.IsInhibitorActive)
            {
                type = RelationshipTypes.Inhibitor;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            this.mouseHasMoved = true;

            this.relationshipService.Draw(e, false, type);
        }

        protected override void OnLeftMouseUp(MouseButtonEventArgs e)
        {
            var mouseContext = e.Context();

            this.relationshipService.CancelDrawing();

            if (mouseContext.IsOnRelationshipTarget() && this.mouseHasMoved)
            {
                e.Handled = true;

                this.relationshipService.Add(mouseContext.RelationshipTarget, type);
            }

            Context.State = new SelectedRelationshipState();
        }
    }
}