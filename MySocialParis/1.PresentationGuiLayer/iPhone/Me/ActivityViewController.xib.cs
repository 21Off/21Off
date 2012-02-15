using System;
using System.Drawing;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public partial class ActivityViewController : UIViewController
	{
		private ActivityView mediaView;
		
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public ActivityViewController (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public ActivityViewController (NSCoder coder) : base(coder)
		{
			
		}

		public ActivityViewController (MSPNavigationController msp) : base("ActivityViewController", null)
		{
			MSPNavigationController = msp;
		}
				
		#endregion		
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			Initialize();			
		}
		
		private MSPNavigationController MSPNavigationController;
		
		void Initialize ()
		{
			var rootElement = new RootElement ("timelineTitle") { UnevenRows = true };
			var section = new Section();
			rootElement.Add(section);
			
			mediaView = new ActivityView(true);
			mediaView.MSPNavigationController = MSPNavigationController;
			mediaView.Root = rootElement;
			
			int ht = 45;
			var frame = new RectangleF(0, ht, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height - ht - 20);
			mediaView.View.Frame = frame;
			
			this.View.AddSubview(mediaView.View);
			
			var view = new UIView(new RectangleF(0, ht - 5, UIScreen.MainScreen.Bounds.Width, 1));
			view.Layer.BackgroundColor = UIColor.LightGray.CGColor;
			this.View.AddSubview(view);				
		}
	}
}