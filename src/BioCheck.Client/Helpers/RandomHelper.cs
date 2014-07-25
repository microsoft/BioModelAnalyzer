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

namespace BioCheck.Helpers
{
    public static class RandomHelper
    {
        private static Random rand;

        static RandomHelper()
        {
            rand = new Random();
        }

        public static int GetRandom(int from, int to)
        {
            // Increment 'to' to get the actual upper limit, else it will never be returned
            var value = rand.Next(from, to + 1);

            return value;
        }
    }
}
