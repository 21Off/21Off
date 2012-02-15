using System;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public static class Graphics
	{
		public static UIImage GetImgResource(string resname)
		{
			try
			{
				return UIImage.FromBundle(string.Format("Images/Ver4/{0}", resname));
			}
			catch (Exception ex)
			{
				Util.LogException("GetImgResource", ex);
				return null;
			}
		}

		// Check for multi-tasking as a way to determine if we can probe for the "Scale" property,
		// only available on iOS4 
		public static bool HighRes = UIDevice.CurrentDevice.IsMultitaskingSupported && UIScreen.MainScreen.Scale > 1;

		static Selector sscale;

		static internal void ConfigLayerHighRes (CALayer layer)
		{
			if (!HighRes)
				return;
			
			if (sscale == null)
				sscale = new Selector ("setContentsScale:");
			
			Messaging.void_objc_msgSend_float (layer.Handle, sscale.Handle, 2.0f);
		}	
		
		public static UIImage PrepareForProfileView (UIImage image, int size)
		{
			return GraphicsUtil.PrepareForProfileView(image, size, size);
		}		
		
		public static CALayer MakeBackgroundLayer (UIImage image, RectangleF frame)
		{
			var textureColor = UIColor.FromPatternImage (image);
			
			UIGraphics.BeginImageContext (frame.Size);
			
			var c = UIGraphics.GetCurrentContext ();
			image.DrawAsPatternInRect (frame);
			
			//Images.MenuShadow.Draw (frame);
			var result = UIGraphics.GetImageFromCurrentImageContext ();
			
			UIGraphics.EndImageContext ();
			
			var back = new CALayer { Frame = frame };
			Graphics.ConfigLayerHighRes (back);
			back.Contents = result.CGImage;
			return back;
		}		
	}
}

