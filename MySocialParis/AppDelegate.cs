using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Threading;

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
}

