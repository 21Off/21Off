using System;
using System.Drawing;
using System.Linq;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using System.Collections.Generic;

namespace MSP.Client
{
	public partial class AuthentificationViewController : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public AuthentificationViewController (IntPtr handle) : base(handle)
		{
			
		}

		[Export("initWithCoder:")]
		public AuthentificationViewController (NSCoder coder) : base(coder)
		{
			
		}
		
		private AppDelegateIPhone _AppDel;
		private UINavigationController _vc;
		private RootElement root;
		
		public AuthentificationViewController (AppDelegateIPhone appDel, UINavigationController vc) 
			: base("AuthentificationViewController", null)
		{
			_AppDel = appDel;
			_vc = vc;
			
			this.root = CreateRoot ();
		}

		void Initialize ()
		{
			okBtn.SetImage(Graphics.GetImgResource("back"), UIControlState.Normal);
			okBtn.TouchUpInside += delegate(object sender, EventArgs e) {
				_vc.PopViewControllerAnimated(true);
			};
		}
		
		void HandleOkBtnTouchUpInside (object sender, EventArgs e)
		{			
			if (string.IsNullOrWhiteSpace(pseudo.Value))
			{
				Util.ShowAlertSheet("Le pseudo est invalide", View);
				return;
			}
			
			if (string.IsNullOrWhiteSpace(pass.Value))
			{
				Util.ShowAlertSheet("Le mot de passe est invalide", View);
				return;
			}
			
			AppDelegateIPhone.ShowRealLoading(View, "Connexion", null, Connect);
		}
		
		private void Connect()
		{
			try
			{
				User user = _AppDel.UsersServ.Authentificate(pseudo.Value, pass.Value);
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
				
				User dbUser = Database.Main.Table<User>().Where(el => el.Name == user.Name).FirstOrDefault();
				if (dbUser == null)
					Database.Main.Insert(user);
				else
				{
					dbUser.Password = pass.Value;
					Database.Main.Update(dbUser);
				}
				
				LastUserLogged lastUser = Database.Main.Table<LastUserLogged>().LastOrDefault();
				if (lastUser == null || lastUser.Id != user.Id)
					Database.Main.Insert(new LastUserLogged(){ UserId = user.Id });
				
				_AppDel.MainUser = user;
				
				InvokeOnMainThread(()=>
                {
					_vc.PopViewControllerAnimated(false);
					_AppDel.MainWnd.WillRemoveSubview(_vc.View);
					_AppDel.InitApp();
				});
			}
			catch (Exception ex)
			{
				Util.LogException("Authentification error", ex);
				Util.ShowAlertSheet(ex.Message, View);
				return;
			}			
		}
		
		#endregion	
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			Initialize ();
			
			var _dialogView = new DialogViewController (root, true);
			_dialogView.TableView.BackgroundColor = UIColor.Clear;
			_dialogView.View.Frame = new RectangleF(0, 40, 320, 480 - 60); // RectangleF(0, 120, 320, 480 - 140);
			this.View.Add(_dialogView.View);
			
			var view = new UIView(new RectangleF(0, 40 , 320, 1));
			view.Layer.BackgroundColor = UIColor.LightGray.CGColor;
			this.View.AddSubview(view);
			
			/*
			var imageVIew = new UIWebImageView(new RectangleF(0, 0, 320, 480), Graphics.GetImgResource("pagedegarde"));
			View.AddSubview(imageVIew);
			View.SendSubviewToBack(imageVIew);
			*/
			
			var list = new List<Element> { pseudo, pass };			
			this.root[0].Insert(0, UITableViewRowAnimation.Bottom, list);
		}
		
		private MyEntryElement pseudo;
		private MyEntryElement pass;
		
		private RootElement CreateRoot ()
		{
			pseudo = new MyEntryElement ("pseudo", null);
			pass = new MyEntryElement ("mot de passe", null, true);
			pass.OnReturn += HandlePassOnReturn;
			
			return new RootElement ("post") {
				new Section (){
					
				}
			};		
		}

		void HandlePassOnReturn (object sender, EventArgs e)
		{
			HandleOkBtnTouchUpInside (sender, e);
		}				
	}
}

