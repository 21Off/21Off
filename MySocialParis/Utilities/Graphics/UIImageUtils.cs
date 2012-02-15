using System;
using System.Drawing;
using System.IO;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public static class ImageExtensions
	{
		public static UIImage resizeImage(this UIImage t, SizeF size)
		{
			return UIImageUtils.resizeImage(t, size);
		}
	}
	
	public static class UIImageUtils
	{
		public static UIImage makeProgressImage(int width, int height, float progress, CGColor baseColor, CGColor topColor)
		{
		   //Create a CGBitmapContext object
		   CGBitmapContext ctx = new CGBitmapContext(IntPtr.Zero, width, height, 8, 4 * width, CGColorSpace.CreateDeviceRGB(), CGImageAlphaInfo.PremultipliedFirst);
		
		   //Draw a rectangle with the base color
		   ctx.SetFillColor(baseColor);
		   ctx.FillRect(new RectangleF(0,0, width, height));
		
		   //Calculate the width of the 2nd rectangle based on the progress
		   float percentWidth = width * progress;
		
		   //Draw the second rectangle with the top color
		   ctx.SetFillColor(topColor);
		   ctx.FillRect(new RectangleF(0, 0, percentWidth, height));
		
		   //return a UIImage object
		   return UIImage.FromImage(ctx.ToImage());
		}		
		
		public static UIImage makeGradientProgressImage(int width, int height, float progress, float[] components, float[] locations)
		{
		   //Create a CGBitmapContext object
		   CGBitmapContext ctx = new CGBitmapContext(IntPtr.Zero, width, height, 8, 4 * width, CGColorSpace.CreateDeviceRGB(), CGImageAlphaInfo.PremultipliedFirst);
		
		   //Calculate the width of the rectangle based on the progress
		   float percentWidth = width * progress;
		
		   //Create a gradient object
		   CGGradient gradient = new CGGradient(CGColorSpace.CreateDeviceRGB(), components, locations);
		
		   //Draw a linear gradient to represent the progress bar
		   ctx.DrawLinearGradient(gradient, new PointF(0, 0), new PointF(percentWidth, 0), CGGradientDrawingOptions.DrawsBeforeStartLocation);
		
		   //return a UIImage object
		   return UIImage.FromImage(ctx.ToImage());
		}		
		
		public static UIImage ScaleToFit (UIImage sourceImage, System.Drawing.SizeF targetSize)
		{
			UIImage newImage;
			
			SizeF imageSize = sourceImage.Size;
			float width = imageSize.Width;
			float height = imageSize.Height;
			
			float targetWidth = targetSize.Width;
			float targetHeight = targetSize.Height;
			
			float scaleFactor = 0f;
			float scaledWidth = targetWidth;
			float scaledHeight = targetHeight;
			
			PointF thumbnailPoint = new PointF (0, 0);
			
			if (imageSize != targetSize) {
				float widthFactor = targetWidth / width;
				float heightFactor = targetHeight / height;
				
				if (widthFactor < heightFactor)
					scaleFactor = widthFactor;
				else
					scaleFactor = heightFactor;
				
				scaledWidth = width * scaleFactor;
				scaledHeight = height * scaleFactor;
				
				// center the image
				
				if (widthFactor < heightFactor) {
					thumbnailPoint.Y = (targetHeight - scaledHeight) * 0.5f;
				} else if (widthFactor > heightFactor) {
					thumbnailPoint.X = (targetWidth - scaledWidth) * 0.5f;
				}
			}
			
			RectangleF thumbnailRect = new RectangleF (new PointF (0, 0), new SizeF (scaledWidth, scaledHeight));
			
			UIGraphics.BeginImageContext (thumbnailRect.Size);
			
			sourceImage.Draw (thumbnailRect);
			
			newImage = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();
			
			return newImage;
		}
		
		public static UIImage ImageToFitSize (this UIImage image, SizeF fitSize)
		{
			double imageScaleFactor = 1.0;
			imageScaleFactor = image.CurrentScale;
			
			double sourceWidth = image.Size.Width * imageScaleFactor;
			double sourceHeight = image.Size.Height * imageScaleFactor;
			double targetWidth = fitSize.Width;
			double targetHeight = fitSize.Height;
			
			double sourceRatio = sourceWidth / sourceHeight;
			double targetRatio = targetWidth / targetHeight;
			
			bool scaleWidth = (sourceRatio <= targetRatio);
			scaleWidth = !scaleWidth;
			
			double scalingFactor, scaledWidth, scaledHeight;
			
			if (scaleWidth) {
				scalingFactor = 1.0 / sourceRatio;
				scaledWidth = targetWidth;
				scaledHeight = Math.Round (targetWidth * scalingFactor);
			} else {
				scalingFactor = sourceRatio;
				scaledWidth = Math.Round (targetHeight * scalingFactor);
				scaledHeight = targetHeight;
			}
			
			RectangleF destRect = new RectangleF (0, 0, (float)scaledWidth, (float)scaledHeight);
			
			UIGraphics.BeginImageContextWithOptions (destRect.Size, false, 0.0f);
			image.Draw (destRect);
			UIImage newImage = UIGraphics.GetImageFromCurrentImageContext ();
			UIGraphics.EndImageContext ();
			
			return newImage;
		}		

		public static void AddImagesToSimulator ()
		{
			var s = "/Users/Valeriu/Desktop/photostest";
			foreach (string eachImage in Directory.GetFiles (s, "*.jpg")) {
				using (UIImage imageToAdd = UIImage.FromFile (eachImage)) {
					imageToAdd.SaveToPhotosAlbum (new UIImage.SaveStatus (delegate(UIImage image, NSError error) {
						
						if (error != null) {
							
							Console.Out.WriteLine ("Error saving image {0}", eachImage);
							
						}
					}));
				}
			}
		}

		public static UIImage FromFile (string filename, SizeF fitSize)
		{
			var imageFile = UIImage.FromFile (filename);
			
			return imageFile.ImageToFitSize (fitSize);
		}


		public static UIImage GetPreview (string fileName)
		{
			return GetPreview (fileName, new SizeF (100, 100));
		}
		
		public static UIImage GetPreview (string fileName, SizeF size)
		{
			return resizeImage (UIImage.FromFileUncached (fileName).StretchableImage((int)size.Width, (int)size.Height), size);
		}
		
		public static UIImage overlayImageWithPolaroid (UIImage image, UIImage overlay, RectangleF imageRect, 
		                                                RectangleF overlayRect, SizeF size)
		{
			RectangleF imageRectAdjusted = imageRect;
			//imageRectAdjusted.Width -= 5f;
			//imageRectAdjusted.Y += 5f;
			UIImage img;
			img = overlayImage (image,overlay,imageRectAdjusted,overlayRect,size);
			return img;
		}
		
		static void BeginImageContext (SizeF size)
		{
			UIGraphics.BeginImageContextWithOptions (size, true, 0);
			return;
			
			if (Graphics.HighRes)
				UIGraphics.BeginImageContextWithOptions (size, true, 0);
			else
				UIGraphics.BeginImageContext (size);
		}		
		
		public static UIImage resizeImage(UIImage image, SizeF size)
		{
			BeginImageContext(size);
			//UIGraphics.BeginImageContext (size);
			
			CGContext context = UIGraphics.GetCurrentContext(); 						
			
			context.TranslateCTM(0.0f,size.Height);
			context.ScaleCTM(1.0f, -1.0f);
			context.DrawImage(new RectangleF(0,0,size.Width,size.Height),image.CGImage);			
			
			UIImage resizedImage = UIGraphics.GetImageFromCurrentImageContext();
			
			UIGraphics.EndImageContext();
			return resizedImage;
		}
		
		public static UIImage MakeEmpty (Size size)
		{
			using (var cs = CGColorSpace.CreateDeviceRGB ()) {
				using (var bit = new CGBitmapContext (IntPtr.Zero, size.Width, size.Height, 8, 0, cs, CGImageAlphaInfo.PremultipliedFirst)) {
					bit.SetStrokeColor (0, 0, 0, 0);
					bit.SetFillColor (1, 1, 1, 1);
					bit.FillRect (new RectangleF (0, 0, size.Width, size.Height));
					
					return UIImage.FromImage (bit.ToImage ());
				}
			}
		}		
		
		public static UIImage drawEmptyImage(SizeF size)
		{
			using (CGBitmapContext context = createBitmapOfSize (size))
			{			
				context.SetFillColor (1, 1, 1, 1);
				context.FillRect (new RectangleF(new PointF(0, 0), size));
	
				context.SetAllowsAntialiasing (true);
				context.SetShouldAntialias (true);
				
				return UIImage.FromImage (context.ToImage ());
			}
		}
		
		public static UIImage MakeEmptyWithGradient(SizeF size, float[] colors)
		{			
			using (var rgb = CGColorSpace.CreateDeviceRGB ())
			{
				var gradient = new CGGradient (rgb, colors, null);
				
				RectangleF rect = new RectangleF(new PointF(0, 0), size);
				
				using (rgb)
				{
					var start = new PointF(rect.Location.X, rect.Location.Y);
					var end = new PointF(rect.Location.X, rect.Location.Y + 1);
				
				   //Create a CGBitmapContext object
				   	CGBitmapContext ctx = createBitmapOfSize(size);
					ctx.ClipToRect (rect);
					ctx.DrawLinearGradient (gradient, start, end, CGGradientDrawingOptions.DrawsBeforeStartLocation 
					                        | CGGradientDrawingOptions.DrawsAfterEndLocation);
				
				   //return a UIImage object
				   return UIImage.FromImage(ctx.ToImage());	
				}
			}
		}		
		
		// Adapted from... http://www.realdevelopers.com/blog/?p=415
		public static UIImage overlayImage (UIImage image, UIImage overlay, RectangleF imageRect, RectangleF overlayRect, SizeF size)
		{			
			RectangleF imageBoundingBox = imageRect;
			RectangleF overlayBoundingBox = overlayRect;
						
			CGBitmapContext context = createBitmapOfSize (size);
			
			//context.SetRGBFillColor (1, 1, 1, 1);
			//context.FillRect (imageBoundingBox);			
			//context.ClearRect (imageBoundingBox);
			
			context.DrawImage (imageBoundingBox, image.CGImage);
			context.DrawImage (overlayBoundingBox, overlay.CGImage);
			
			context.SetAllowsAntialiasing (true);
			context.SetShouldAntialias (true);
			
			UIImage result = UIImage.FromImage (context.ToImage ());
			return result;
		}
		
		// Adapted from... http://www.realdevelopers.com/blog/?p=415
		private static CGBitmapContext createBitmapOfSize (SizeF size)
		{
			int bitmapBytesPerRow = (int)(size.Width * 4);
			CGColorSpace colorspace = CGColorSpace.CreateDeviceRGB ();
			
			CGBitmapContext context = new CGBitmapContext (
					IntPtr.Zero,
					(int)size.Width,
					(int)size.Height,
					8,
					bitmapBytesPerRow,
					colorspace,
					CGImageAlphaInfo.PremultipliedLast
				);
			
			context.SetAllowsAntialiasing (true);
			context.SetShouldAntialias (true);
			
			return context;
		}		
	}
	
}


