using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using XFPage.Control;
using XFPage.Droid.Mapping;
using XFPage.Helper;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android; 
using Xamarin.Forms.Platform.Android.AppCompat;

[assembly: ExportRenderer(typeof(TransitionNavigationPage), typeof(TransitionNavigationPageRenderer))]
namespace XFPage.Droid.Mapping
{
    [Preserve(AllMembers = true)]
    public class TransitionNavigationPageRenderer : NavigationPageRenderer
    {
        #region  Fields

        TransitionNavigationPage TransitionNavigationPage => Element as TransitionNavigationPage;

        #endregion

        #region Constructor

        public TransitionNavigationPageRenderer(Context context) : base(context)
        {

        }

        #endregion

        #region Override Methods

        //For future purpose
        //protected override void OnLayout(bool changed, int l, int t, int r, int b)
        //{
        //    TransitionNavigationPage.IgnoreLayoutChange = true;
        //    base.OnLayout(changed, l, t, r, b);
        //    TransitionNavigationPage.IgnoreLayoutChange = false;
        //    int containerHeight = b - t;

        //    PageController.ContainerArea = new Rectangle(0, 0, Context.FromPixels(r - l), Context.FromPixels(containerHeight));

        //    for (var i = 0; i < ChildCount; i++)
        //    {
        //        Android.Views.View child = GetChildAt(i);

        //        if (child is Android.Support.V7.Widget.Toolbar)
        //            continue;

        //        child.Layout(0, 0, r, b);
        //    }
        //}

        protected override void SetupPageTransition(Android.Support.V4.App.FragmentTransaction transaction, bool isPush)
        {
            var transistionType = !isPush && TransitionNavigationPage.TransitionType == TransitionType.None ? TransitionType.SlideFromRight : TransitionNavigationPage.TransitionType;
            switch (transistionType)
            {
                case TransitionType.None:
                    return;
                case TransitionType.Default:
                    return;
                case TransitionType.Fade:
                    transaction.SetCustomAnimations(Resource.Animation.fade_in, Resource.Animation.fade_out,
                                                    Resource.Animation.fade_out, Resource.Animation.fade_in);
                    break;
                case TransitionType.SlideFromLeft:
                    if (isPush)
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_left, Resource.Animation.exit_right,
                                                        Resource.Animation.enter_right, Resource.Animation.exit_left);
                    }
                    else
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_right, Resource.Animation.exit_left,
                                                        Resource.Animation.enter_left, Resource.Animation.exit_right);
                    }
                    break;
                case TransitionType.SlideFromRight:
                    if (isPush)
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_right, Resource.Animation.exit_left,
                                                        Resource.Animation.enter_left, Resource.Animation.exit_right);
                    }
                    else
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_left, Resource.Animation.exit_right,
                                                        Resource.Animation.enter_right, Resource.Animation.exit_left);
                    }
                    break;
                case TransitionType.SlideFromTop:
                    if (isPush)
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_top, Resource.Animation.exit_bottom,
                                                        Resource.Animation.enter_bottom, Resource.Animation.exit_top);
                    }
                    else
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_bottom, Resource.Animation.exit_top,
                                                        Resource.Animation.enter_top, Resource.Animation.exit_bottom);
                    }
                    break;
                case TransitionType.SlideFromBottom:
                    if (isPush)
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_bottom, Resource.Animation.exit_top,
                                                        Resource.Animation.enter_top, Resource.Animation.exit_bottom);
                    }
                    else
                    {
                        transaction.SetCustomAnimations(Resource.Animation.enter_top, Resource.Animation.exit_bottom,
                                                        Resource.Animation.enter_bottom, Resource.Animation.exit_top);
                    }
                    break;
                default:
                    return;
            }
        }

        #endregion
    }
}