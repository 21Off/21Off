using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace MSP.Client
{
	// Base type probably should be MonoTouch.UIKit.UINavigationBar or subclass
	[MonoTouch.Foundation.Register("CustomNavigationBar")]
	public partial class CustomNavigationBar {
	}	
	
	public partial class CustomNavigationBar : UINavigationBar
	{

		//Die Standardkonstrutoren einfach durchgeben
		public CustomNavigationBar () : base()
		{

		}

		public CustomNavigationBar (NSCoder coder) : base(coder)
		{

		}

		public CustomNavigationBar (IntPtr ptr) : base(ptr)
		{					
			this.SetItems(new UINavigationItem[2]{ new MyNavigationItem("aaaa"), new MyNavigationItem("bbb"), }, true);
		}
		

		public CustomNavigationBar (NSObjectFlag t) : base(t)
		{

		}

		public CustomNavigationBar (RectangleF frame) : base(frame)
		{

		}
		
		/*
		public override void Draw (RectangleF rect)
		{			
			CGContext context = UIGraphics.GetCurrentContext ();
			
			base.Draw (rect);
			
			UIColor.White.SetFill();
			context.FillRect(new RectangleF(0, 0, 320, 20));							
			
			//Background zeichnen
			//UIImage image = UIImage.FromFile("Images/blueArrow.png");
			//image.Draw(rect);
		}
		*/
	}	
}
