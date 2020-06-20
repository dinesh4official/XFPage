using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CoreGraphics;
using Foundation;
using UIKit;

namespace XFPage.iOS.Control 
{ 
	[Preserve(AllMembers = true)]
	public class NativeCircleView : UIView
	{
		#region Properties

		public UIColor FillColor { get; set; }

		public UIColor StrokeColor { get; set; }

		public bool IsActive { get; set; }

		#endregion

		#region Constructor

		public NativeCircleView()
		{
			this.BackgroundColor = UIColor.Clear;
		}

        #endregion

        #region Override Methods

        public override void Draw(CGRect rect)
		{
			this.DrawCircleView(rect, this.FillColor, this.StrokeColor, this.IsActive);
		}

        #endregion

        #region Private Methods

        private void DrawCircleView(CGRect frame, UIColor colorFill, UIColor colorStroke, bool isActive)
		{
			if (isActive)
			{
				var fillPath = UIBezierPath.FromOval(new CGRect(frame.GetMinX() + 1.0f, frame.GetMinY() + 1.0f, frame.Width - 2.0f, frame.Height - 2.0f));
				colorFill.SetFill();
				fillPath.Fill();
			}

			var strokePath = UIBezierPath.FromOval(new CGRect(frame.GetMinX() + 1.0f, frame.GetMinY() + 1.0f, frame.Width - 2.0f, frame.Height - 2.0f));
			colorStroke.SetStroke();
			strokePath.LineWidth = 1.5f;
			strokePath.Stroke();
		}

        #endregion
    }
}