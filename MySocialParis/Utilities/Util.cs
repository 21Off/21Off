using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using MonoTouch.CoreLocation;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public static class Util
	{
		/// <summary>
		///   A shortcut to the main application
		/// </summary>
		public static UIApplication MainApp = UIApplication.SharedApplication;
		
		public readonly static string BaseDir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "..");

		//
		// Since we are a multithreaded application and we could have many
		// different outgoing network connections (api.twitter, images,
		// searches) we need a centralized API to keep the network visibility
		// indicator state
		//
		static object networkLock = new object ();
		static int active;
		
		public static void PushNetworkActive ()
		{
			lock (networkLock){
				active++;
				MainApp.NetworkActivityIndicatorVisible = true;
			}
		}
		
		public static void PopNetworkActive ()
		{
			lock (networkLock){
				active--;
				if (active == 0)
					MainApp.NetworkActivityIndicatorVisible = false;
			}
		}		
				
		public static void ReportError (UIViewController current, Exception e, string msg)
		{
			var root = new RootElement (Locale.GetText ("Error")) {
				new Section (Locale.GetText ("Error")) {
					new StyledStringElement (msg){
						Font = UIFont.BoldSystemFontOfSize (14),
					}
				}
			};
			
			if (e != null){
				root.Add (new Section (e.GetType ().ToString ()){
					new StyledStringElement (e.Message){
						Font = UIFont.SystemFontOfSize (14),
					}
				});
				root.Add (new Section ("Stacktrace"){
					new StyledStringElement (e.ToString ()){
						Font = UIFont.SystemFontOfSize (14),
					}
				});
			};
			
			// Delay one second, as UIKit does not like to present
			// views in the middle of an animation.
			NSTimer.CreateScheduledTimer (TimeSpan.FromSeconds (1), delegate {
				UINavigationController nav = null;
				DialogViewController dvc = new DialogViewController (root);
				dvc.NavigationItem.LeftBarButtonItem = new UIBarButtonItem (Locale.GetText ("Close"), UIBarButtonItemStyle.Plain, delegate {
					nav.DismissModalViewControllerAnimated (false);
				});
				
				nav = new UINavigationController (dvc);
				current.PresentModalViewController (nav, false);	
			});
		}
		
		public static DateTime LastUpdate (string key)
		{
			var s = Defaults.StringForKey (key);
			if (s == null)
				return DateTime.MinValue;
			long ticks;
			if (Int64.TryParse (s, out ticks))
				return new DateTime (ticks, DateTimeKind.Utc);
			else
				return DateTime.MinValue;
		}
		
		public static bool NeedsUpdate (string key, TimeSpan timeout)
		{
			return DateTime.UtcNow - LastUpdate (key) > timeout;
		}
		
		public static void RecordUpdate (string key)
		{
			Defaults.SetString (key, DateTime.UtcNow.Ticks.ToString ());
		}
			
		
		public static NSUserDefaults Defaults = NSUserDefaults.StandardUserDefaults;
				
		const long TicksOneDay = 864000000000;
		const long TicksOneHour = 36000000000;
		const long TicksMinute = 600000000;
		
		static string s1 = Locale.GetText ("1 sec");
		static string sn = Locale.GetText (" secs");
		static string m1 = Locale.GetText ("1 min");
		static string mn = Locale.GetText (" mins");
		static string h1 = Locale.GetText ("1 hour");
		static string hn = Locale.GetText (" hours");
		static string d1 = Locale.GetText ("1 day");
		static string dn = Locale.GetText (" days");
		static string mth1 = Locale.GetText("1 month");
		static string mthn = Locale.GetText(" months");
		
		public static string FormatTime (TimeSpan ts)
		{
			int v;
			
			if (ts.Ticks < TicksMinute){
				v = ts.Seconds;
				if (v <= 1)
					return s1;
				else
					return v + sn;
			} else if (ts.Ticks < TicksOneHour){
				v = ts.Minutes;
				if (v == 1)
					return m1;
				else
					return v + mn;
			} else if (ts.Ticks < TicksOneDay){
				v = ts.Hours;
				if (v == 1)
					return h1;
				else
					return v + hn;
			} else {
				v = ts.Days;
				if (v == 1)
					return d1;
				else
					return v + dn;
			}
		}		
		
		public static RootElement MakeProgressRoot (string caption)
		{
			return new RootElement (caption){
				new Section (){
					//new ActivityElement ()
				}
			};
		}
		
		public static RootElement MakeError (string diagMsg)
		{
			return new RootElement (Locale.GetText ("Error")){
				new Section (Locale.GetText ("Error")){
					new MultilineElement (Locale.GetText ("Unable to retrieve the information"))
				}
			};
		}
		
		static long lastTime;
		[Conditional ("TRACE")]
		public static void ReportTime (string s)
		{
			long now = DateTime.UtcNow.Ticks;
			
			Console.WriteLine ("[{0}] ticks since last invoke: {1}", s, now-lastTime);
			lastTime = now;
		}
		
		//[Conditional ("TRACE")]
		public static void Log (string format, params object [] args)
		{
			Console.WriteLine (String.Format (format, args));
		}				
		
		//[Conditional ("TRACE")]
		public static void LogException (string text, Exception e)
		{
			try
			{
				var msg = String.Format ("On {0}, message: {1}\nException:\n{2}", DateTime.Now, text, e.ToString());
				Console.WriteLine (msg);
				
				using (var s = System.IO.File.AppendText (Util.BaseDir + "/Documents/crash.log")){				
					s.WriteLine (msg);					
				}
			}
			catch (Exception)
			{
			}
		}
				
		public class ActionSheetManager : IDisposable
		{
			public Action OkAction;
			public Action CancelAction;
			private UIActionSheet _Sheet;
			
			public ActionSheetManager(UIActionSheet sheet, Action okAction, Action cancelAction)
			{
				OkAction = okAction;
				CancelAction = cancelAction;
				
				_Sheet = sheet;
				sheet.Clicked+= HandleActionSheetClicked;
				sheet.Canceled += HandleSheetCanceled;
			}

			void HandleSheetCanceled (object sender, EventArgs e)
			{
				if (CancelAction != null)
					CancelAction();
				Dispose();
			}
			
			void HandleActionSheetClicked (object sender, UIButtonEventArgs args)
			{
				if (args.ButtonIndex == 0)
				{
					if (OkAction != null)
						OkAction();
				}
				if (args.ButtonIndex == 1)
				{
					if (CancelAction != null)
						CancelAction();
				}
				Dispose();		
			}
		

			#region IDisposable implementation
			
			public void Dispose ()
			{
				_Sheet.Clicked -= HandleActionSheetClicked;
				_Sheet.Canceled -= HandleSheetCanceled;
			}
			
			#endregion
		}
			
		private static UIActionSheet sheet;
		
		public static void ShowAlertSheet(string title, UIView view)
		{
			sheet = new UIActionSheet (title, null, null, "OK", null);
			sheet.CancelButtonIndex = 1;
			sheet.ShowInView (view);			
		}
		
		public static void ShowAlertSheet(string title, UIView view, Action OkAction)
		{
			ShowAlertSheet(title, view, OkAction, null);		
		}
		
		public static void ShowAlertSheet(string title, UIView view, Action OkAction, Action cancelAction)
		{
			sheet = new UIActionSheet (title, null, "Cancel", "OK", null);
			sheet.CancelButtonIndex = 1;
			
			new ActionSheetManager(sheet, OkAction, cancelAction);
			var tabView = AppDelegateIPhone.tabBarController.View;
			
			sheet.ShowInView (tabView);			
		}		
		
		static CultureInfo americanCulture;
		public static CultureInfo AmericanCulture {
			get {
				if (americanCulture == null)
					americanCulture = new CultureInfo ("en-US");
				return americanCulture;
			}
		}
		
		#region Location
		
		public class MyCLLocationManagerDelegate : CLLocationManagerDelegate {
			Action<CLLocation> callback;
			int attempts = 0;
			
			public void SetCallBack(Action<CLLocation> callback)
			{
				this.attempts = 0;
				this.callback = callback;
			}
			
			public MyCLLocationManagerDelegate (Action<CLLocation> callback)
			{
				this.callback = callback;
			}
			
			public override void UpdatedLocation (CLLocationManager manager, CLLocation newLocation, CLLocation oldLocation)
			{
				attempts++;
				
				if (newLocation.Timestamp.SecondsSinceReferenceDate < 10 ||
				    attempts >= 3 ||
				    newLocation.HorizontalAccuracy < 200f)
				{
					//System.Threading.Thread.Sleep(100000);
					manager.StopUpdatingLocation ();
					if (callback != null)
						callback (newLocation);
				}
			}
			
			public override void Failed (CLLocationManager manager, NSError error)
			{
				Util.LogException("MyCLLocationManagerDelegate Failed", new Exception(error.ToString()));
			
				if (callback != null)
					callback (null);
			}			
		}

		static CLLocationManager locationManager;
		static public void RequestLocation (Action<CLLocation> callback)
		{
			if (CLLocationManager.LocationServicesEnabled)
			{
				if (locationManager == null)
				{
					locationManager = new CLLocationManager () {
						DesiredAccuracy = CLLocation.AccuracyHundredMeters,
						Delegate = new MyCLLocationManagerDelegate (callback),
						DistanceFilter = 100f, 
					};
				}
				else
					((MyCLLocationManagerDelegate)locationManager.Delegate).SetCallBack(callback);
	
				locationManager.StartUpdatingLocation ();
			}
			else
				callback(null);
		}	
		#endregion
	}
}

