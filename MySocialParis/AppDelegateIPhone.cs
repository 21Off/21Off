using System;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;
using System.Threading;
using System.Collections.Generic;
using MonoTouch.Dialog.Utilities;
using System.Net;

namespace MSP.Client
{	
	//[Register("AppDelegate")]
	// The name AppDelegateIPhone is referenced in the MainWindowIPhone.xib file.
	public partial class AppDelegateIPhone : UIApplicationDelegate
	{
		public UIWindow MainWnd { get {return window;} }
		public User MainUser {get;set;}
		public static AppDelegateIPhone AIphone {get;set;}
		
		public int GetMainUserId()
		{
			if (MainUser != null)
				return MainUser.Id;
			return -1;
		}
		
		// This method is invoked when the application has loaded its UI and its ready to run
		public override bool FinishedLaunching (UIApplication app, NSDictionary options)
		{
			//ThreadPool.QueueUserWorkItem(el => UIImageUtils.AddImagesToSimulator());
			AIphone = this;
			ImageLoader.DefaultLoader = new ImageLoader(200, 4194304 * 5);
			
			DBActivServ = new DBActiviesService();
			
			FacebookServ = new FacebookService();
			UsersServ = new UsersService();
			ImgServ = new ImagesService();
			FlwServ = new FollowersService();
			KeywServ = new KeywordsService();
			LikesServ = new LikesService();
			CommentServ = new CommentsService();
			ActivServ = new ActivitiesService();
			ReportServ = new ReportService();
			ShareServ = new ShareService();
			InstagramServ = new InstagramService();
			
			InitializeWindow ();
			
//			ServicePointManager.ServerCertificateValidationCallback = (sender, cert, chain, ssl) => 
//			{
//				return true;
//			};
			
			var mainDB = Database.Main;
			try
			{
				LastUserLogged lastUser = Database.Main.Table<LastUserLogged>().LastOrDefault();
				if (lastUser != null)
				{
					MainUser = mainDB.Get<User>(lastUser.UserId);
				}
				
				if (MainUser != null)
				{
					_welcomePage = new UIImageView(Graphics.GetImgResource("pagedegarde"));
					_welcomePage.Tag = 666;
					window.AddSubview(_welcomePage);
					
					ShowRealLoading(window, "Logging in ...", "", 
					()=>
					{											
						//InstagramServ.GetPopular();
						AuthSequence(MainUser);
					});
				}
				else
				{					
					InitLoginPage();
				}
			}
			catch (Exception ex)
			{
				Util.LogException("FinishedLaunching", ex);
			}
						
			window.MakeKeyAndVisible ();			
			
			return true;
		}
		
		private UIImageView _welcomePage;
		private UINavigationController _currentNavControler;
		
		private void AuthSequence(User user)
		{
			User authUser = UsersServ.Authentificate(user.Name, user.Password);
			if (authUser == null || user.Id == 0)
			{
				MainUser = null;
				InvokeOnMainThread(()=>
				{
					InitLoginPage();
					Util.ShowAlertSheet("Authentification failed", window);
				});
				
				return;
			}
			
			if (_welcomePage != null)
			{
				_welcomePage.Image = null;
				_welcomePage.RemoveFromSuperview();
				_welcomePage = null;
			}
			
			InvokeOnMainThread(InitApp);
		}
		
		public void Logout()
		{
		 	MainWnd.WillRemoveSubview(tabBarController.View);
			
			buzzNavigationController = null;
			shareNavigationController = null;
			meNavigationController = null;
			aroundNavigationController = null;
			
			tabBarController.SetViewControllers(new UIViewController[0], true);
			tabBarController = null;
		}
		
		public void InitLoginPage()
		{
			var welcomePage = new WelcomePage(this);			
			var nav = new UINavigationController(welcomePage) { NavigationBarHidden = true, };
			_currentNavControler = nav;
			welcomePage.Nav = nav;										
			nav.Add(welcomePage.View);			
			window.AddSubview(nav.View);
			
			if (_welcomePage != null)
				window.WillRemoveSubview(_welcomePage);
		}			
		
		public UINavigationController GetCurrentNavControler()
		{
			return _currentNavControler;
		}
		
		public override void DidEnterBackground (UIApplication application)
		{
			ImageStore.Purge();
			ImageLoader.Purge();
			NSUrlConnectionWrapper.KillAllConnections();
            System.GC.Collect();
		}
		
		public override void WillEnterForeground (UIApplication application)
		{
		}		
		
        /// <summary>
        /// GC is nessessary due to periodical memory alloc/free in camera capturing class.
        /// </summary>        
        public override void ReceiveMemoryWarning(UIApplication application)
        {
			ImageStore.Purge();
            System.GC.Collect();
        }		
	}
}

