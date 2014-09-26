
using System.Windows;
using System.Windows.Media;

namespace BioCheck.Controls.Arrows
{

    /// <summary>

    ///     Draws a series of connected straight lines with

    ///     optional arrows on the ends.

    /// </summary>

    public class ArrowPolyline : ArrowLineBase
    {

        /// <summary>

        ///     Identifies the Points dependency property.

        /// </summary>

        public static readonly DependencyProperty PointsProperty =

            DependencyProperty.Register("Points",

                typeof(PointCollection), typeof(ArrowPolyline),

                new PropertyMetadata(null));



        /// <summary>

        ///     Gets or sets a collection that contains the

        ///     vertex points of the ArrowPolyline.

        /// </summary>

        public PointCollection Points
        {

            set { SetValue(PointsProperty, value); }

            get { return (PointCollection)GetValue(PointsProperty); }

        }



        /// <summary>

        ///     Initializes a new instance of the ArrowPolyline class.

        /// </summary>

        public ArrowPolyline()
        {

            Points = new PointCollection();

            SetData();

        }



        /// <summary>

        ///     Gets a value that represents the Geometry of the ArrowPolyline.

        /// </summary>

        public override void SetData()
        {

            // Clear out the PathGeometry.

            geometry.Figures.Clear();



            // Try to avoid unnecessary indexing exceptions.

            if (Points.Count > 0)
            {

                // Define a PathFigure containing the points.

                figure.StartPoint = Points[0];

                lineSegment.Points.Clear();



                for (int i = 1; i < Points.Count; i++)

                    lineSegment.Points.Add(Points[i]);



                figure.IsFilled = false;



                geometry.Figures.Add(figure);

            }



            // Call the base method to add arrows on the ends.

            base.SetData();

        }

    }

}