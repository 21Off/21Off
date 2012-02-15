using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using FacebookSdk;
using SocialLogin;
using Share;

namespace FaceBook
{
	public class FaceBookApplication : ISocialApplication
	{
		protected UIViewController _parentViewController;
		
		// Your Facebook APP Id must be set before running this example
		// See http://www.facebook.com/developers/createapp.php
		protected const string _appId = "168889879843414";
		
		protected Facebook _facebook;
		
		protected SessionDelegate _sessionDelegate;

		protected RequestDelegate _requestDelegate;
		
		protected LoginDialogDelegate _loginDialogDelegate;
		
		protected DialogDelegate _dialogDelegate;
		
		#region Constructors
		
		public FaceBookApplication (UIViewController parentViewController)
		{
			_parentViewController = parentViewController;
			_facebook = new Facebook(_appId);
			_sessionDelegate = new SessionDelegate(this);
			_requestDelegate = new RequestDelegate(this);
			_loginDialogDelegate = new LoginDialogDelegate(this);
			_dialogDelegate = new DialogDelegate(this);
		}
		
		#endregion Constructors
		
		#region Public Methods
		/// <summary>
		/// 
		/// </summary>
		/// <param name="name">
		/// The title of the post. The post should fit on one line in a user's stream;
		/// make sure you account for the width of any thumbnail.
		/// </param>
		/// <param name="href">
		/// The URL to the source of the post referenced in the name.
		/// The URL should not be longer than 1024 characters.
		/// </param>
		/// <param name="caption">
		/// A subtitle for the post that should describe why
		/// the user posted the item or the action the user took.
		/// This field can contain plain text only, as well as the {*actor*} token,
		/// which gets replaced by a link to the profile of the session user.
		/// The caption should fit on one line in a user's stream;
		/// make sure you account for the width of any thumbnail.
		/// </param>
		/// <param name="imageUrl">
		/// Maps to the image URL, and to the URL where a user should be taken
		/// if he or she clicks the photo. By default, imageUrl is the same as href.
		/// </param>		
		public void Publish(
		               string name,
		               string href,
		               string caption,
		               string imageUrl
		               )
		{
#if DEBUG
			if (string.IsNullOrEmpty(name)) name = "name";
			if (string.IsNullOrEmpty(href)) href = "http://i54.tinypic.com/2n7i3p4.jpg";
			if (string.IsNullOrEmpty(caption)) caption = "caption";
#endif
			if (string.IsNullOrEmpty(imageUrl)) imageUrl = href;

			NSString attachmentStr = new NSString(
				"{" +
			    "\"name\":\"" + name + "\"," +
			    "\"href\":\"" + href + "\"," +
			    "\"caption\":\"" + caption + "\"," +
			    "\"media\": [{" +
					"\"type\":\"image\"," +
					"\"src\":\""+ imageUrl + "\"," +
					"\"href\":\""+ imageUrl + "\"" +
				"}]}"
			);
			NSMutableDictionary parms = new NSMutableDictionary();
			//parms.Add(new NSString("api_key"),_appId);
			parms.Add(new NSString("attachment"),attachmentStr);
			_facebook.RequestWithMethodName("stream.publish",parms,"POST",_requestDelegate);
		}

		public bool LoggedIn()
		{
			string accessToken = NSUserDefaults.StandardUserDefaults.StringForKey("FacebookAccessToken");
			NSDate expirationDate = (NSDate)NSUserDefaults.StandardUserDefaults.ValueForKey(new NSString("FacebookExpirationDate"));
			if (!string.IsNullOrEmpty(accessToken) && expirationDate!=null)
			{
				_facebook.AccessToken = accessToken;
				_facebook.ExpirationDate = expirationDate;
				return _facebook.IsSessionValid;
			}
			return false;
		}
		
		public void Login()
		{
			//_facebook.Authorize(new string[]{"publish_stream", "offline_access"}, _sessionDelegate);
			var socialLogin = new SocialLogin.SocialLogin(new SocialLogin.FacebookConfig(){
				AppID = _appId,
				Permissions = new string[] { "publish_stream" }
			});
			socialLogin.LoginComplete += delegate(SocialLogin.SocialLogin sender) {
				Console.WriteLine("Logged in as " + sender.Username);
				_facebook.AccessToken = sender.AccessToken;
				_facebook.ExpirationDate = sender.ExpirationDate;
				
				GraphUser guser = null;
				decimal id;
				
				if (decimal.TryParse(sender.UserId, out id))
				{
					guser = new GraphUser()
					{
						 id = id,
						 name = sender.Username,
					};
				}
				
				SaveSessionData(true);
				
				if (OnLoginComplete != null)
					OnLoginComplete();
				
				if (OnExtraLoginComplete != null)
					OnExtraLoginComplete(guser);
			};
			socialLogin.LoginFailure += delegate {
				Console.WriteLine("Login failure");
				SaveSessionData(false);
				
				if (OnLoginComplete != null)
					OnLoginComplete();	
				
				if (OnExtraLoginComplete != null)
					OnExtraLoginComplete(null);				
			};
			socialLogin.Login(_parentViewController, true);
		}

		public void Logout()
		{
			_facebook.Logout(_sessionDelegate);	 
		}
		
//		public void HandleOpenURL(NSUrl url)
//		{
//			_facebook.HandleOpenUrl(url);
//		}
		
		public void SaveSessionData(bool loggedIn)
		{
			NSUserDefaults.StandardUserDefaults.SetString(loggedIn ?
				_facebook.AccessToken : "","FacebookAccessToken");
			NSUserDefaults.StandardUserDefaults.SetValueForKey(loggedIn ?
				_facebook.ExpirationDate : NSDate.Now,new NSString("FacebookExpirationDate"));
		}
		
		public static Dictionary<string, string> GetJsonDict(string json)
		{
			var res = new Dictionary<string, string>();
			
			string[] splits = json.Split(new string[] { "{", "}", ","}, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < splits.Length; i++)
			{
				string split = splits[i];
				split = split.Replace("\"", "");
				string[] vals = split.Split(new string[] {":"}, StringSplitOptions.RemoveEmptyEntries);
				if (vals.Length == 2)
				{
					string key = vals[0];
					string val = vals[1];
					res[key] = val;					
				}
			}
			
			return res;			
		}		
	
		#endregion Public Methods
		
		public event Action OnLoginComplete;
		
		public event Action<GraphUser> OnExtraLoginComplete;
				
	}
}

