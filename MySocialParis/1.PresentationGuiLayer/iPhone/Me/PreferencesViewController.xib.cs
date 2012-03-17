using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Drawing;
using TweetStation;

namespace MSP.Client
{
	public partial class PreferencesViewController : UIViewController
	{
		#region Constructors
		
		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code
		
		public PreferencesViewController (IntPtr handle) : base (handle)
		{
			
		}
		
		[Export ("initWithCoder:")]
		public PreferencesViewController (NSCoder coder) : base (coder)
		{
			
		}
		
		public PreferencesViewController (UINavigationController nav) : base ("PreferencesViewController", null)
		{
			this.nav = nav;
		}
		
		private UINavigationController nav;
		private DialogViewController _dialogView;
		
		#endregion
				
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			backBtn.TouchDown += HandleBackBtnTouchDown;
			okBtn.TouchDown += HandleOkBtnTouchDown;
			
			Initialize();
		}

		void HandleOkBtnTouchDown (object sender, EventArgs e)
		{
		
		}

		void HandleBackBtnTouchDown (object sender, EventArgs e)
		{
			nav.PopViewControllerAnimated(true);	
		}
		
		void Initialize ()
		{
			var root = CreatePreferencesSection();
			
			var dv = new DialogViewController (root, true);			
			dv.TableView.BackgroundView = new UIImageView(Graphics.GetImgResource("fond"));	
			dv.View.Frame = new RectangleF(0, 45, 320, 480 - 40 - 25);
			
			_dialogView = dv;
			
			this.View.AddSubview(dv.View);
			
			var view = new UIView(new RectangleF(0, 40 , 320, 1));
			view.Layer.BackgroundColor = UIColor.LightGray.CGColor;
			this.View.AddSubview(view);
		}
		
		RootElement CreatePreferencesSection ()
		{
			var email = new EntryElement ("e-mail:", "", "aaaa@mail.com");
			var pseudo = new EntryElement ("username:", "", AppDelegateIPhone.AIphone.MainUser.Name);
			UIFont font = UIFont.FromName ("HelveticaNeue", 12);
			var shareMyPosts = new CustomBooleanElement ("Everybody can share my posts", true, font);
			
			int userId = AppDelegateIPhone.AIphone.MainUser.Id;
			
			UIImage img = ImageStore.GetLocalFullPicture(userId, userId, SizeDB.SizeProfil);			
			var photo = new PhotoElement (img);
			photo.OnExited += ()=>
			{
				_dialogView.View.Frame = new System.Drawing.RectangleF (0, 40, 320, 420);
			};
			
			return new RootElement ("my profile"){
				new Section ("Profile") {
					email,
					pseudo,
					/*
					new BooleanImageElement("test", true, 
						UIImage.FromBundle("Images/Ver4/map"), 
						UIImage.FromBundle("Images/Ver4/admirer"))
					*/
				},
				new Section ("Preferences") {
					shareMyPosts,
				}
			};
		}		
	}
}

