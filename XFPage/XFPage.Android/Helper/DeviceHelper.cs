using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace XFPage.Droid.Helper 
{
    [Preserve(AllMembers = true)]
    public static class DeviceHelper
    {
        #region Fields

        public static MainActivity MainActivity;

        #endregion

        #region Static Methods

        public static void Init(MainActivity mainActivity)
        {
            MainActivity = mainActivity;
        }

        #endregion

        #region Properties

        public static double DPI
        {
            get
            {
                return (double)Application.Context.Resources.DisplayMetrics.DensityDpi;
            }
        }

        #endregion
    }
}