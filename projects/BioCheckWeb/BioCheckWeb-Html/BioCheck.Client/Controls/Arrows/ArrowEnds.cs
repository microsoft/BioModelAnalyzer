// From
// http://webcache.googleusercontent.com/search?q=cache:EyRx37REEc8J:www.sanbarcomputing.com/bloglineswitharrows.shtml+bloglineswitharrows&cd=5&hl=en&ct=clnk&gl=uk&client=firefox-a&source=www.google.co.uk

// As mentioned:
// http://forums.silverlight.net/forums/p/105958/563070.aspx


//------------------------------------------
// ArrowEnds.cs (c) 2007 by Charles Petzold
//------------------------------------------

using System;

namespace BioCheck.Controls.Arrows
{
    /// <summary>
    ///     Indicates which end of the line has an arrow.
    /// </summary>
    [Flags]
    public enum ArrowEnds
    {
        None = 0,
        Start = 1,
        End = 2,
        Both = 3
    }
}
