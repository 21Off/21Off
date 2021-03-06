/*
 * MonoTouch.Facebook.Authorization
 * 
 * Developed by: Redth
 * Updated:      2010-05-13
 * 
 * About:
 * 
 * This is just a simple UIViewController to help you with logging users into Facebook on MonoTouch.
 * You end up calling it with something like:
 * 
 * FacebookAuthorizationViewController fac = new FacebookAuthorizationViewController("clientidhere", new string[] {"read_stream", "publish_stream", "user_groups"}, FbDisplayType.Touch);
 * fac.AccessToken += delegate(string accessToken, DateTime expires) {
 *   //Logged in, got your accessToken here and when it expires!
 *   myNavigationController.PopViewControllerAnimated(true);
 * 
 *   //Do something else here (eg: Save the accessToken and expiry date to be used in your Graph API calls)
 * }
 * 
 * myNavigationController.PushViewController(fac, true);
 * 
 *
 * More Information:
 *    Extended Permissions List:
 *    http://developers.facebook.com/docs/authentication/permissions
 *
 *    Facebook Graph API Documentation:
 *    http://developers.facebook.com/docs/api
 *
 *    Facebook Authentication Documentation:
 *    http://developers.facebook.com/docs/authentication/
 *
 */

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Specialized;

// https://gist.github.com/raw/400008/8de8170198148995d7477916f7cda526473b67f1/MonoTouch.Facebook.Authorization.cs
//www.facebook.com/logout.php?app_key=KEY&session_key=SESSION&next=REDIRECT_URL

namespace MonoTouch.Facebook.Authorization
{
	public class FacebookAuthorizationViewController : UIViewController
	{
		UIWebView webView;
		UIActivityIndicatorView activityIndicator;
		UIView loadingView;
		
		#region Constructors


		public FacebookAuthorizationViewController (string clientId, string[] extendedPermissions, FbDisplayType display) : base() //: base("FbAuthorizationViewController", NSBundle.MainBundle)
		{
			this.ClientId = clientId;
			this.ExtendedPermissions = extendedPermissions;
			this.Display = display;
		}		
		#endregion
				
		public string ClientId
		{
			get; private set;
		}
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return false;
			//return (toInterfaceOrientation == UIInterfaceOrientation.LandscapeLeft || toInterfaceOrientation == UIInterfaceOrientation.LandscapeRight);
		}
		
		public string[] ExtendedPermissions
		{
			get; private set;
		}
		
		public FbDisplayType Display
		{
			get; private set;	
		}
		
		public delegate void AccessTokenEventHandler(string accessToken, DateTime expires);
		public event AccessTokenEventHandler AccessToken;
		
		public delegate void AuthorizationFailedEventHandler(string message);
		public event AuthorizationFailedEventHandler AuthorizationFailed;
		public event EventHandler Canceled;
				
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			this.webView = new UIWebView(new RectangleF(0, 0, this.View.Frame.Width, this.View.Frame.Height));
			this.webView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth;
			
			//This is for the overlay 			
			this.loadingView = new UIView(new RectangleF(0, 0, this.View.Frame.Width, this.View.Frame.Height));
			this.loadingView.BackgroundColor = UIColor.FromRGBA(0.0f, 0.0f, 0.0f, 0.7f);
			this.loadingView.Hidden = true;
			this.loadingView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			
			this.activityIndicator = new UIActivityIndicatorView(new RectangleF(this.View.Frame.Width / 2 - 20, this.View.Frame.Height / 2 - 20, 40, 40));
			this.activityIndicator.AutoresizingMask = UIViewAutoresizing.FlexibleBottomMargin | UIViewAutoresizing.FlexibleTopMargin | UIViewAutoresizing.FlexibleLeftMargin | UIViewAutoresizing.FlexibleRightMargin;
			this.activityIndicator.ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.WhiteLarge;
			
			this.loadingView.AddSubview(this.activityIndicator);
			this.View.AddSubview(this.webView);
			this.View.AddSubview(this.loadingView);
			
