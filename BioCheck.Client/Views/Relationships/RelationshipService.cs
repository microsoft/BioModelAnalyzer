using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using BioCheck.Helpers;
using BioCheck.ViewModel;
using BioCheck.ViewModel.Cells;
using BioCheck.Views;
using System.Linq;
using BioCheck.Controls.Arrows;

namespace BioCheck.Services
{
    /// <summary>
    /// Service for showing and closing the ContextBar
    /// </summary>
    public interface IRelationshipService
    {
        void Init(Canvas arrowCanvas, DataTemplate relationshipTemplate);

        void StartDrawing(IRelationshipTarget target, MouseButtonEventArgs e);

        void CancelDrawing();

        void Draw(MouseEventArgs e, bool includeBindings, RelationshipTypes type);

        void Add(IRelationshipTarget to, RelationshipTypes type);

        void RemoveRelationships();
        void AddRelationship(RelationshipViewModel relationshipVM);
        void RemoveRelationship(RelationshipViewModel relationshipVM);

        // TODO - move this logic to the VM layer and just add/remove them there?
        void ResetRelationships(VariableViewModel variableVM);
    }

    public class RelationshipService : IRelationshipService
    {
        protected readonly Control control;
        private Canvas arrowCanvas;
        private DataTemplate relationshipTemplate;
        private int arrowId = 0;

        private readonly static Brush arrowBrush;
        private RelationshipView arrowLine;

        private IRelationshipTarget fromTarget;
        private IRelationshipTarget toTarget;
        private Point? arrowStartPoint;

        static RelationshipService()
        {
            // #FF231F20
            //var arrowColor = Color.FromArgb(255, 35, 31, 32);
            var arrowColor = Colors.Gray; // Consistent with NormalFill defined in RelationshipView
            arrowBrush = new SolidColorBrush(arrowColor);
        }

        public RelationshipService(Shell shell)
        {
            this.control = shell.containerGrid;
        }

        public virtual void Init(Canvas arrowCanvas, DataTemplate relationshipTemplate)
        {
            this.arrowCanvas = arrowCanvas;
            this.relationshipTemplate = relationshipTemplate;
        }

        public void StartDrawing(IRelationshipTarget target, MouseButtonEventArgs e)
        {
            this.fromTarget = target;
            this.arrowStartPoint = e.GetPosition(this.arrowCanvas);

        }

        public void Draw(MouseEventArgs e, bool includeBindings, RelationshipTypes type)
        {
            var from = arrowStartPoint.Value;
            var to = e.GetPosition(this.arrowCanvas);

            DoDraw(from, to, includeBindings, type);
        }

        public void Add(IRelationshipTarget to, RelationshipTypes type)
        {
            // TODO - can move this into the bit that calls it
            this.toTarget = to;

            var modelVM = ApplicationViewModel.Instance.ActiveModel;
            var relationshipVM = modelVM.NewRelationship(fromTarget.DataContext as VariableViewModel,
                                                             toTarget.DataContext as VariableViewModel,
                                                             type);
        }

        public void CancelDrawing()
        {
            ClearArrow();
            this.arrowStartPoint = null;
            NullArrow();
        }

        #region Privates

        private void NullArrow()
        {
            this.arrowLine = null;
        }

        private void ClearArrow()
        {
            if (this.arrowLine != null)
            {
                this.arrowCanvas.Children.Remove(this.arrowLine);
            }
        }

        private RelationshipView DoDraw(Point p1, Point p2, bool includeBindings, RelationshipTypes type)
        {
            if (includeBindings)
            {
                // load data template content
                arrowLine = this.relationshipTemplate.LoadContent() as RelationshipView;
                arrowLine.Id = ++arrowId;

                InitArrow(type);
            }
            else
            {
                if (arrowLine == null)
                {
                    arrowLine = new RelationshipView(true);
                    InitArrow(type);
                    arrowLine.IsHitTestVisible = false;
                }
            }

            arrowLine.X1 = p1.X;
            arrowLine.Y1 = p1.Y;
            arrowLine.X2 = p2.X;
            arrowLine.Y2 = p2.Y;

            arrowLine.SetData();

            return arrowLine;
        }

        protected RelationshipView DrawLoopingArrow(Point arrowFrom, Point arrowTo, Point loopFrom, Point loopTo, Size loopSize, bool includeBindings, RelationshipTypes type)
        {
            if (includeBindings)
            {
                // load data template content
                arrowLine = this.relationshipTemplate.LoadContent() as RelationshipView;
                arrowLine.Id = ++arrowId;

                InitArrow(type);
            }
            else
            {
                if (arrowLine == null)
                {
                    arrowLine = new RelationshipView(true);
                    InitArrow(type);
                }
            }

            arrowLine.X1 = arrowFrom.X;
            arrowLine.Y1 = arrowFrom.Y;
            arrowLine.X2 = arrowTo.X;
            arrowLine.Y2 = arrowTo.Y;

            arrowLine.LoopSize = loopSize;
            arrowLine.LoopFrom = loopFrom;
            arrowLine.LoopTo = loopTo;

            arrowLine.SetLoopData();

            return arrowLine;
        }

        private void InitArrow(RelationshipTypes type)
        {
            arrowLine.Stroke = arrowBrush;
            arrowLine.StrokeThickness = 3;

            arrowLine.ArrowLength = 8;
            arrowLine.ArrowEnds = ArrowEnds.End;
            arrowLine.Fill = arrowBrush;

            arrowLine.Type = type;

            arrowLine.IsArrowClosed = false;

            this.arrowCanvas.Children.Add(arrowLine);
        }

