using System;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

namespace MonoTouch.Dialog
{
	public static class GraphicsUtil {
		
		/// <summary>
		///    Creates a path for a rectangle with rounded corners
		/// </summary>
		/// <param name="rect">
		/// The <see cref="RectangleF"/> rectangle bounds
		/// </param>
		/// <param name="radius">
		/// The <see cref="System.Single"/> size of the rounded corners
		/// </param>
		/// <returns>
		/// A <see cref="CGPath"/> that can be used to stroke the rounded rectangle
		/// </returns>
		public static CGPath MakeRoundedRectPath (RectangleF rect, float radius)
		{
			float minx = rect.Left;
			float midx = rect.Left + (rect.Width)/2;
			float maxx = rect.Right;
			float miny = rect.Top;
			float midy = rect.Y+rect.Size.Height/2;
			float maxy = rect.Bottom;

			var path = new CGPath ();
			path.MoveToPoint (minx, midy);
			path.AddArcToPoint (minx, miny, midx, miny, radius);
			path.AddArcToPoint (maxx, miny, maxx, midy, radius);
			path.AddArcToPoint (maxx, maxy, midx, maxy, radius);
			path.AddArcToPoint (minx, maxy, minx, midy, radius);		
			path.CloseSubpath ();
			
			return path;
        }
		
		public static void FillRoundedRect (CGContext ctx, RectangleF rect, float radius)
		{
			var p = GraphicsUtil.MakeRoundedRectPath (rect, radius);
			ctx.AddPath (p);
			ctx.FillPath ();
		}

		public static CGPath MakeRoundedPath (float size, float radius)
		{
			float hsize = size/2;
			
			var path = new CGPath ();
			path.MoveToPoint (size, hsize);
			path.AddArcToPoint (size, size, hsize, size, radius);
			path.AddArcToPoint (0, size, 0, hsize, radius);
			path.AddArcToPoint (0, 0, hsize, 0, radius);
			path.AddArcToPoint (size, 0, size, hsize, radius);
			path.CloseSubpath ();
			
			return path;
		}
		
		//
		// Centers image, scales and removes borders
		//
		public static UIImage PrepareForProfileView (UIImage image, int dimx, int dimy)
		{
			
			if (image == null)
				throw new ArgumentNullException ("image");
			
			UIGraphics.BeginImageContext (new SizeF (dimx, dimy));
			var c = UIGraphics.GetCurrentContext ();
			
			var path = GraphicsUtil.MakeRoundedRectPath(new RectangleF(0, 0, dimx, dimy), 4);
			
			c.AddPath (path);
			c.Clip ();
			
			// Twitter not always returns squared images, adjust for that.
			var cg = image.CGImage;
			float width = cg.Width;
			float height = cg.Height;
			if (width != height) {
				float x = 0, y = 0;
				if (width > height) {
					x = (width - height) / 2;
					width = height;
				} else {
					y = (height - width) / 2;
					height = width;
				}
				c.ScaleCTM (1, -1);
				using (var copy = cg.WithImageInRect (new RectangleF (x, y, width, height))) {
					c.DrawImage (new RectangleF (0, 0, dimx, -dimy), copy);
				}
			} else
				image.Draw (new RectangleF (0, 0, dimx, dimy));
			
			var converted = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();
			return converted;
		}		
	}
}

