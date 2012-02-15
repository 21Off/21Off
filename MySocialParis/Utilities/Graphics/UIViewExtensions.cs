using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using System;
using MonoTouch.Foundation;

namespace MSP.Client
{
	public static class UIViewExtensions {
		
		
		private static SizeF GetTextSize (string text, UILabel label)
		{
			return new NSString (text).StringSize (label.Font, UIScreen.MainScreen.Bounds.Width, 
			                                       UILineBreakMode.TailTruncation);
		}		
		
		public static void SetTitleText(string text1, string text2, UILabel l1, UILabel l2)
		{
			if (l1 != null)
				l1.Text = text1;
			
			if (l2 != null)
				l2.Text = text2;			
			
			if (l1 != null)
			{
				SizeF sizeF = GetTextSize(text1, l1);
				PointF point = l1.Frame.Location;
				point.X = (320 - sizeF.Width) / 2;
				
				l1.Frame = new RectangleF(point, sizeF);
			}

			if (l2 != null)
			{
				SizeF sizeF = GetTextSize(text2, l2);
				PointF point = l2.Frame.Location;
				point.X = (320 - sizeF.Width) / 2;
				//point.Y = 25;
				
				l2.Frame = new RectangleF(point, sizeF);
			}
		}
		
		public static void DrawRoundRectangle (this UIView view, RectangleF rrect, float radius, UIColor color) 
		{
			var context = UIGraphics.GetCurrentContext ();
	
			color.SetColor ();	
			
			float minx = rrect.Left;
			float midx = rrect.Left + (rrect.Width)/2;
			float maxx = rrect.Right;
			float miny = rrect.Top;
			float midy = rrect.Y+rrect.Size.Height/2;
			float maxy = rrect.Bottom;
	
			if (context != null)
			{			
				context.MoveTo (minx, midy);
				context.AddArcToPoint (minx, miny, midx, miny, radius);
				context.AddArcToPoint (maxx, miny, maxx, midy, radius);
				context.AddArcToPoint (maxx, maxy, midx, maxy, radius);
				context.AddArcToPoint (minx, maxy, minx, midy, radius);
				context.ClosePath ();
				context.DrawPath (CGPathDrawingMode.Fill); // test others?
			}
		}
		
		public static void DrawBorder(this UIView view, UIColor borderColor)
		{
			view.Layer.BorderColor = borderColor.CGColor;
			view.Layer.BorderWidth = 2f;	
		}
	}
	

}