			//Want to display the indicator
			this.webView.LoadStarted += delegate {
				if (this.loadingView.Hidden)
				{
					this.activityIndicator.StartAnimating();
					this.loadingView.Hidden = false;
				}
			};
			
			//This is where the magic happens
			this.webView.LoadFinished += delegate {
				
#if true // fel
				if (string.IsNullOrEmpty(webView.Request.Url.AbsoluteString))
				{
					//NSThread.SleepFor(0.5);
					this.ParentViewController.DismissModalViewControllerAnimated(false);
				}
				else
#endif
				//When success, Facebook redirects us to their page with a #access_token=blah type url fragment
				if (!string.IsNullOrEmpty(webView.Request.MainDocumentURL.Fragment))
				{				
					//NameValueCollection qs = HttpUtility.ParseQueryString (webView.Request.MainDocumentURL.Query);
					var query = HttpUtility.ParseQueryString(webView.Request.MainDocumentURL.Fragment);
					//Make sure we at least have the access_token
					if (query != null && !string.IsNullOrEmpty(query["access_token"]))
					{
						string accessToken = string.Empty;
						DateTime expires = DateTime.MinValue;
									
						accessToken = query["access_token"];	
					
						//Depending if we requested offline_access or not, there will be an access_token expiry date, which is # seconds from now
						if (!string.IsNullOrEmpty(query["expires_in"]))
						{
							int seconds = 0;
							if (int.TryParse(query["expires_in"], out seconds))
								expires = DateTime.Now.AddSeconds(seconds);	
#if true // fel
							if (seconds==0)
							{
								expires = DateTime.MaxValue;
							}
#endif
						}
						else
							expires = DateTime.MaxValue; //offline_access must have been requested, so no expires was sent to us, meaning it's good indefinitely
						
						//Raise the event let the consumer of this controller handle it
						if (this.AccessToken != null)
							this.AccessToken(accessToken, expires);
					}
				}
				else
				{
					var request = webView.Request.MainDocumentURL.ToString();
					//Console.WriteLine("Main Request: " + request);
					if(request.Contains("error_reason") && !request.Contains("redirect_uri"))
					{
						if(Canceled != null)
							Canceled(this, null);
#if true // fel
						if (ParentViewController != null)
							this.ParentViewController.DismissModalViewControllerAnimated(true);
						else
							this.DismissModalViewControllerAnimated(true);
#endif
					}
					//Hide the activity indicator
					this.activityIndicator.StopAnimating();
					this.loadingView.Hidden = true;
				}				
			};
			
			//Error means it didn't work
			this.webView.LoadError += delegate(object sender, UIWebErrorArgs e) {
				
				//Hide the activity indicator
				this.activityIndicator.StopAnimating();
				this.loadingView.Hidden = true;
								
				//Call the error event
				if (this.AuthorizationFailed != null)
					this.AuthorizationFailed(e.Error.Code + ": " + e.Error.LocalizedDescription);
#if true // fel
				using(UIAlertView alert = new UIAlertView("Facebook",e.Error.Code + ": " + e.Error.LocalizedDescription,null,"OK",null))
					{ alert.Show();}
				this.webView.LoadHtmlString(e.Error.Code + ": " + e.Error.LocalizedDescription, new NSUrl(""));

#endif
			};
						
#if true // fel
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
#endif
			//Build the scope string from the list of extended permissions
			string scope = string.Empty;
			
			if (this.ExtendedPermissions != null)
			{
				foreach (string s in this.ExtendedPermissions)
					scope += s + ",";
			}
			
			string startUrl = string.Format("https://graph.facebook.com/oauth/authorize?client_id={0}&redirect_uri={1}&display={2}&type=user_agent&scope={3}",
					this.ClientId,
					"http://www.facebook.com/connect/login_success.html",	
			        this.Display.ToString().ToLower(),
			        scope.TrimEnd(','));

			//Actually start loading the page
			this.webView.LoadRequest(NSUrlRequest.FromUrl(NSUrl.FromString(startUrl)));
		}
		
	}
	
	public enum FbDisplayType
	{
		Wap,
		Touch,
		Popup,
		Page
	}
	
}