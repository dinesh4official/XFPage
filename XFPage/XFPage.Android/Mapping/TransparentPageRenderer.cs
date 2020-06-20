using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using XFPage.Control;
using XFPage.Droid.Mapping;
using XFPage.Droid.Helper;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;


[assembly: ExportRenderer(typeof(TransparentPage), typeof(TransparentPageRenderer))]
namespace XFPage.Droid.Mapping
{
    [Preserve(AllMembers = true)]
    public class TransparentPageRenderer : PageRenderer, GestureDetector.IOnGestureListener
    {
        #region Fields

        private long previousClickTime = 0;
        private float downX = 0;
        private float downY = 0;
        private float density = 0;
        private GestureDetector gestureDetector;

        #endregion

        #region Properties

        public TransparentPage TransparentPageElement { get { return this.Element as TransparentPage; } }

        #endregion

        #region Constructor

        public TransparentPageRenderer(Context context) : base(context)
        {
            gestureDetector = new GestureDetector(context, this);
            density = Resources.DisplayMetrics.Density;
        }

        #endregion

        #region Override Methods

        protected override void OnAttachedToWindow()
        {
            base.OnAttachedToWindow();
            var modalContainer = this.Parent.GetModalContainer();
            if (modalContainer != null && modalContainer.ChildCount > 0)
                modalContainer.GetChildAt(0).SetBackgroundColor(Android.Graphics.Color.Transparent);
        }

        public override bool OnInterceptTouchEvent(MotionEvent ev)
        {
            OnTouchEvent(ev);
            return base.OnInterceptTouchEvent(ev);
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            base.OnTouchEvent(e);

            var xdiff = Math.Abs((downX - e.RawX) / density);
            var ydiff = Math.Abs((downY - e.RawY) / density);
            bool canPassMove = xdiff > 10 || ydiff > 10;
            if (e.Action != MotionEventActions.Move || canPassMove)
                gestureDetector.OnTouchEvent(e);

            if (e.Action == MotionEventActions.Down)
            {
                downX = e.RawX;
                downY = e.RawY;
            }

            return true;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (gestureDetector != null)
                {
                    gestureDetector.Dispose();
                    gestureDetector = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Public Interface

        public virtual void OnClick(global::Android.Views.View view)
        {
            var touchPosition = new Xamarin.Forms.Point(downX / density, downY / density);
            long currentTouchTime = Java.Lang.JavaSystem.CurrentTimeMillis();
            if (previousClickTime == 0 || (currentTouchTime - previousClickTime > 250))
            {
                previousClickTime = currentTouchTime;
                if (previousClickTime != 0 && TransparentPageElement != null)
                    TransparentPageElement.RaiseItemClicked(touchPosition);
            }
        }

        public bool OnDown(MotionEvent e)
        {
            return true;
        }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            return true;
        }

        public void OnLongPress(MotionEvent e)
        {

        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return true;
        }

        public void OnShowPress(MotionEvent e)
        {
        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            OnClick(this);
            return true;
        }

        #endregion

        #region Methods

        #endregion
    }
}