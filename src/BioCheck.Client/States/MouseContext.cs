using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using BioCheck.ViewModel.Cells;
using BioCheck.Views;

namespace BioCheck.States
{
    public class MouseContext
    {
        public VariableViewModel VariableVM { get; private set; }

        public VariableView VariableView { get; private set; }

        public MembranceReceptorView MembranceReceptorView { get; private set; }

        public IRelationshipTarget RelationshipTarget { get; private set; }

        public ContainerViewModel ContainerVM { get; private set; }

        public ContainerView ContainerView { get; private set; }

        public Point Position { get; private set; }

        public MouseContext(Point position)
        {
            this.Position = position;
        }

        public bool IsOnVariable()
        {
            var variableView = (from element in VisualTreeHelper.FindElementsInHostCoordinates(Position, Application.Current.RootVisual)
                                where element is VariableView
                                select element as VariableView).FirstOrDefault();
            if (variableView != null)
            {
                VariableView = variableView;
                VariableVM = variableView.DataContext as VariableViewModel;
                return true;
            }

            return false;
        }

        public bool IsOnMembraneReceptor()
        {
            var membraneView = (from element in VisualTreeHelper.FindElementsInHostCoordinates(Position, Application.Current.RootVisual)
                                where element is MembranceReceptorView
                                select element as MembranceReceptorView).FirstOrDefault();
            if (membraneView != null)
            {
                MembranceReceptorView = membraneView;
                VariableVM = membraneView.DataContext as VariableViewModel;
                return true;
            }

            return false;
        }

        public bool IsOnRelationshipTarget()
        {
            var relationshipTarget = (from element in VisualTreeHelper.FindElementsInHostCoordinates(Position, Application.Current.RootVisual)
                                      where element is IRelationshipTarget
                                      select element as IRelationshipTarget).FirstOrDefault();
            if (relationshipTarget != null)
            {
                RelationshipTarget = relationshipTarget;
                return true;
            }

            return false;
        }

        public bool IsOnContainer()
        {
            var containerView = (from element in VisualTreeHelper.FindElementsInHostCoordinates(Position, Application.Current.RootVisual)
                                 where element is ContainerView
                                 select element as ContainerView).FirstOrDefault();
            if (containerView != null)
            {
                ContainerView = containerView;
                ContainerVM = containerView.DataContext as ContainerViewModel;
                return true;
            }

            return false;
        }

        public bool IsOnContainerRing()
        {
            var containerView = (from element in VisualTreeHelper.FindElementsInHostCoordinates(Position, Application.Current.RootVisual)
                                 where element is ContainerView
                                 select element as ContainerView).FirstOrDefault();
            if (containerView != null)
            {
                var outerPath = (from element in
                         VisualTreeHelper.FindElementsInHostCoordinates(Position, Application.Current.RootVisual)
                     where element is Path
                     && (element as Path).Name == ContainerView.TemplateParts.OuterPath
                     select element as Path).FirstOrDefault();
              
                if (outerPath != null)
                {
                    ContainerView = containerView;
                    ContainerVM = containerView.DataContext as ContainerViewModel;

                    return true;
                }
            }

            return false;
        }

    }

    public static class MouseExtensions
    {
        public static MouseContext Context(this MouseButtonEventArgs e)
        {
            var context = new MouseContext(e.GetPosition(null));

            return context;
        }

        public static MouseContext Context(this MouseEventArgs e)
        {
            var context = new MouseContext(e.GetPosition(null));

            return context;
        }
    }

}