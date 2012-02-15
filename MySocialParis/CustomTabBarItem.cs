using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace MSP.Client
{
	public partial class CustomTabBarItem : UITabBarItem	
	{
		public CustomTabBarItem () : base()
		{
		}
		
		public CustomTabBarItem (string title, UIImage image, int tag) : base (title, image, tag)
		{
		}
			
		
		public virtual UIImage customHighlightedImage { get; set; }
	
		public UIImage selectedImage()
		{
		    return this.customHighlightedImage;
		}

		public UIImage unselectedImage()
		{
		    return this.Image;
			
		}
		
	}
}

