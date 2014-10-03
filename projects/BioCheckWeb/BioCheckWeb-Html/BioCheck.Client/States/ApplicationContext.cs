using System.Windows.Input;

namespace BioCheck.States
{
    public class ApplicationContext
    {
        private UIState state;

        public ApplicationContext()
        {
            this.state = new DefaultState();
        }

        public UIState State
        {
            get { return state; }
            set
            {
                if (this.state != value)
                {
                    state = value;
                }
            }
        }

        public void PaintVariableTool()
        {
            State.PaintVariableTool();
        }

        public void SelectRelationshipTool()
        {
            State.SelectRelationshipTool();
        }

        public void SelectionTool()
        {
            State.SelectionTool();
        }

        public void LeftMouseDown(MouseButtonEventArgs e)
        {
            State.LeftMouseDown(e);
        }

        public void RightMouseDown(MouseButtonEventArgs e)
        {
            State.RightMouseDown(e);
        }

        public void LeftMouseUp(MouseButtonEventArgs e)
        {
            State.LeftMouseUp(e);
        }

        public void RightMouseUp(MouseButtonEventArgs e)
        {
            State.RightMouseUp(e);
        }

        public void MouseMove(MouseEventArgs e)
        {
            State.MouseMove(e);
        }
    }
}