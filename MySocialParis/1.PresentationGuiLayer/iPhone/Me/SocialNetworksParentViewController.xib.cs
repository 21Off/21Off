using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using Share;

namespace MSP.Client
{
	public partial class SocialNetworksParentViewController : UIViewController
	{			
		protected FaceBook.FaceBookApplication facebookApp;
		
		protected Twitter.TwitterApplication twitterApp;
		
		#region Constructors
		
		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code
		
		public SocialNetworksParentViewController (IntPtr handle) : base (handle)
		{
			Initialize ();
		}
		
		[Export ("initWithCoder:")]
		public SocialNetworksParentViewController (NSCoder coder) : base (coder)
		{
			Initialize ();
		}
		
		private UINavigationController msp;
		
		public SocialNetworksParentViewController (UINavigationController msp) : base ("SocialNetworksParentViewController", null)
		{
			this.msp = msp;
			
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
			okBtn.TouchDown+= HandleOkBtnTouchDown;
			
			// Facebook
			facebookApp = new FaceBook.FaceBookApplication(this);
			/*
			facebookApp.OnLoginComplete+= ()=>
			{
				GraphUser guser = AppDelegateIPhone.AIphone.FacebookServ.GetMyProfile();
				if (guser != null)
				{
					NSUserDefaults.StandardUserDefaults.SetDouble((double)guser.id,"FacebookId");
					var id = AppDelegateIPhone.AIphone.MainUser.Id;
					AppDelegateIPhone.AIphone.UsersServ.UpdateSocialdId(guser.id, id, 1);
				}
			};
			*/
			facebookApp.OnExtraLoginComplete+= (GraphUser u)=>
			{
				GraphUser guser = u;//AppDelegateIPhone.AIphone.FacebookServ.GetMyProfile();
				if (guser != null)
				{
					NSUserDefaults.StandardUserDefaults.SetDouble((double)guser.id,"FacebookId");
					var id = AppDelegateIPhone.AIphone.MainUser.Id;
					AppDelegateIPhone.AIphone.UsersServ.UpdateSocialdId(guser.id, id, 1);
				}
			};			
			
			SwitchFacebook.ValueChanged += delegate {
				if (SwitchFacebook.On)
				{
					facebookApp.Login();					
				}
				else
				{
					facebookApp.Logout();
				}
			};
			
			// Twitter			
			twitterApp = new Twitter.TwitterApplication(this);			
			SwitchTwitter.ValueChanged += delegate {
				if (SwitchTwitter.On)
				{
					twitterApp.Login();
				}
				else
				{
					twitterApp.Logout();
				}
			};
			
			var view = new UIView(new RectangleF(0, 40 , 320, 1));
			view.Layer.BackgroundColor = UIColor.LightGray.CGColor;
			this.View.AddSubview(view);
		}

		void HandleOkBtnTouchDown (object sender, EventArgs e)
		{
			msp.PopViewControllerAnimated(false);
		}

		void HandleBackBtnBackBarButtonItemClicked (object sender, EventArgs e)
		{
			msp.PopViewControllerAnimated(false);
		}
		
		public override void ViewDidAppear(bool animated) 
		{
			SwitchTwitter.On = twitterApp.LoggedIn();
			SwitchFacebook.On = facebookApp.LoggedIn();
			
			base.ViewDidAppear(animated);
			
			Util.Defaults.SetInt (SwitchTwitter.On ? 1 : 0, "twitterOption");
			Util.Defaults.SetInt (SwitchFacebook.On ? 1 : 0, "facebookOption");
			
			Util.Defaults.SetInt (SwitchTwitter.On ? 0 : 1, "twitter");
			Util.Defaults.SetInt (SwitchFacebook.On ? 0 : 1, "facebook");
			Util.Defaults.Synchronize();
		}
		
		public UISwitch SwitchFacebook
		{
			get { return switchFacebook; }
		}
	
		
		public UISwitch SwitchTwitter
		{
			get { return switchTwitter; }
		}
	}
}

