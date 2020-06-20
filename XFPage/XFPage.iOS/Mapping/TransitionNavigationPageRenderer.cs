using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using CoreAnimation;
using CoreGraphics;
using Foundation;
using XFPage.Control;
using XFPage.Helper;
using XFPage.iOS.Mapping;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TransitionNavigationPage), typeof(TransitionNavigationPageRenderer))]
namespace XFPage.iOS.Mapping
{
    [Preserve(AllMembers = true)]
    public class TransitionNavigationPageRenderer : NavigationRenderer
    {
        #region Fields

        TransitionNavigationPage TransitionNavigationPage => Element as TransitionNavigationPage;

        #endregion

        #region Constructor

        public TransitionNavigationPageRenderer() : base()
        {

        }

        #endregion

        #region Override Methods

        public override void DidMoveToParentViewController(UIViewController parent)
        {
            base.DidMoveToParentViewController(parent);

            if (ParentViewController != null && TransitionNavigationPage.NeedTransparentPage)
                ParentViewController.ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext;
        }

        public override void PushViewController(UIViewController viewController, bool animated)
        {
            var transistionType = TransitionNavigationPage.TransitionType;
            if (transistionType == TransitionType.None || transistionType == TransitionType.Default)
            {
                var needanimation = transistionType == TransitionType.None ? false : animated;
                base.PushViewController(viewController, needanimation);
                return;
            }
            else if (transistionType == TransitionType.Fade)
            {
                FadeAnimation(View);
            }
            else
            {
                var transition = CATransition.CreateAnimation();
                transition.Duration = 0.25f;
                transition.Type = CAAnimation.TransitionPush;
                switch (transistionType)
                {
                    case TransitionType.SlideFromBottom:
                        transition.Subtype = CAAnimation.TransitionFromBottom;
                        break;
                    case TransitionType.SlideFromLeft:
                        transition.Subtype = CAAnimation.TransitionFromLeft;
                        break;
                    case TransitionType.SlideFromRight:
                        transition.Subtype = CAAnimation.TransitionFromRight;
                        break;
                    case TransitionType.SlideFromTop:
                        transition.Subtype = CAAnimation.TransitionFromTop;
                        break;
                }

                View.Layer.AddAnimation(transition, null);
            }

            base.PushViewController(viewController, false);
        }

        public override UIViewController PopViewController(bool animated)
        {
            var transistionType = TransitionNavigationPage.TransitionType == TransitionType.None ? TransitionType.SlideFromRight : TransitionNavigationPage.TransitionType;
            if (transistionType == TransitionType.None || transistionType == TransitionType.Default)
            {
                var needanimation = transistionType == TransitionType.None ? false : animated;
                return base.PopViewController(needanimation);
            }
            else if (transistionType == TransitionType.Fade)
            {
                FadeAnimation(View);
            }
            else
            {
                var transition = CATransition.CreateAnimation();
                transition.Duration = 0.25f;
                transition.Type = CAAnimation.TransitionPush;
                switch (transistionType)
                {
                    case TransitionType.SlideFromBottom:
                        transition.Subtype = CAAnimation.TransitionFromTop;
                        break;
                    case TransitionType.SlideFromLeft:
                        transition.Subtype = CAAnimation.TransitionFromRight;
                        break;
                    case TransitionType.SlideFromRight:
                        transition.Subtype = CAAnimation.TransitionFromLeft;
                        break;
                    case TransitionType.SlideFromTop:
                        transition.Subtype = CAAnimation.TransitionFromBottom;
                        break;
                }

                View.Layer.AddAnimation(transition, null);
            }

            return base.PopViewController(false);
        }

        #endregion

        #region Private Methods

        private void FadeAnimation(UIView view, double duration = 1.0)
        {
            view.Alpha = 0.0f;
            view.Transform = CGAffineTransform.MakeIdentity();
            UIView.Animate(duration, 0, UIViewAnimationOptions.CurveEaseInOut, () =>
            {
                view.Alpha = 1.0f;
            },
               null
            );
        }

        #endregion
    }
}