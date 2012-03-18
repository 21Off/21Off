using System;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;
using System.Threading;
using System.Collections.Generic;
using MonoTouch.Dialog.Utilities;

namespace MSP.Client
{	
	//[Register("AppDelegate")]
	// The name AppDelegateIPhone is referenced in the MainWindowIPhone.xib file.
	public partial class AppDelegateIPhone : UIApplicationDelegate
	{
		public UIWindow MainWnd { get {return window;} }
		public User MainUser {get;set;}
		public static AppDelegateIPhone AIphone {get;set;}
		
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
					window.AddSubview(new UIImageView(UIImage.FromBundle("Images/21logo.jpg")));
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
		
		private void AuthSequence(User user)
		{
			//UsersServ.GetSocialIds(new List<long>()  {1, 2, 3}, 1);
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
			InvokeOnMainThread(InitApp);
		}
		
		public void Logout()
		{
		 	MainWnd.WillRemoveSubview(tabBarController.View);
			tabBarController.SetViewControllers(new UIViewController[0], true);
			tabBarController = null;
		}
		
		public void InitLoginPage()
		{
			var welcomePage = new WelcomePage(this);			
			var nav = new UINavigationController(welcomePage) { NavigationBarHidden = true, };
			welcomePage.Nav = nav;										
			nav.Add(welcomePage.View);			
			window.AddSubview(nav.View);
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

