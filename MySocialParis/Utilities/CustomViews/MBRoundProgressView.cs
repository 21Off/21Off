
using System;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.ObjCRuntime;
namespace MSP.Client
{
}
namespace RedPlum
{
	public class MBRoundProgressView : UIProgressView
	{
		public MBRoundProgressView () : base(new RectangleF (0.0f, 0.0f, 37.0f, 37.0f))
		{
		}

		public override void Draw (RectangleF rect)
		{


			RectangleF allRect = this.Bounds;
			RectangleF circleRect = new RectangleF (allRect.Location.X + 2, allRect.Location.Y + 2, allRect.Size.Width - 4, allRect.Size.Height - 4);

			CGContext context = UIGraphics.GetCurrentContext ();

			// Draw background
			context.SetRGBStrokeColor (1.0f, 1.0f, 1.0f, 1.0f);
			// white
			context.SetRGBFillColor (1.0f, 1.0f, 1.0f, 0.1f);
			// translucent white
			context.SetLineWidth (2.0f);
			context.FillEllipseInRect (circleRect);
			context.StrokeEllipseInRect (circleRect);

			// Draw progress
			float x = (allRect.Size.Width / 2);
			float y = (allRect.Size.Height / 2);
			context.SetRGBFillColor (1.0f, 1.0f, 1.0f, 1.0f);
			// white
			context.MoveTo (x, y);
			context.AddArc (x, y, (allRect.Size.Width - 4) / 2, -(float)(Math.PI / 2), (float)(this.Progress * 2 * Math.PI) - (float)(Math.PI / 2), false);
			context.ClosePath ();
			context.FillPath ();
		}

	}
}
