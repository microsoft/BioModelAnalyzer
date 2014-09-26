
using System;

namespace BioCheck.Controls.Arrows
{

    /// <summary>

    /// 2 dimensional geometry math library class

    ///

    /// All angles are radians - positive is counterclockwise

    /// All numbers are double

    /// </summary>

    public class Math2D
    {

        /// <summary>

        /// Tested

        /// Calculate a vector length from X and Y coordinates

        /// </summary>

        /// <param name="xx">X coordinate</param>

        /// <param name="yy">Y coordinate</param>

        /// <returns>Length of vector</returns>

        public static double LengthFromCoords(double xx, double yy)
        {

            return Math.Sqrt(xx * xx + yy * yy);

        }

        /// <summary>

        /// //##jss Untested

        /// Calculate a vector sum length from coordinates of 2 vectors

        /// </summary>

        /// <param name="x1">Vector 1 x coordinate</param>

        /// <param name="y1">Vector 1 y coordinate</param>

        /// <param name="x2">Vector 2 x coordinate</param>

        /// <param name="y2">Vector 2 y coordinate</param>

        /// <returns>Vector sum length</returns>

        public static double LengthFromCoordsSum(double x1, double y1, double x2, double y2)
        {

            double sumX = x1 + x2;

            double sumY = y1 + y2;



            return LengthFromCoords(sumX, sumY);

        }

        /// <summary>

        /// Tested

        /// Calculate a vector difference length from coordinates of 2 vectors

        /// </summary>

        /// <param name="x1">Vector 1 x coordinate</param>

        /// <param name="y1">Vector 1 y coordinate</param>

        /// <param name="x2">Vector 2 x coordinate</param>

        /// <param name="y2">Vector 2 y coordinate</param>

        /// <returns>Vector difference length</returns>

        public static double LengthFromCoordsDiff(double x1, double y1, double x2, double y2)
        {

            double diffX = x2 - x1;

            double diffY = y2 - y1;



            return LengthFromCoords(diffX, diffY);

        }

        /// <summary>

        /// Tested

        /// Calculate the x axis angle from vector coordinates

        /// </summary>

        /// <param name="xx">X coordinate</param>

        /// <param name="yy">Y coordinate</param>

        /// <returns>X axis angle</returns>

        public static double XAxisAngleFromCoords(double xx, double yy)
        {

            double hyp = Math.Sqrt(xx * xx + yy * yy);



            double cosXAxisAngle = xx / hyp;



            // NaN = Not a Number which means a divide by zero happened above

            if (Double.IsNaN(cosXAxisAngle))
            {

                return Double.NaN;

            }



            double xAxisAngle = Math.Acos(cosXAxisAngle);



            // acos goes from 0 to pi.  Normalize to between 0 and 2 * pi so we do not lose info

            if (yy < 0.0)
            {

                xAxisAngle = 2 * Math.PI - xAxisAngle;

            }

            return xAxisAngle;

        }

        /// <summary>

        /// //##jss Untested

        /// Calculate x axis angle for sum of 2 vectors from coordinates

        /// </summary>

        /// <param name="x1">Vector 1 x coordinate</param>

        /// <param name="y1">Vector 1 y coordinate</param>

        /// <param name="x2">Vector 2 x coordinate</param>

        /// <param name="y2">Vector 2 x coordinate</param>

        /// <returns>Vector sum x axis angle</returns>

        public static double XAxisAngleFromCoordsSum(double x1, double y1, double x2, double y2)
        {

            double sumX = x1 + x2;

            double sumY = y1 + y2;



            return XAxisAngleFromCoords(sumX, sumY);

        }

        /// <summary>

        /// //##jss Untested

        /// Calculate X axis angle of difference vector from coordinates

        /// </summary>

        /// <param name="x1">Vector 1 x coordinate</param>

        /// <param name="y1">Vector 1 y coordinate</param>

        /// <param name="x2">Vector 2 x coordinate</param>

        /// <param name="y2">Vector 2 y coordinate</param>

        /// <returns>Difference vector x axis angle</returns>

        public static double XAxisAngleFromCoordsDiff(double x1, double y1, double x2, double y2)
        {

            double diffX = x2 - x1;

            double diffY = y2 - y1;



            return XAxisAngleFromCoords(diffX, diffY);

        }

        /// <summary>

        /// Tested

        /// Calculate x coordinate from length and x axis angle of a vector

        /// </summary>

        /// <param name="length">Vector length</param>

        /// <param name="angle">Vector x axis angle</param>

        /// <returns>X coordinate</returns>

        public static double XCoordFromLengthsAndXAxisAngles(double length, double angle)
        {

            return length * Math.Cos(angle);

        }

        /// <summary>

        /// //##jss Untested

        /// Calculate x coordinate of sum vector from lengths and x axis angles of 2 vectors

        /// </summary>

        /// <param name="length1">Vector 1 length</param>

        /// <param name="angle1">Vector 1 x axis angle</param>

        /// <param name="length2">Vector 2 length</param>

        /// <param name="angle2">Vector 2 x axis angle</param>

        /// <returns>X coordinate</returns>

        public static double XCoordFromLengthsAndXAxisAnglesSum(double length1, double angle1,

                double length2, double angle2)
        {

            return length1 * Math.Cos(angle1) + length2 * Math.Cos(angle2);

        }

        /// <summary>

        /// //##jss Untested

        /// Calculate x coordinate of difference vector from lengths and x axis angles of 2 vectors

        /// </summary>

        /// <param name="length1">Vector 1 length</param>

        /// <param name="angle1">Vector 1 x axis angle</param>

        /// <param name="length2">Vector 2 length</param>

        /// <param name="angle2">Vector 2 x axis angle</param>

        /// <returns>X coordinate</returns>

        public static double XCoordFromLengthsAndXAxisAnglesDiff(double length1, double angle1,

                double length2, double angle2)
        {

            return length1 * Math.Cos(angle1) - length2 * Math.Cos(angle2);

        }

        /// <summary>

        /// Tested

        /// Calculate y coordinate from length and x axis angle of a vector

        /// </summary>

        /// <param name="length">Vector length</param>

        /// <param name="angle">Vector x axis angle</param>

        /// <returns>Y coordinate</returns>

        public static double YCoordFromLengthsAndXAxisAngles(double length, double angle)
        {

            return length * Math.Sin(angle);

        }

        /// <summary>

        /// //##jss Untested

        /// Calculate y coordinate of sum vector from length and x axis angles of 2 vectors

        /// </summary>

        /// <param name="length1">Vector 1 length</param>

        /// <param name="angle1">Vector 1 x axis angle</param>

        /// <param name="length2">Vector 2 length</param>

        /// <param name="angle2">Vector 2 x axis angle</param>

        /// <returns>Y coordinate</returns>

        public static double YCoordFromLengthsAndXAxisAnglesSum(double length1, double angle1,

                double length2, double angle2)
        {

            return length1 * Math.Sin(angle1) + length2 * Math.Sin(angle2);

        }

        /// <summary>

        /// //##jss Untested

        /// Calculate y coordinate from difference vector from length and x axis angles of 2 vectors

        /// </summary>

        /// <param name="length1">Vector 1 length</param>

        /// <param name="angle1">Vector 1 x axis angle</param>

        /// <param name="length2">Vector 2 length</param>

        /// <param name="angle2">Vector 2 x axis angle</param>

        /// <returns>Y coordinate</returns>

        public static double YCoordFromLengthsAndXAxisAnglesDiff(double length1, double angle1,

                double length2, double angle2)
        {

            return length1 * Math.Sin(angle1) - length2 * Math.Sin(angle2);

        }

    }

}