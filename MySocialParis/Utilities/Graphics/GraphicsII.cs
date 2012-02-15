using System;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public class GraphicsII
	{
		public static UIImage AdjustImage (RectangleF rect, UIImage template, CGBlendMode mode, UIColor color)
		{
			float red = new float ();
			float green = new float ();
			float blue = new float ();
			float alpha = new float ();
			if (color == null)
				color = UIColor.FromRGB (100, 0, 0);
			
			color.GetRGBA (out red, out green, out blue, out alpha);
			return AdjustImage (rect, template, mode, red, green, blue, alpha);
		}
		
		public static UIImage AdjustImage (RectangleF rect, UIImage template, CGBlendMode mode, 
		                                   float red, float green, float blue, float alpha)
		{
			using (var cs = CGColorSpace.CreateDeviceRGB ()) {
				using (var context = new CGBitmapContext (IntPtr.Zero, (int)rect.Width, (int)rect.Height, 8, 
				                                          (int)rect.Height * 4, cs, CGImageAlphaInfo.PremultipliedLast)) {
					
					context.TranslateCTM (0.0f, 0f);
					//context.ScaleCTM(1.0f,-1.0f);
					context.DrawImage (rect, template.CGImage);
					context.SetBlendMode (mode);
					context.ClipToMask (rect, template.CGImage);
					context.SetFillColor (red, green, blue, alpha);
					context.FillRect (rect);
					
					return UIImage.FromImage (context.ToImage ());
				}
			}
		}

		public static UIImage ResizeImage (SizeF size, UIImage image, bool KeepRatio)
		{
			var curSize = image.Size;
			SizeF newSize;
			if (KeepRatio) {
				var ratio = Math.Min (size.Width / curSize.Width, size.Height / curSize.Height);
				newSize = new SizeF (curSize.Width * ratio, curSize.Height * ratio);
			
			} else {
				newSize = size;
			}
			
			return image.Scale (newSize);		
		}

		// Check for multi-tasking as a way to determine if we can probe for the "Scale" property,
		// only available on iOS4 
		public static bool HighRes = UIDevice.CurrentDevice.IsMultitaskingSupported && UIScreen.MainScreen.Scale > 1;

		// Child proof the image by rounding the edges of the image
		public static UIImage RemoveSharpEdges (UIImage image)
		{
			if (image == null)
				throw new ArgumentNullException ("image");
			
			return RemoveSharpEdges(image, image.Size.Height, 4);
		}
		
		public static UIImage Scale (UIImage image, SizeF size)
		{
			UIGraphics.BeginImageContext (size);
			image.Draw (new RectangleF (new PointF (0, 0), size));
			var ret = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();
			return ret;
		}		
		
		public static UIImage Scale (UIImage source, int dimx, int dimy)
		{
			UIGraphics.BeginImageContext (new SizeF (dimx, dimy));
			var ctx = UIGraphics.GetCurrentContext ();
			
			var img = source.CGImage;
			ctx.TranslateCTM (0, dimy);
			if (img.Width > img.Height)
				ctx.ScaleCTM (1, -img.Width / dimy);
			else
				ctx.ScaleCTM (img.Height / dimx, -1);
		
			var rect = new RectangleF (0, 0, dimx, dimy);	
			ctx.DrawImage (rect, source.CGImage);
			
			var ret = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();
			return ret;
		}
		
		public static UIImage RemoveSharpEdges (UIImage image, float width, int radius)  
		{  
		    UIGraphics.BeginImageContext (new SizeF (width, width)); 
			//UIGraphics.BeginImageContextWithOptions(new SizeF (width, width), true, 0.0f);
		    var c = UIGraphics.GetCurrentContext ();  

			c.AddPath(MakeRoundedPath(width, radius)); 
			c.Clip();
		      
		    //image.Draw (new PointF (0, 0));
			image.Draw (new RectangleF (0, 0, width, width));
		    var converted = UIGraphics.GetImageFromCurrentImageContext ();  
		    UIGraphics.EndImageContext ();  
		    return converted;  
		}

		static internal CGPath MakeRoundedPath (float size, int radius)
		{
			float hsize = size / 2;
			
			var path = new CGPath ();
			path.MoveToPoint (size, hsize);
			path.AddArcToPoint (size, size, hsize, size, radius);
			path.AddArcToPoint (0, size, 0, hsize, radius);
			path.AddArcToPoint (0, 0, hsize, 0, radius);
			path.AddArcToPoint (size, 0, size, hsize, radius);
			path.CloseSubpath ();
			
			return path;
		}
	}
}
