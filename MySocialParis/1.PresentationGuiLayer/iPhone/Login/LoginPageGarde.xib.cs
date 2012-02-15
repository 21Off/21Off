using System;
using System.Json;
using System.Net;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using System.Drawing;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Security.Cryptography;
using System.Collections.Specialized;
using System.IO;
using System.Web;
using Share;

namespace MSP.Client
{
	public partial class LoginPageGarde : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public LoginPageGarde (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public LoginPageGarde (NSCoder coder) : base(coder)
		{
		}
		
		private AppDelegateIPhone _AppDel;
		
		public UINavigationController Nav { get; set; }

		public LoginPageGarde (AppDelegateIPhone appDel) : base("LoginPageGarde", null)
		{
			_AppDel = appDel;
		}

		void Initialize ()
		{
			loginBtn.TouchUpInside += HandleLoginBtnTouchUpInside;
			registerBtn.TouchUpInside += HandleRegisterBtnTouchUpInside;
			facebookBtn.TouchDown += HandleFacebookBtnTouchDown;
		}

		void HandleFacebookBtnTouchDown (object sender, EventArgs e)
		{
			FacebookAuth ();
		}

		void HandleRegisterBtnTouchUpInside (object sender, EventArgs e)
		{	
			Nav.PushViewController (new NewAccountViewController (_AppDel, Nav), false);
		}

		void HandleLoginBtnTouchUpInside (object sender, EventArgs e)
		{
			Nav.PushViewController(new AuthentificationViewController(_AppDel, Nav), true);						
		}
		
		private FaceBook.FaceBookApplication facebookApp;		
				
		void FacebookAuth ()
		{ 					
			facebookApp = new FaceBook.FaceBookApplication (this);
			facebookApp.OnExtraLoginComplete += (GraphUser u) =>
			{	
				GraphUser guser = u;//AppDelegateIPhone.AIphone.FacebookServ.GetMyProfile ();
				if (guser != null) {
					NSUserDefaults.StandardUserDefaults.SetDouble ((double)guser.id, "FacebookId");
					
					PostAuth(guser.name, guser.id);
				}				
			};
			
			facebookApp.Login ();
		}
		
		private void PostAuth(string username, decimal userid)
		{
			Action act = () =>
			{		
				try
				{
					User user = _AppDel.UsersServ.AuthentificateFacebook(username, userid);
					if (user == null)
					{
						InvokeOnMainThread(()=>
	                    {
							Util.ShowAlertSheet("Authentification failed", View);
						});
						return;
					}
					if (user.Id == 0)
					{
						InvokeOnMainThread(()=>
	                    {
							Util.ShowAlertSheet("User or password is wrong", View);
						});
						return;						
					}					
					
					string img = string.Format("http://storage.21offserver.com/files/21OffFB.jpg");
					
					if (facebookApp != null)
						facebookApp.Publish("21Off", img, "21Off subscription", img);
					
					User dbUser = Database.Main.Table<User>().Where(el => el.Id == user.Id).FirstOrDefault();
					if (dbUser == null)
						Database.Main.Insert(user);
					else
					{
						Database.Main.Update(dbUser);
					}
					
					LastUserLogged lastUser = Database.Main.Table<LastUserLogged>().LastOrDefault();
					if (lastUser == null || lastUser.Id != user.Id)
						Database.Main.Insert(new LastUserLogged(){ UserId = user.Id });
					
					_AppDel.MainUser = user;
					
					InvokeOnMainThread(()=>
                    {
						Nav.PopViewControllerAnimated(false);
						_AppDel.MainWnd.WillRemoveSubview(Nav.View);
						_AppDel.InitApp();
					});
				}
				catch (Exception ex)
				{
					Util.LogException("Authentification error", ex);
					Util.ShowAlertSheet(ex.Message, View);
					return;
				}
			};
			
			AppDelegateIPhone.ShowRealLoading(View, "Connexion", null, act);			
		}
		
		#endregion	
		
		private UIWebImageView imageVIew;
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
						
			Initialize ();	
			
			var view = new UIView (new RectangleF (0, 40, 320, 1));
			view.Layer.BackgroundColor = UIColor.LightGray.CGColor;
			this.View.AddSubview (view);
			
			//imageVIew = new UIWebImageView(new RectangleF(0, 40, 320, 480 - 40), "Images/21logo.jpg");
			
			UIImage img = Graphics.HighRes ? UIImage.FromBundle ("Images/21logo@2x.jpg") : UIImage.FromBundle ("Images/21logo.jpg");
			imageVIew = new UIWebImageView (new RectangleF (0, 0, 320, 480), img);
			View.Add (imageVIew);			
			View.SendSubviewToBack (imageVIew);
			
			/*
			imageVIew.AnimationImages = new UIImage [] {
				UIImage.FromBundle ("Images/texture"),
				UIImage.FromBundle ("Images/menu-shadow"),			
			};
			imageVIew.AnimationDuration = 5;
			imageVIew.StopAnimating ();
			*/
		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			imageVIew = null;
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			if (imageVIew != null)
				imageVIew.StopAnimating ();
		}
		
		public override void ViewWillAppear (bool animated)
		{
			//imageVIew.StartAnimating ();
		}
	}
	
	
}

