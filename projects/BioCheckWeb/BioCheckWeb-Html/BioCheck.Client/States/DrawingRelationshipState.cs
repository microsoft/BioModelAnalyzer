using System.Windows.Input;
using BioCheck.ViewModel;
using BioCheck.ViewModel.Cells;
using System.Windows;
using System;

namespace BioCheck.States
{
    public class DrawingRelationshipState : UIState
    {
        private const double MinMouseMoveThreshold = 5.0; // pixels

        private RelationshipTypes type;
        private bool mouseHasMoved, mouseHasStartedMoving;
        private Point startingPosition;

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
            if (!this.mouseHasMoved)
            {
                // Crude min distance threshold. Can't get the mouse position
                // on button down, so grab position here on the first move
                // and check threshold on the second
                if (!this.mouseHasStartedMoving)
                {
                    this.startingPosition = e.GetPosition(null);
                    this.mouseHasStartedMoving = true;
                }
                else
                {
                    var currentPosition = e.GetPosition(null);
                    if (Math.Abs(currentPosition.X - this.startingPosition.X) > MinMouseMoveThreshold ||
                        Math.Abs(currentPosition.Y - this.startingPosition.Y) > MinMouseMoveThreshold)
                        this.mouseHasMoved = true;
                }
            }

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