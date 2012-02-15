using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TweetStation;
using SocialLogin;

namespace Twitter
{
	public class TwitterApplication : ISocialApplication
	{
		protected UIViewController _parentViewController;
		
		protected const string _callback = "oob";
		
		protected const string _consumerKey = "XjO3cQB2YkXz6j76rDepg";
		
		protected const string _consumerSecret = "Yt22R5Y8Z7HZDapNdE6cOJFtSSTBUdCbaXsb5z1yPVo";
		
		protected string _accessToken = "";
		
		protected string _accessTokenSecret = "";
		
		protected SocialLogin.SocialLogin _socialLogin;
		
		#region Constructors
		
		public TwitterApplication (UIViewController parentViewController)
		{
			_parentViewController = parentViewController;
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

			try
			{
				string statusesUpdateUrl = "http://api.twitter.com/1/statuses/update.json";
				string status = OAuth.PercentEncode(name + ": " + caption + ": " + imageUrl);
				
				var headers = new Dictionary<string,string> ()
				{
					{ "oauth_consumer_key", _consumerKey },
					{ "oauth_nonce", OAuthAuthorizer.MakeNonce () },
					{ "oauth_signature_method", "HMAC-SHA1" },
					{ "oauth_token", _accessToken },
					{ "oauth_timestamp", OAuthAuthorizer.MakeTimestamp () },
					{ "oauth_version", "1.0" },
					{ "status", status}
				};

				string signature = OAuthAuthorizer.MakeSignature ("POST", statusesUpdateUrl, headers);
				string compositeSigningKey = OAuthAuthorizer.MakeSigningKey (_consumerSecret,_accessTokenSecret);
				string oauth_signature = OAuthAuthorizer.MakeOAuthSignature (compositeSigningKey, signature);
				headers.Add ("oauth_signature", OAuth.PercentEncode (oauth_signature));

				headers.Remove("status");
				statusesUpdateUrl += "?status="+status;
				
				WebClient webClient = new WebClient();
				webClient.Encoding = Encoding.UTF8;			
				webClient.Headers [HttpRequestHeader.Authorization] = OAuthAuthorizer.HeadersToOAuth (headers);
				
				Console.WriteLine("signature="+signature);
				Console.WriteLine("compositeSigningKey="+compositeSigningKey);
				Console.WriteLine("oauth_signature="+oauth_signature);
				Console.WriteLine("Authorization="+webClient.Headers [HttpRequestHeader.Authorization]);			
#if false
	        	string reply = webClient.UploadString (statusesUpdateUrl,"");		
	        	Console.WriteLine (reply);
#else
				webClient.UploadStringCompleted += delegate(object sender, UploadStringCompletedEventArgs e)
				{
					Console.WriteLine("TwitterApplication.Publish():UploadStringCompleted");
					if (e.Error!=null)
						Console.WriteLine(e.Error);
					else
					if (e.Result!=null)
						Console.WriteLine(e.Result);
				};
				webClient.UploadStringAsync (new Uri(statusesUpdateUrl),"");
#endif
			}
			catch (WebException wex)
			{
				byte[] buffer = new byte[wex.Response.ContentLength];
				wex.Response.GetResponseStream().Read(buffer,0,buffer.Length);
				Console.WriteLine(Encoding.UTF8.GetString(buffer));
			}
			catch (Exception e)
			{
				Console.WriteLine (e);
				// fallthrough for errors
			}
		}
		
		public bool LoggedIn()
		{
			Console.WriteLine("TwitterApplication.LoggedIn()");
			string accessToken = NSUserDefaults.StandardUserDefaults.StringForKey("TwitterAccessToken");
			string accessTokenSecret = NSUserDefaults.StandardUserDefaults.StringForKey("TwitterAccessTokenSecret");
			if (!string.IsNullOrEmpty(accessToken) && !string.IsNullOrEmpty(accessTokenSecret))
			{
				_accessToken = accessToken;
				_accessTokenSecret = accessTokenSecret;
					
				return true;
			}
			return false;
		}
		
		public event Action OnLoginComplete;
		
		public void Login()
		{
			SocialLogin.SocialLogin socialLogin = new SocialLogin.SocialLogin(new SocialLogin.TwitterConfig(){
    			Callback = _callback,
    			ConsumerSecret = _consumerSecret,
    			ConsumerKey = _consumerKey
			});
			
			socialLogin.LoginFailure += delegate {
				Console.WriteLine("Login failure");
				SaveSessionData(false);
				
				if (OnLoginComplete != null)
					OnLoginComplete();				
			};			
			
			socialLogin.LoginComplete += delegate(SocialLogin.SocialLogin sender) {
    			Console.WriteLine("Logged in as " + sender.Username);
				Console.WriteLine("Access Token " + sender.AccessToken);
				Console.WriteLine("Access Token Secret " + sender.AccessTokenSecret);
				_accessToken = sender.AccessToken;
				_accessTokenSecret = sender.AccessTokenSecret;
				SaveSessionData(true);
				
				if (OnLoginComplete != null)
					OnLoginComplete();
			};
			socialLogin.Login(_parentViewController, true);
		}

		public void Logout()
		{
			SaveSessionData(false);	 
		}
		
//		public void HandleOpenURL(NSUrl url)
//		{
//		}
		
		public void SaveSessionData(bool loggedIn)
		{
			Console.WriteLine("TwitterApplication.SaveSessionData("+loggedIn.ToString()+")");
			NSUserDefaults.StandardUserDefaults.SetString(loggedIn ?
				_accessToken : "","TwitterAccessToken");
			NSUserDefaults.StandardUserDefaults.SetString(loggedIn ?
				_accessTokenSecret : "","TwitterAccessTokenSecret");
		}
	
		#endregion Public Methods
	}
}

