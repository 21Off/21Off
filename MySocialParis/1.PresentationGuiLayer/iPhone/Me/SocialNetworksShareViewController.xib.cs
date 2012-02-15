using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public partial class SocialNetworksShareViewController : UIViewController
	{
		#region Members
		
		protected FaceBook.FaceBookApplication facebookApp;
		
		protected Twitter.TwitterApplication twitterApp;
		
		protected Image image;
		
		protected string imageUrl;

		#endregion
		
		#region Constructors
		
		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code
		
		public SocialNetworksShareViewController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		[Export ("initWithCoder:")]
		public SocialNetworksShareViewController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		private UINavigationController msp;
		
		public SocialNetworksShareViewController (UINavigationController msp, Image image) : base ("SocialNetworksShareViewController", null)
		{
			this.msp = msp;
			this.image = image;
			imageUrl = string.Format("http://storage.21offserver.com/files/{0}/{1}.jpg", image.UserId, image.Id);
			
			Initialize ();
		}
		
		void Initialize ()
		{
		}
		
		#endregion
		
		public override void ViewDidLoad() 
		{
			base.ViewDidLoad();
			
			backBtn.TouchDown += HandleBackBtnBackBarButtonItemClicked;
/*
			UIGraphics.BeginImageContext(facebookBtn.Frame.Size);
			CGContext context = UIGraphics.GetCurrentContext();
			context.SetFillColor(0.0f,1.0f,0.0f,1.0f);
			context.FillRect(facebookBtn.Frame);
			UIImage image = UIGraphics.GetImageFromCurrentImageContext();
			UIGraphics.EndImageContext();
			FacebookBtn.SetBackgroundImage(image,UIControlState.Normal);
*/
			// Facebook
			facebookApp = new FaceBook.FaceBookApplication(this);			

			facebookBtn.TouchDown += delegate {
				if (facebookApp.LoggedIn())
				{										
					facebookApp.Publish("21Off", imageUrl, image.Name ?? "No comment", imageUrl);
					AppDelegateIPhone.ShowMessage(View, "Posted on facebook", null, 2);
				}
				else
				{
					var _MSP = AppDelegateIPhone.tabBarController.SelectedViewController as UINavigationController;
					var vc = new SocialNetworksParentViewController(_MSP);
					_MSP.PushViewController(vc, true);
				}
			};
						
			// Twitter			
			twitterApp = new Twitter.TwitterApplication(this);
			
			TwitterBtn.TouchDown += delegate {
				if (twitterApp.LoggedIn())
				{					
					twitterApp.Publish("21Off", imageUrl, image.Name ?? "No comment" , imageUrl);
					AppDelegateIPhone.ShowMessage(View, "Posted on twitter", null, 2);
				}
				else
				{
					var _MSP = AppDelegateIPhone.tabBarController.SelectedViewController as UINavigationController;
					var vc = new SocialNetworksParentViewController(_MSP);
					_MSP.PushViewController(vc, true);
				}
			};
			
			// 21Off
			if (this.image.UserId == AppDelegateIPhone.AIphone.MainUser.Id)
			{
				SelfshareLbl.Hidden = SelfshareBtn.Hidden = selfShareSubLbl.Hidden = true;
			}
			else
			{
				SelfshareBtn.TouchDown += delegate
				{
					Action act  = ()=>
					{
						AppDelegateIPhone.AIphone.ShareServ.PhotoShare(image);
					};
					
					AppDelegateIPhone.ShowRealLoading(null, "Sharing post", null, act);
				};
			}
		}

		void HandleBackBtnBackBarButtonItemClicked (object sender, EventArgs e)
		{
			msp.PopViewControllerAnimated(true);
		}
				
		public UIButton FacebookBtn
		{
			get { return facebookBtn; }
		}
	
		
		public UIButton TwitterBtn
		{
			get { return twitterBtn; }
		}
		
		public UILabel SelfshareLbl
		{
			get { return selfshareLbl; }
		}
		
		public UIButton SelfshareBtn
		{
			get { return selfshareBtn; }
		}
		
	}
}

