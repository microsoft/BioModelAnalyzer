using System.Windows.Input;
using BioCheck.Services;
using BioCheck.ViewModel;
using Microsoft.Practices.Unity;

namespace BioCheck.States
{
    /// <summary>
    /// Default initial UI state
    /// </summary>
    public abstract class UIState
    {
        private ApplicationContext context;

        protected readonly IContextBarService contextBar;
        protected readonly IRelationshipService relationshipService;
        protected IDragDropService dragDropService;

        protected UIState()
        {
            this.contextBar = ApplicationViewModel.Instance
                .Container.Resolve<IContextBarService>();

            this.relationshipService = ApplicationViewModel.Instance
                .Container.Resolve<IRelationshipService>();
        }

        protected ApplicationContext Context
        {
            get
            {
                if (this.context == null)
                {
                    this.context = ApplicationViewModel.Instance.Context;
                }
                return this.context;
            }
        }

        public void PaintVariableTool()
        {
            this.contextBar.Close();

            OnPaintVariableTool();

            Context.State = new PaintingVariableState();
        }

        public void SelectRelationshipTool()
        {
            this.contextBar.Close();

            OnSelectRelationshipTool();

            Context.State = new SelectedRelationshipState();
        }

        public void SelectionTool()
        {
            this.contextBar.Close();

            OnSelectionTool();

            Context.State = new DefaultState();
        }

        public void LeftMouseDown(MouseButtonEventArgs e)
        {
            this.contextBar.Close();

            OnLeftMouseDown(e);
        }

        public void RightMouseDown(MouseButtonEventArgs e)
        {
            this.contextBar.Close();
            e.Handled = true;

            OnRightMouseDown(e);
        }

        public void LeftMouseUp(MouseButtonEventArgs e)
        {
            OnLeftMouseUp(e);
        }

        public void RightMouseUp(MouseButtonEventArgs e)
        {
            OnRightMouseUp(e);
        }

        public void MouseMove(MouseEventArgs e)
        {
            OnMouseMove(e);
        }

        protected virtual void OnPaintVariableTool()
        {

        }

        protected virtual void OnSelectRelationshipTool()
        {

        }

        protected virtual void OnSelectionTool()
        {

        }

        protected virtual void OnLeftMouseDown(MouseButtonEventArgs e)
        {

        }

        protected virtual void OnRightMouseDown(MouseButtonEventArgs e)
        {

        }

        protected virtual void OnLeftMouseUp(MouseButtonEventArgs e)
        {

        }

        protected virtual void OnRightMouseUp(MouseButtonEventArgs e)
        {

        }

        protected virtual void OnMouseMove(MouseEventArgs e)
        {

        }
    }

    //public class DraggingState : UIState
    //{
    //    private Point? dragOrigin;

    //    protected override void OnMouseMove(MouseEventArgs e)
    //    {
    //        if (dragOrigin.HasValue)
    //        {
    //            var mouseContext = e.Context();

    //            if (mouseContext.IsOnVariable())
    //            {
    //                if (mouseContext.Position.Distance(dragOrigin.Value) > 5)
    //                {
    //                    this.dragDropService = new DragDropService<VariableViewModel>(mouseContext.VariableView, dragOrigin);

    //                    Context.State = new DraggingState();
    //                }
    //            }
    //        }
    //    }

    //    protected override void OnLeftMouseUp(MouseButtonEventArgs e)
    //    {
    //        this.dragDropService.Cancel();

    //        Context.State = new DefaultState();
    //    }
    //}




    //    var containerView = (from element in VisualTreeHelper.FindElementsInHostCoordinates(absoluteLocation, Application.Current.RootVisual)
    //                        where element is ContainerView
    //                         select element as ContainerView).FirstOrDefault();
    //    if (containerView != null)
    //    {
}
