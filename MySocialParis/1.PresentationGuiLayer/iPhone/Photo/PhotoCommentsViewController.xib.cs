using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.MapKit;
using System.Drawing;
using MonoTouch.Dialog;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public partial class PhotoCommentsViewController : UIViewController
	{		
		private UIViewController _MSP;
		private int imageID;
		private User _photoOwner;
		
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public PhotoCommentsViewController (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public PhotoCommentsViewController (NSCoder coder) : base(coder)
		{
			
		}
		
		public PhotoCommentsViewController (UIViewController msp, int imgId, User photoOwner) : base("PhotoCommentsViewController", null)
		{
			_MSP = msp;
			imageID = imgId;
			_photoOwner = photoOwner;
		}
		
		#endregion
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			var dv = new PhotoCommentsView(true, imageID, _photoOwner);				
			dv.View.Frame = mainView.Frame;

			this.View.WillRemoveSubview(mainView);
			this.View.AddSubview(dv.View);
			
			var view = new UIView(new RectangleF(0, 45 , 320, 1));
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

