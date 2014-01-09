
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BioCheck.Controls.Arrows
{

    /// <summary>
    ///     Draws a straight line between two points with
    ///     optional arrows on the ends.
    /// </summary>
    public class LoopingArrowLine : ArrowLineBase
    {
        public LoopingArrowLine()
        {
           // SetData();
        }

        /// <summary>
        ///     Identifies the X1 dependency property.
        /// </summary>
        public static readonly DependencyProperty X1Property =
            DependencyProperty.Register("X1",
                typeof(double), typeof(LoopingArrowLine),
                new PropertyMetadata(0.0));

        /// <summary>
        ///     Gets or sets the x-coordinate of the LoopingArrowLine start point.
        /// </summary>
        public double X1
        {
            set { SetValue(X1Property, value); }
            get { return (double)GetValue(X1Property); }
        }


        /// <summary>

        ///     Identifies the Y1 dependency property.

        /// </summary>

        public static readonly DependencyProperty Y1Property =

            DependencyProperty.Register("Y1",

                typeof(double), typeof(LoopingArrowLine),

                new PropertyMetadata(0.0));



        /// <summary>

        ///     Gets or sets the y-coordinate of the LoopingArrowLine start point.

        /// </summary>

        public double Y1
        {

            set { SetValue(Y1Property, value); }

            get { return (double)GetValue(Y1Property); }

        }



        /// <summary>

        ///     Identifies the X2 dependency property.

        /// </summary>

        public static readonly DependencyProperty X2Property =

            DependencyProperty.Register("X2",

                typeof(double), typeof(LoopingArrowLine),

                new PropertyMetadata(0.0));



        /// <summary>

        ///     Gets or sets the x-coordinate of the LoopingArrowLine end point.

        /// </summary>

        public double X2
        {

            set { SetValue(X2Property, value); }

            get { return (double)GetValue(X2Property); }

        }



        /// <summary>
        ///     Identifies the Y2 dependency property.
        /// </summary>
        public static readonly DependencyProperty Y2Property =
            DependencyProperty.Register("Y2",
                typeof(double), typeof(LoopingArrowLine),
                new PropertyMetadata(0.0));

        /// <summary>
        ///     Gets or sets the y-coordinate of the LoopingArrowLine end point.
        /// </summary>
        public double Y2
        {
            set { SetValue(Y2Property, value); }
            get { return (double)GetValue(Y2Property); }
        }

        /// <summary>
        ///     Identifies the LoopSize dependency property.
        /// </summary>
        public static readonly DependencyProperty LoopSizeProperty =
            DependencyProperty.Register("LoopSize",
                typeof(Size), typeof(LoopingArrowLine),
                new PropertyMetadata(null));

        /// <summary>
        ///     Gets or sets the LoopSize of the LoopingArrowLine end point.
        /// </summary>
        public Size LoopSize
        {
            set { SetValue(LoopSizeProperty, value); }
            get { return (Size)GetValue(LoopSizeProperty); }
        }

        /// <summary>
        ///     Identifies the LoopFrom dependency property.
        /// </summary>
        public static readonly DependencyProperty LoopFromProperty =
            DependencyProperty.Register("LoopFrom",
                typeof(Point), typeof(LoopingArrowLine),
                new PropertyMetadata(null));

        /// <summary>
        ///     Gets or sets the x-coordinate of the LoopingArrowLine start point.
        /// </summary>
        public Point LoopFrom
        {
            set { SetValue(LoopFromProperty, value); }
            get { return (Point)GetValue(LoopFromProperty); }
        }


        /// <summary>
        ///     Identifies the LoopTo dependency property.
        /// </summary>
        public static readonly DependencyProperty LoopToProperty =
            DependencyProperty.Register("LoopTo",
                typeof(Point), typeof(LoopingArrowLine),
                new PropertyMetadata(null));

        /// <summary>
        ///     Gets or sets the x-coordinate of the LoopingArrowLine start point.
        /// </summary>
        public Point LoopTo
        {
            set { SetValue(LoopToProperty, value); }
            get { return (Point)GetValue(LoopToProperty); }
        }

        /// <summary>
        ///     Gets a value that represents the Geometry of the LoopingArrowLine.
        /// </summary>
        public override void SetData()
        {
            // Clear out the PathGeometry.

            geometry.Figures.Clear();

            // Define a single PathFigure with the points.
            figure.StartPoint = new Point(X1, Y1);
            lineSegment.Points.Clear();
            lineSegment.Points.Add(new Point(X2, Y2));
            geometry.Figures.Add(figure);

            // Call the base method to add arrows on the ends.
            base.SetData();
        }

        /// <summary>
        ///     Gets a value that represents the Geometry of the LoopingArrowLine.
        /// </summary>
        public void SetLoopData()
        {
            // Clear out the PathGeometry.

            geometry.Figures.Clear();

            // Define a single PathFigure with the points.
            figure.StartPoint = new Point(X1, Y1);
            lineSegment.Points.Clear();
            lineSegment.Points.Add(new Point(X2, Y2));
            geometry.Figures.Add(figure);

            // Add the loop
            var arc = new ArcSegment();
            arc.Size = LoopSize;
            arc.SweepDirection = SweepDirection.Clockwise;
            arc.IsLargeArc = true;
            arc.RotationAngle = 0;
            arc.Point = LoopTo;

            var loopFigure = new PathFigure();
            loopFigure.StartPoint = LoopFrom;

            // Add the arc to the geometry
            loopFigure.Segments.Add(arc);
            loopFigure.IsFilled = false;
            loopFigure.IsClosed = false;

            geometry.Figures.Add(loopFigure);


            //var path = new Path();
            //path.Data = geometry;
            //path.Stroke = new SolidColorBrush(Colors.Black);
            //path.StrokeThickness = 1;


            // Call the base method to add arrows on the ends.
            base.SetData();
        }
    }
}