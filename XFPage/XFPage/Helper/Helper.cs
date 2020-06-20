using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Xamarin.Forms.Internals;

namespace XFPage.Helper 
{
    [Preserve(AllMembers = true)]
    public enum TransitionType
    {
        None = -1,

        Default = 0,

        Fade = 1,

        SlideFromLeft = 2,

        SlideFromRight = 3,

        SlideFromTop = 4,

        SlideFromBottom = 5
    }

    [Preserve(AllMembers = true)]
    public class GestureEventArgs : EventArgs
    {
        public Xamarin.Forms.Point GesturePoint { get; internal set; }

    }

    [Preserve(AllMembers = true)]
    public class TabsEventArgs : CancelEventArgs
    {
        public int SelectedIndex { get; internal set; }

        public int LastSelectedIndex { get; internal set; }
    }

    [Preserve(AllMembers = true)]
    public enum BadgePosition
    {
        PositionTopRight = 0,
        PositionTopLeft = 1,
        PositionBottomRight = 2,
        PositionBottomLeft = 3,
        PositionCenter = 4,
        PositionTopCenter = 5,
        PositionBottomCenter = 6,
        PositionLeftCenter = 7,
        PositionRightCenter = 8,
    }
}