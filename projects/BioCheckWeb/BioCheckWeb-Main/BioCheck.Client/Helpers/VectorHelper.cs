using System;
using System.Windows;

namespace BioCheck.Helpers
{
    public static class VectorHelper
    {
        public static void CreateStop(double margin, double fromX, double fromY, double toX, double toY, out double newToX, out double newToY)
        {
            // Get the horizontal position of the FromX relative to ToX
            HorizontalAlignment horizontal = GetHorizontalStart(fromX, toX);

            // Get the vertical position of the FromY relative to ToY
            VerticalAlignment vertical = GetVerticalStart(fromY, toY);

            // Draw the arrow to its final rest at the centre point - margin px of the variable
            if (horizontal != HorizontalAlignment.Center &&
                vertical != VerticalAlignment.Center)
            {
                CalculateStop(margin, fromX, fromY, toX, toY, out toX, out toY);
            }
            else if (horizontal == HorizontalAlignment.Center)
            {
                if (vertical == VerticalAlignment.Top)
                {
                    toY -= margin;
                }
                else if (vertical == VerticalAlignment.Bottom)
                {
                    toY += margin;
                }
            }
            else if (vertical == VerticalAlignment.Center)
            {
                if (horizontal == HorizontalAlignment.Left)
                {
                    toX -= margin;
                }
                else if (horizontal == HorizontalAlignment.Right)
                {
                    toX += margin;
                }
            }

            newToX = toX;
            newToY = toY;
        }

        public static void CalculateStop(double margin, double fromX, double fromY, double toX, double toY, out double newToX, out double newToY)
        {
            double x = fromX - toX;
            double y = toY - fromY;

            double newX;
            double newY;

            VectorHelper.Vector(margin, x, y, out newX, out newY);

            newToX = toX + newX;
            newToY = toY - newY;
        }

        public static void Vector(double margin, double x, double y, out double newX, out double newY)
        {
            double hypotenuse = Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2));

            newX = (margin * x) / hypotenuse;
            newY = (margin * y) / hypotenuse;
        }

        public static void Trig(double opposite, double adjacent, out double newOpposite, out double newAdjacent)
        {
            double hypotenuse = Math.Sqrt(Math.Pow(opposite, 2) + Math.Pow(adjacent, 2));

            // Get the angle O from the origin to the top/opposite
            double sinOangle = opposite / hypotenuse;
            double OAngle = Math.Round((Math.Asin(sinOangle) * (180 / Math.PI)), 4);

            // Remove 20 pixels from the hypotenuse to get the new length
            // TODO - if it's less than 20, show an error
            double newHypotenuse = hypotenuse - 20;

            // Calculate the new Opposite and Adjacent values
            newOpposite = Math.Round((newHypotenuse * Math.Sin(OAngle)), 4);
            newAdjacent = Math.Round((newHypotenuse * Math.Cos(OAngle)), 4);
        }

        private static VerticalAlignment GetVerticalStart(double fromY, double toY)
        {
            VerticalAlignment vertical = VerticalAlignment.Stretch;
            if (toY == fromY)
            {
                vertical = VerticalAlignment.Center;
            }
            else if (toY > fromY)
            {
                // ToY is below.
                vertical = VerticalAlignment.Top;
            }
            else if (fromY > toY)
            {
                // ToY is above
                vertical = VerticalAlignment.Bottom;
            }
            return vertical;
        }

        private static HorizontalAlignment GetHorizontalStart(double fromX, double toX)
        {
            HorizontalAlignment horizontal = HorizontalAlignment.Stretch;
            if (toX == fromX)
            {
                horizontal = HorizontalAlignment.Center;
            }
            else if (toX > fromX)
            {
                // ToX is to the right.
                horizontal = HorizontalAlignment.Left;
            }
            else if (fromX > toX)
            {
                // ToX is to the left
                horizontal = HorizontalAlignment.Right;
            }
            return horizontal;
        }
    }
}
