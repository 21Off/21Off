using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using System.Drawing;

namespace MSP.Client
{
	public partial class PhotoKeywordsViewController : UIViewController
	{
		private UIViewController _MSP;
		private int imageID;
		private User _photoOwner;
		
		#region Constructors
		
		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code
		
		public PhotoKeywordsViewController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		[Export ("initWithCoder:")]
		public PhotoKeywordsViewController (NSCoder coder) : base (coder)
		{
	
		}
		
		public PhotoKeywordsViewController (UIViewController msp, int imgId, User photoOwner) : base ("PhotoKeywordsViewController", null)
		{
			_MSP = msp;
			imageID = imgId;
			_photoOwner = photoOwner;
		}
		
		#endregion
		
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			var dv = new PhotoKeywordsView(true, imageID, _photoOwner);				
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

