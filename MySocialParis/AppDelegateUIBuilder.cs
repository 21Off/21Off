using System.Drawing;
using MonoTouch.UIKit;
using System;
using MSP.Client.DataContracts;
using TweetStation;

namespace MSP.Client
{
	public partial class AppDelegateIPhone
	{
		// Containers
		public static UITabBarController tabBarController;
		public static UINavigationController [] navigationRoots;	

		// Buzz 1° & 2°
		//private StartMapViewController startMap;
		private StartMediaViewController startMedia;
		
		public static MSPNavigationController buzzNavigationController;

		// Me 8° (B, C)
		private ProfileViewController me;
		public static UINavigationController meNavigationController;
		
		// Share 4°
		private UINavigationController shareNavigationController;

		// Activity 8 A
		private ActivityViewController around;
		public static  MSPNavigationController aroundNavigationController;

		private void InitializeWindow ()
		{
			window = new UIWindow(UIScreen.MainScreen.Bounds);
			//window.BackgroundColor = UIColor.FromPatternImage(UIImage.FromFile("Images/background.png"));
		}

		private void InitializeMainViewControllers ()
		{
			InitializeBuzzTab ();
			InitializeShareTab ();
			InitializeAroundTab ();
			InitializeMeTab ();
		}
		
		private void InitializeShareTab ()
		{	
			shareNavigationController = new MSPNavigationController(this);
			shareNavigationController.View.Frame = new RectangleF(0, 20, 320, 400);				
			shareNavigationController.TabBarItem = new UITabBarItem("share", Graphics.GetImgResource("share"), 1);
		}
		
		private void InitializeMeTab ()
		{								
			meNavigationController = new MSPNavigationController (this) { NavigationBarHidden = true };			
			me = new ProfileViewController (meNavigationController);			
			
			meNavigationController.TabBarItem = new UITabBarItem("me", Graphics.GetImgResource("me"), 3);
			meNavigationController.PushViewController (me, false);
		}

		private void InitializeBuzzTab ()
		{	
			buzzNavigationController = new MSPNavigationController (this);
			buzzNavigationController.TabBarItem = new UITabBarItem("buzz", Graphics.GetImgResource("buzz"), 0);
			
			startMedia = new StartMediaViewController (buzzNavigationController);			
			buzzNavigationController.PushViewController (startMedia, false);			
		}
		
		private void InitializeAroundTab ()
		{					
			aroundNavigationController = new MSPNavigationController (this);
			around = new ActivityViewController (aroundNavigationController);
			
			aroundNavigationController.TabBarItem = new UITabBarItem("activity", Graphics.GetImgResource("activity"), 2);
			aroundNavigationController.PushViewController (around, false);
		}
		
		public void InitApp()
		{
			InitializeMainViewControllers();
			InitializeTabController();
			
			timer = new System.Threading.Timer(GetNotifications, null, 4000, 5 * 60 * 1000);
		}
		
		public void LoadFacebookFriends()
		{
			tabBarController.SelectedViewController = meNavigationController;
			var rotatingTb = (RotatingTabBar)AppDelegateIPhone.tabBarController;
			rotatingTb.SelectTab(3);
			
			Action act = () => me.LoadFacebookFriends();
			AppDelegateIPhone.ShowRealLoading(null, "Loading facebook friends", null, act);			
		}
		
		private System.Threading.Timer timer;
		private bool asking = false;
		
		private void InitializeTabController ()
		{
			tabBarController = new RotatingTabBar();									
			
			navigationRoots = new UINavigationController [4] {
				buzzNavigationController,
				shareNavigationController,
				aroundNavigationController, 
				meNavigationController,
			};

			tabBarController.SetViewControllers(navigationRoots, true);
			tabBarController.ViewControllerSelected += HandleTabBarControllerViewControllerSelected;			
						
			window.AddSubview (tabBarController.View);					
		}					
		
		public void GotoToShare(Tweet tweet = null, UINavigationController navCtrl = null)
		{
			if (navCtrl == null)
				navCtrl = shareNavigationController;
			
			var vc = new VCViewController();
			vc.Delegate = new pickerDelegate(vc, navCtrl, tweet);			
			
			tabBarController.PresentModalViewController(vc, true);			
		}
		
		public void GotoToBuzz()
		{
			var _MSP = (UINavigationController)tabBarController.SelectedViewController;
			_MSP.PopViewControllerAnimated(true);
			_MSP.PopViewControllerAnimated(true);
			_MSP.SetViewControllers(new UIViewController[0], false);
			
			var aaa =  _MSP.TabBarController;			
			aaa.SelectedViewController = aaa.ViewControllers[0];
			
			var rotatingTb = (RotatingTabBar)AppDelegateIPhone.tabBarController;
			rotatingTb.SelectTab(0);			
		}
		
		private void GetNotifications(object o)
		{
			if (asking)
				return;
			
			asking = true;
			
			try
			{
				double last = Util.Defaults.DoubleForKey("LastUpdate");
				long ticks = (long)last;
				
				var myId = MainUser.Id;
				var count = AppDelegateIPhone.AIphone.ActivServ.GetNotificationsCountSince(myId, ticks);
				
				InvokeOnMainThread(()=>
				{
					navigationRoots[2].TabBarItem.BadgeValue = count == 0 ? null : count.ToString();
				});
			}
			catch (Exception)
			{
				// Ignore get notifications exceptions
			}
			
			asking = false;
		}
		
		void HandleTabBarControllerViewControllerSelected (object sender, UITabBarSelectionEventArgs e)
		{			
			if (e.ViewController == buzzNavigationController)
				return;
			
			if (e.ViewController == meNavigationController)
				return;
			
			if (e.ViewController == shareNavigationController)
			{
				GotoToShare();
			}
			else
			{
				if (shareNavigationController.VisibleViewController != null)
				{
					shareNavigationController.PopViewControllerAnimated(false);
				}
				if (shareNavigationController.ViewControllers.Length >= 1)
				{
					shareNavigationController.SetViewControllers(new UIViewController[0], false);
				}				
				ShowLoading(window, null);
			}
		}
	}
}

