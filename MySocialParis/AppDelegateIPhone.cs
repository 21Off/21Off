using System;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;
using System.Threading;
using System.Collections.Generic;

namespace MSP.Client
{
	public partial class AppDelegateIPhone : UIApplicationDelegate
	{
		public DBActiviesService DBActivServ {get;set;}
		
		public FacebookService FacebookServ {get;set;}
		public UsersService UsersServ {get;set;}
		public ImagesService ImgServ {get;set;}
		public FollowersService FlwServ {get;set;}
		public KeywordsService KeywServ {get;set;}
		public LikesService LikesServ {get;set;}
		public CommentsService CommentServ {get;set;}
		public ActivitiesService ActivServ {get;set;}
		public ReportService ReportServ {get;set;}
		public ShareService ShareServ {get;set;}
		public InstagramService InstagramServ {get;set;}
				
		public static void ShowLoading(UIView view, string message)
		{
			ShowRealLoading(view, "Loading", message, null);
		}
		
		public static void ShowRealLoading(string title, string message, ManualResetEventSlim wait)
		{
			var hud = new LoadingHUDView(title, message);
			AppDelegateIPhone.AIphone.MainWnd.AddSubview(hud);
			hud.StartAnimating();			
			
			ThreadPool.QueueUserWorkItem(delegate {
				if (wait != null) 
					wait.Wait();
					
				s.InvokeOnMainThread(delegate {
					hud.StopAnimating ();
					hud.RemoveFromSuperview ();
					hud = null;
				});
			});
			
		}		
		
		public static void ShowRealLoading(UIView view, string title, string message, Action action)
		{
			var hud = new LoadingHUDView(title, message);
			//view.AddSubview(hud);
			AppDelegateIPhone.AIphone.MainWnd.AddSubview(hud);
			hud.StartAnimating();
			
			/*
			// Show the UI, and on a callback, do the scaling, so the user gets an animation
			NSTimer.CreateScheduledTimer (0.1, delegate {
				
				if (action != null) 
					action();
				
				hud.StopAnimating ();
				hud.RemoveFromSuperview ();
				hud = null;
			});
			*/
			
			ThreadPool.QueueUserWorkItem(delegate {
				
				try
				{
					if (action != null) 
						action();
				}
				catch (Exception ex)
				{
					Util.LogException(title, ex);
				}
				
				s.InvokeOnMainThread(delegate {
					hud.StopAnimating ();
					hud.RemoveFromSuperview ();
					hud = null;
				});
			});
			
		}
		
		private static NSString s = new NSString("x");
		
		public static void ShowMessage(UIView view, string title, string message, int timewait)
		{
			var hud = new LoadingHUDView(title, message);
			//view.AddSubview(hud);
			AppDelegateIPhone.AIphone.MainWnd.AddSubview(hud);
			hud.StartAnimating();
			
			// Show the UI, and on a callback, do the scaling, so the user gets an animation
			NSTimer.CreateScheduledTimer (timewait, delegate {
				
				hud.StopAnimating ();
				hud.RemoveFromSuperview ();
				hud = null;
			});				
		}
		
		public static void ShowMessage(UIView view, string title, string message)
		{
			ShowMessage(view, title, message, 1);
		}
	}	
	
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
				
				if (MainUser != null && false)
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
			var loginPage = new LoginPageGarde(this);
			var welcomePage = new WelcomePage(this);
						
			var vc = new UIViewController();
			var nav = new UINavigationController(welcomePage)
			{
				NavigationBarHidden = true,
			};
			loginPage.Nav = nav;
			welcomePage.Nav = nav;		
									
			nav.Add(welcomePage.View);
			
			window.AddSubview(nav.View);
			//window.AddSubview(vc.View);
			//vc.PresentModalViewController(nav, false);
		}
		
		public override void DidEnterBackground (UIApplication application)
		{
			ImageStore.Purge();
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

