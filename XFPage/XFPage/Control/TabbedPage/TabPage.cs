using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace XFPage.Control 
{
    [Preserve(AllMembers = true)]
    public class TabPage : TabbedPage
    {
        #region Bindable Properties

        public double BottomBarHeight
        {
            get { return (double)GetValue(BottomBarHeightProperty); }
            set { SetValue(BottomBarHeightProperty, value); }
        }

        public static readonly BindableProperty BottomBarHeightProperty = BindableProperty.Create(nameof(BottomBarHeight), typeof(double), typeof(TabPage), 0d);

        public bool IsSelectionChanged
        {
            get { return (bool)GetValue(IsSelectionChangedProperty); }
            set { SetValue(IsSelectionChangedProperty, value); }
        }

        public static readonly BindableProperty IsSelectionChangedProperty = BindableProperty.Create(nameof(IsSelectionChanged), typeof(bool), typeof(TabPage), false);

        #endregion

        #region Constructor

        public TabPage()
        {
            TabItemBounds = new Dictionary<string, Rectangle>();
        }

        #endregion

        #region Properties

        public Dictionary<string, Rectangle> TabItemBounds { get; set; }

        #endregion
    }
}
