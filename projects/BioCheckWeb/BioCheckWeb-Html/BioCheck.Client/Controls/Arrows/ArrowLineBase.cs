using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BioCheck.Controls.Arrows
{

    /// <summary>

    ///     Provides a base class for ArrowLine and ArrowPolyline.

    ///     This class is abstract.

    /// </summary>

    public abstract class ArrowLineBase : Path
    {

        protected PathGeometry geometry;
        protected PathFigure figure;
        protected PolyLineSegment lineSegment;


        /// <summary>

        ///     Identifies the ArrowAngle dependency property.

        /// </summary>

        public static readonly DependencyProperty ArrowAngleProperty =

            DependencyProperty.Register("ArrowAngle",

                typeof(double), typeof(ArrowLineBase),

                new PropertyMetadata(45.0 * Math.PI / 180.0));



        /// <summary>

        ///     Gets or sets the angle between the two sides of the arrowhead.

        /// </summary>

        public double ArrowAngle
        {

            set { SetValue(ArrowAngleProperty, value); }

            get { return (double)GetValue(ArrowAngleProperty); }

        }



        /// <summary>

        ///     Identifies the ArrowLength dependency property.

        /// </summary>

        public static readonly DependencyProperty ArrowLengthProperty =

            DependencyProperty.Register("ArrowLength",

                typeof(double), typeof(ArrowLineBase),

                new PropertyMetadata(8.0));



        /// <summary>

        ///     Gets or sets the length of the two sides of the arrowhead.

        /// </summary>

        public double ArrowLength
        {

            set { SetValue(ArrowLengthProperty, value); }

            get { return (double)GetValue(ArrowLengthProperty); }

        }



        /// <summary>

        ///     Identifies the ArrowEnds dependency property.

        /// </summary>

        public static readonly DependencyProperty ArrowEndsProperty =

            DependencyProperty.Register("ArrowEnds",

                typeof(ArrowEnds), typeof(ArrowLineBase),

                new PropertyMetadata(ArrowEnds.End));



        /// <summary>

        ///     Gets or sets the property that determines which ends of the

        ///     line have arrows.

        /// </summary>

        public ArrowEnds ArrowEnds
        {

            set { SetValue(ArrowEndsProperty, value); }

            get { return (ArrowEnds)GetValue(ArrowEndsProperty); }

        }



        /// <summary>

        ///     Identifies the IsArrowClosed dependency property.

        /// </summary>

        public static readonly DependencyProperty IsArrowClosedProperty =

            DependencyProperty.Register("IsArrowClosed",

                typeof(bool), typeof(ArrowLineBase),

                new PropertyMetadata(true));



        /// <summary>
        ///     Gets or sets the property that determines if the arrow head
        ///     is closed to resemble a triangle.
        /// </summary>
        public bool IsArrowClosed
        {
            set { SetValue(IsArrowClosedProperty, value); }
            get { return (bool)GetValue(IsArrowClosedProperty); }
        }


        /// <summary>

        ///     Initializes a new instance of ArrowLineBase.

        /// </summary>

        public ArrowLineBase()
        {

            geometry = new PathGeometry();



            figure = new PathFigure();

            lineSegment = new PolyLineSegment();

            figure.Segments.Add(lineSegment);

        }



        /// <summary>

        ///     Gets a value that represents the Geometry of the ArrowLine.

        /// </summary>

        public virtual void SetData()
        {

            int count = lineSegment.Points.Count;

            if (count > 0)
            {

                // Draw the arrow at the start of the line.

                if ((ArrowEnds & ArrowEnds.Start) == ArrowEnds.Start)
                {
                    Point pt1 = figure.StartPoint;
                    Point pt2 = lineSegment.Points[0];
                    geometry.Figures.Add(CalculateArrow(pt2, pt1));
                }

                // Draw the arrow at the end of the line.
                if ((ArrowEnds & ArrowEnds.End) == ArrowEnds.End)
                {
                    Point pt1 = count == 1 ? figure.StartPoint :
                                             lineSegment.Points[count - 2];

                    Point pt2 = lineSegment.Points[count - 1];
                    geometry.Figures.Add(CalculateArrow(pt1, pt2));
                }
            }

            this.Data = geometry;
        }

        /// <summary>
        /// Calculate and return the PathFigure for an arrowhead
        /// </summary>
        /// <param name="pt1">First point in line segment</param>
        /// <param name="pt2">Second point in line segment</param>
        /// <returns>PathFigure containing arrowhead figure</returns>
        PathFigure CalculateArrow(Point pt1, Point pt2)
        {
            // Get the angle of the vector pointing from pt1 to pt2
            double baseAngle = Math2D.XAxisAngleFromCoordsDiff(pt1.X, pt1.Y, pt2.X, pt2.Y);

            if (Double.IsNaN(baseAngle))
            {
                baseAngle = 0.0;
            }

            // Adjust angle to be angle of vector from pt2 to pt1 instead
            baseAngle += Math.PI;

            Point startPoint = new Point();
            startPoint.X = pt2.X + Math2D.XCoordFromLengthsAndXAxisAngles(ArrowLength, baseAngle + ArrowAngle / 2);
            startPoint.Y = pt2.Y + Math2D.YCoordFromLengthsAndXAxisAngles(ArrowLength, baseAngle + ArrowAngle / 2);

            PolyLineSegment arrowSegment = new PolyLineSegment();
            arrowSegment.Points.Add(pt2);

            Point segPoint = new Point();
            segPoint.X = pt2.X + Math2D.XCoordFromLengthsAndXAxisAngles(ArrowLength, baseAngle - ArrowAngle / 2);
            segPoint.Y = pt2.Y + Math2D.YCoordFromLengthsAndXAxisAngles(ArrowLength, baseAngle - ArrowAngle / 2);

            arrowSegment.Points.Add(segPoint);



            PathFigure pathfig = new PathFigure();

            pathfig.StartPoint = startPoint;
            
            pathfig.Segments.Add(arrowSegment);
            pathfig.IsClosed = IsArrowClosed;

            pathfig.IsFilled = false;



            return pathfig;

        }

    }

}