using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using XFPage.Control;
using XFPage.iOS.Mapping;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(TransparentPage), typeof(TransparentPageRenderer))]
namespace XFPage.iOS.Mapping
{
    [Preserve(AllMembers = true)]
    public class TransparentPageRenderer : PageRenderer
    {
        #region Properties

        private bool IsModalStyleUpdated { get; set; }

        public TransparentPage TransparentPageElement { get { return this.Element as TransparentPage; } }

        #endregion

        #region Constructor

        public TransparentPageRenderer() : base()
        {

        }

        #endregion

        #region Override Methods

        public override void DidMoveToParentViewController(UIViewController parent)
        {
            base.DidMoveToParentViewController(parent);

            if (ParentViewController != null)
                ParentViewController.ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            View.BackgroundColor = Color.Transparent.ToUIColor();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            if (!IsModalStyleUpdated)
                this.UpdateModalPresentationStyle();
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            UITouch touch = touches.AnyObject as UITouch;
            var initialX = touch.LocationInView(this.NativeView).X;
            var initialY = touch.LocationInView(this.NativeView).Y;
            var touchPosition = new Point((int)initialX, (int)initialY);

            if (TransparentPageElement != null && !touchPosition.IsEmpty)
                TransparentPageElement.RaiseItemClicked(touchPosition);

            base.TouchesBegan(touches, evt);
        }

        #endregion

        #region Private Methods

        private void UpdateModalPresentationStyle()
        {
            var parentContoller = this.ViewController.ParentViewController;
            while (parentContoller != null)
            {
                parentContoller.ModalPresentationStyle = UIModalPresentationStyle.OverCurrentContext;
                if (parentContoller.ParentViewController == null)
                {
                    var transparentColor = UIColor.Clear;
                    var ModalwrapperController = parentContoller;
                    ModalwrapperController.View.BackgroundColor = transparentColor;
                    ModalwrapperController.View.Subviews[0].BackgroundColor = transparentColor;
                    IsModalStyleUpdated = true;
                }

                parentContoller = parentContoller.ParentViewController;
            }
        }

        #endregion
    }
}