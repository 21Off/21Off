
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TweetStation;
using System.Drawing;

namespace MSP.Client
{
	public partial class PhotoLikersViewController : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public PhotoLikersViewController (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public PhotoLikersViewController (NSCoder coder) : base(coder)
		{
		}

		public PhotoLikersViewController (UIViewController msp, Tweet tweet) : base("PhotoLikersViewController", null)
		{
			_Tweet = tweet;
			_MSP = msp;
		}
		
		private Tweet _Tweet;
		private UIViewController _MSP;
		
		#endregion
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			var frame = new RectangleF(0, 40, 320, 480 - 40 - 25);
			
			var dv = new PhotoLikersView(true, _Tweet.Image, _Tweet.User);
			dv.View.Frame = frame;
			this.View.AddSubview(dv.View);
			
			var view = new UIView(new RectangleF(0, 40 , 320, 1));
			view.Layer.BackgroundColor = UIColor.LightGray.CGColor;
			this.View.AddSubview(view);			
			
			Initialize();
		}	
		
		void Initialize ()
		{
			backBtn.SetImage(Graphics.GetImgResource("back"), UIControlState.Normal);
			this.backBtn.TouchDown += HandleBackBtnhandleTouchDown;
		}		


		void HandleBackBtnhandleTouchDown (object sender, EventArgs e)
		{
			_MSP.DismissModalViewControllerAnimated(true);
		}		
	}
}

