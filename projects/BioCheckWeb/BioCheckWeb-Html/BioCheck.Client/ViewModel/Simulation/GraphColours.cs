using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.Generic;
using BioCheck.Helpers;

namespace BioCheck.ViewModel.Simulation
{
    public static class GraphColours
    {
        static List<Color> colours;
        static List<Color> usedColours;

        static GraphColours()
        {
            CreateColours();
        }

        public static Brush Next()
        {
            if (colours.Count == 0)
            {
                colours.AddRange(usedColours);
                usedColours.Clear();
            }

            var indexToUse = RandomHelper.GetRandom(0, colours.Count - 1);
            //int indexToUse = 0;
            var colourToUse = colours[indexToUse];
            colours.RemoveAt(indexToUse);
            usedColours.Add(colourToUse);

            return new SolidColorBrush(colourToUse);
        }

        private static void CreateColours()
        {
            usedColours = new List<Color>();

            colours = new List<Color>()
                {
                    Color.FromArgb(255, 240, 58, 6),
                    Color.FromArgb(255, 240, 114, 6),
                    Color.FromArgb(255, 240, 161, 6),
                    Color.FromArgb(255, 240, 203, 6),
                    Color.FromArgb(255, 240, 240, 6),

                    Color.FromArgb(255, 107, 240, 6),
                    Color.FromArgb(255, 6, 240, 30),
                    Color.FromArgb(255, 6, 240, 130),
                    Color.FromArgb(255, 6, 240, 230),


                    Color.FromArgb(255, 6, 147, 240),
                    Color.FromArgb(255, 6, 53, 240),
                    Color.FromArgb(255, 62, 6, 240),
                    Color.FromArgb(255, 158, 6, 240),
                    Color.FromArgb(255, 240, 6, 200),
                    Color.FromArgb(255, 240, 6, 6),
                };
        }
    }
}