        public void AddRelationship(RelationshipViewModel relationshipVM)
        {
            var fromVariableVM = relationshipVM.From;
            var toVariableVM = relationshipVM.To;

            // Get the centre points of the from and to variables
            double fromLeft = fromVariableVM.Left;
            double fromTop = fromVariableVM.Top;
            double toLeft = toVariableVM.Left;
            double toTop = toVariableVM.Top;

            // HACK - i only need this offset because currently, non-constant variables
            // have a position relative to their parent containers, which is 20 pix
            var fromOffset = 20;
            if (fromVariableVM.Type == VariableTypes.Constant)
            {
                fromOffset = 0;
            }
            fromLeft += fromOffset;
            fromTop += fromOffset;

            var toOffset = 20;
            if (toVariableVM.Type == VariableTypes.Constant)
            {
                toOffset = 0;
            }
            toLeft += toOffset;
            toTop += toOffset;

            // Get the positions around the outside
            double fromWidth = fromVariableVM.Type == VariableTypes.Constant ? 50 : 32;
            double fromHeight = fromVariableVM.Type == VariableTypes.Constant ? 50 : 32;
            double toWidth = toVariableVM.Type == VariableTypes.Constant ? 50 : 32;
            double toHeight = toVariableVM.Type == VariableTypes.Constant ? 50 : 32;

            var fromCentreX = fromLeft + (fromWidth / 2);
            var fromCentreY = fromTop + (fromHeight / 2);
            var toCentreX = toLeft + (toWidth / 2);
            var toCentreY = toTop + (toHeight / 2);

            // Adjust the x positions relative to whether the to view is centre, right, or left of the from view
            var fromX = fromCentreX;
            var toX = toCentreX;

            var fromY = fromCentreY;
            var toY = toCentreY;

            // Draw the arrow to its final rest at the centre point - 20 px of the variable
            double toMargin = toVariableVM.Type == VariableTypes.Constant ? 32 : 20;
            VectorHelper.CreateStop(toMargin, fromX, fromY, toX, toY, out toX, out toY);

            // Only start the arrow from 20px because the relationship canvas is on top of the variable one at the moment.
            VectorHelper.CreateStop(toMargin, toX, toY, fromX, fromY, out fromX, out fromY);

            if (fromVariableVM == toVariableVM)
            {
                // Draw a feedback loop
                ClearArrow();

                // Draw the arrow head and the end of the arc
                var headFromX = fromX;
                var headFromY = fromY + 22.0;
                var headToX = fromX;
                var headToY = fromY + 18;

                // Draw the loop on the canvas
                var loopFrom = new Point(fromX, fromY - 10.0);
                var loopTo = new Point(fromX, fromY + 22.0);

                Size loopSize;
                if (fromVariableVM.Type == VariableTypes.Constant)
                {
                    loopSize = new Size(22, 36);
                }
                else
                {
                    loopSize = new Size(12, 26);
                }

                var relationshipView = DrawLoopingArrow(new Point(headFromX, headFromY),
                                                        new Point(headToX, headToY),
                                                        loopFrom,
                                                        loopTo,
                                                        loopSize,
                                                        true, relationshipVM.Type);
                relationshipView.DataContext = relationshipVM;

                NullArrow();
            }
            else
            {
                ClearArrow();

                // Draw the arrow on the canvas
                var relationshipView = DoDraw(new Point(fromX, fromY), new Point(toX, toY), true, relationshipVM.Type);
                relationshipView.DataContext = relationshipVM;

                NullArrow();
            }
        }

        public void RemoveRelationships()
        {
            if (this.arrowCanvas != null)
                this.arrowCanvas.Children.Clear();
        }

        /// <summary>
        /// Removes the relationship.
        /// </summary>
        /// <param name="relationshipVM">The relationship VM.</param>
        public void RemoveRelationship(RelationshipViewModel relationshipVM)
        {
            // Remove the corresponding arrow from the canvas
            var arrowToRemove = this.arrowCanvas.Children.OfType<RelationshipView>()
                                        .FirstOrDefault(relationshipView => relationshipView.DataContext == relationshipVM);
            if (arrowToRemove != null)
            {
                this.arrowCanvas.Children.Remove(arrowToRemove);
                arrowToRemove.Dispose();
            }
        }

        /// <summary>
        /// Resets the relationship.
        /// </summary>
        /// <param name="relationshipVM">The relationship VM.</param>
        private void ResetRelationship(RelationshipViewModel relationshipVM)
        {
            this.RemoveRelationship(relationshipVM);
            this.AddRelationship(relationshipVM);
        }

        /// <summary>
        /// Resets all the relationships for this variable
        /// </summary>
        /// <param name="variableVM">The variable VM.</param>
        public void ResetRelationships(VariableViewModel variableVM)
        {
            var resetting = (from rv in this.arrowCanvas.Children.OfType<RelationshipView>()
                             let rvm = (RelationshipViewModel)rv.DataContext
                             where rvm.From == variableVM || rvm.To == variableVM
                             select new { View = rv, ViewModel = rvm }).ToList();

            while (resetting.Count > 0)
            {
                var resetter = resetting[0];

                this.arrowCanvas.Children.Remove(resetter.View);
                resetter.View.Dispose();

                this.AddRelationship(resetter.ViewModel);
                resetting.Remove(resetter);
            }
        }

        #endregion
    }
}
