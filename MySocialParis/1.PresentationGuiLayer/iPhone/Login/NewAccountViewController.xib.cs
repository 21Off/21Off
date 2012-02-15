using System;
using System.Drawing;
using System.IO;
using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;

namespace MSP.Client
{
	public partial class NewAccountViewController : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public NewAccountViewController (IntPtr handle) : base(handle)
		{
			
		}

		[Export("initWithCoder:")]
		public NewAccountViewController (NSCoder coder) : base(coder)
		{
			
		}

		private AppDelegateIPhone _AppDel;
		private UINavigationController _vc;
		private RootElement root;

		public NewAccountViewController (AppDelegateIPhone appDel, UINavigationController vc) : base("NewAccountViewController", null)
		{
			_AppDel = appDel;
			_vc = vc;
			
			this.root = CreateRoot ();
		}

		void Initialize ()
		{
			okBtn.SetImage(Graphics.GetImgResource("ok"), UIControlState.Normal);
			backBtn.SetImage(Graphics.GetImgResource("back"), UIControlState.Normal);
			
			Graphics.ConfigLayerHighRes(okBtn.Layer);
			Graphics.ConfigLayerHighRes(backBtn.Layer);
			
			okBtn.TouchUpInside += HandleOkBtnTouchUpInside;
			backBtn.TouchUpInside += HandleBackBtnTouchUpInside;
		}

		void HandleBackBtnTouchUpInside (object sender, EventArgs e)
		{
			_vc.PopViewControllerAnimated (true);
		}
		
		private Size maxSizeProfile = new Size(150, 150);

		void HandleOkBtnTouchUpInside (object sender, EventArgs e)
		{
			var documentsDirectory = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			string png = null;
			
			if (photo.Value != null && photo.Value != emptyImage)
			{				
				png = Path.Combine (documentsDirectory, "Profile.jpg");				
				//var imgP = UIImageUtils.ScaleToFit(photo.Value, new SizeF(maxSizeProfile.Width, maxSizeProfile.Height));
								
				UIImage img = Graphics.PrepareForProfileView(photo.Value, maxSizeProfile.Width);				
				NSData imgData = img.AsJPEG (0.8f);
				NSError err = null;
				if (imgData.Save (png, false, out err)) 
				{
					
				}
				else
					return;
			}			
			
			if (string.IsNullOrWhiteSpace (pseudo.Value)) {
				Util.ShowAlertSheet ("Le pseudo est invalide", View);
				return;
			}
			
			if (string.IsNullOrWhiteSpace (pass.Value)) {
				Util.ShowAlertSheet ("Le mot de passe est invalide", View);
				return;
			}
			
			if (string.IsNullOrWhiteSpace (email.Value)) {
				Util.ShowAlertSheet ("L'e-mail est invalide", View);
				return;
			}
			
			Action act = ()=>
			{			
				User user = null;
				try {
					user = _AppDel.UsersServ.CreateUser (pseudo.Value, pass.Value, email.Value, png);
				} catch (Exception ex) {
					Util.ShowAlertSheet ("La creation de l'utilisateur a echouée: " + ex.Message, View);
					return;
				}
				
				User dbUser = Database.Main.Table<User>().Where(el => el.Name == user.Name).FirstOrDefault();
				if (dbUser == null)
					Database.Main.Insert(user);
				
				LastUserLogged lastUser = Database.Main.Table<LastUserLogged>().LastOrDefault();
				if (lastUser == null || lastUser.Id != user.Id)
					Database.Main.Insert(new LastUserLogged(){ UserId = user.Id });
				
				_AppDel.MainUser = user;
				
				InvokeOnMainThread(()=>
                {
					_vc.PopViewControllerAnimated (true);
					_AppDel.MainWnd.WillRemoveSubview (_vc.View);
					_AppDel.InitApp ();
				});
			};
			AppDelegateIPhone.ShowRealLoading(View, "Creating user", null, act);			
		}

		#endregion

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			Initialize ();
			
			_dialogView = new DialogViewController (root, true);									
			_dialogView.TableView.BackgroundColor = UIColor.Clear;
			_dialogView.View.Frame = new System.Drawing.RectangleF(0, 40, 320, 480 - 60);
			this.View.Add (_dialogView.View);
			
			lineView = new UIView(new RectangleF(0, 40 , 320, 1));
			lineView.Tag = 1;
			lineView.Layer.BackgroundColor = UIColor.LightGray.CGColor;
			this.View.AddSubview(lineView);
						
			UIImage img = Graphics.HighRes ? UIImage.FromBundle("Images/21logo@2x.jpg") : UIImage.FromBundle("Images/21logo.jpg");
			var imageVIew = new UIWebImageView(new RectangleF(0, 0, 320, 480), img);
			View.AddSubview(imageVIew);
			View.SendSubviewToBack(imageVIew);			
		}
		
		private UIView lineView;
		
		private DialogViewController _dialogView;
		private MyEntryElement email;
		private MyEntryElement pseudo;
		private MyEntryElement pass;
		private PhotoElement photo;
		
		private UIImage emptyImage;

		RootElement CreateRoot ()
		{
			email = new MyEntryElement ("e-mail", null);
			pseudo = new MyEntryElement ("username", null);
			pass = new MyEntryElement ("password", null, true);
			emptyImage = UIImage.FromBundle("Images/Login/emptyProfile.png");
			
			/*
			var ise = new PhotoStringElement("image profile", 
				UIImage.FromBundle("Images/Login/emptyProfile.png").resizeImage(new SizeF(30, 30)));
			
			ise.OnExited += ()=>
			{
				_dialogView.View.Frame = new System.Drawing.RectangleF (0, 40, 320, 420);
			};
			*/
			
			photo = new PhotoElement (emptyImage);//.resizeImage(new SizeF(48, 43)));
			photo.OnEntered+= ()=>
			{
				lineView.Hidden = true;
			};
			photo.OnExited += ()=>
			{
				_dialogView.View.Frame = new System.Drawing.RectangleF (0, 40, 320, 420);
				lineView.Hidden = false;
			};
			
			var root = new RootElement ("post") { new Section { email, pseudo, pass }, 
				new Section { photo }
			  } ;
						
			return root;
		}
	}
}

