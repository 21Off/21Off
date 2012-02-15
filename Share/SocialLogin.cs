using System;
using System.IO;
using System.Net;
using System.Threading;
using MonoTouch.UIKit;
using MonoTouch.Facebook.Authorization;
#if true // fel
using MonoTouch.Foundation;
#endif

namespace SocialLogin
{
	/// <summary>
	/// Allows a user to log in using oauth based social networks like Twitter and Facebook.
	/// </summary>
	public class SocialLogin
	{
		public delegate void OnLoginCompleteHandler(SocialLogin sender);
		/// <summary>
		/// Event fired when login is successful.
		/// </summary>
		public event OnLoginCompleteHandler LoginComplete;
		
		public delegate void OnLoginFailureHandler(SocialLogin sender);
		/// <summary>
		/// Event fired when login fails.
		/// </summary>
		public event OnLoginFailureHandler LoginFailure;
		
		/// <summary>
		/// Active NetworkConfig for this instance.
		/// </summary>
		public NetworkConfig Config { get; private set; }
		
		/// <summary>
		/// OAuth access token assigned by the social network.
		/// </summary>
		public string AccessToken { get; private set; }
		
		/// <summary>
		/// Username of the user who logged in.
		/// </summary>
		public string Username { get; private set; }
		
		public string UserId {get;private set;}
		
#if true // fel
		/// <summary>
		/// Gets or sets the expiration date.
		/// </summary>
		public NSDate ExpirationDate { get; private set; }
		
		/// <summary>
		/// Gets or sets the access token secret.
		/// </summary>
		public string AccessTokenSecret { get; private set; }
#endif
		public SocialLogin (NetworkConfig config)
		{
#if true // fel
			System.Net.ServicePointManager.Expect100Continue = false;
#endif
			this.Config = config;
		}
		
		/// <summary>
		/// Starts the login process.
		/// </summary>
		public void Login(UIViewController parent, bool animate)
		{
			switch (Config.Network)
			{
				case SocialNetwork.Facebook:
					{
						var config = (FacebookConfig)Config;
						var authorizor = new FacebookAuthorizationViewController(config.AppID,
#if true // fel
							config.Permissions, FbDisplayType.Popup);
#else
							config.Permissions, FbDisplayType.Touch);
#endif
				
						authorizor.AccessToken += delegate(string accessToken, DateTime expires) {
							try
							{
								var wc = new System.Net.WebClient();
								var result = wc.DownloadData("https://graph.facebook.com/me?access_token="+accessToken);
								var stream = new MemoryStream(result);						
								var jsonArray = (System.Json.JsonObject)System.Json.JsonObject.Load(stream);
								string username = jsonArray["name"].ToString().Replace("\"", "");
								this.UserId = jsonArray["id"].ToString().Replace("\"", "");
								this.AccessToken = accessToken;
								this.Username = username;
#if true // fel
								this.ExpirationDate = expires;
#endif
								parent.BeginInvokeOnMainThread(delegate{
									parent.DismissModalViewControllerAnimated(animate);
									OnSuccess();
								});
							}
							catch
							{
								OnFailure();
							}
						};
				
						authorizor.Canceled += delegate {
							OnFailure();
						};
				
						authorizor.AuthorizationFailed += delegate {
							OnFailure();
						};
				
						parent.BeginInvokeOnMainThread(delegate{
							parent.PresentModalViewController(authorizor,animate);
						});
					}
					break;
				case SocialNetwork.Twitter:
					{
						var config = (TwitterConfig)Config;
						var authorizor = new TweetStation.OAuthAuthorizer(new TweetStation.OAuthConfig(){
							ConsumerKey = config.ConsumerKey,
							ConsumerSecret = config.ConsumerSecret,
							Callback = config.Callback,
							RequestTokenUrl = "https://api.twitter.com/oauth/request_token",
							AccessTokenUrl = "https://api.twitter.com/oauth/access_token",
							AuthorizeUrl = "https://api.twitter.com/oauth/authorize"
						});
						parent.BeginInvokeOnMainThread(delegate {
							authorizor.AcquireRequestToken();
							authorizor.AuthorizeUser(parent,delegate(){
								if (authorizor.AccessScreenname != "")
								{
									this.Username = authorizor.AccessScreenname;
									this.AccessToken = authorizor.AccessToken;
									this.AccessTokenSecret = authorizor.AccessTokenSecret;
									OnSuccess();
								}
								else
								{
									OnFailure();
								}
							});
						});
					}
					break;
			}
		}
		
		private void OnFailure()
		{	
			if (LoginFailure != null)
				LoginFailure(this);
		}
		
		private void OnSuccess()
		{
			if (LoginComplete != null)
				LoginComplete(this);
		}
	}
	
	/// <summary>
	/// Enumeration of social networks that can be used for login.
	/// </summary>
	public enum SocialNetwork
	{
		None,
		Facebook,
		Twitter
	}
	
	/// <summary>
	/// Configuration object for use with SocialLogin.
	/// </summary>
	public class NetworkConfig
	{
		/// <summary>
		/// Social network the configuration is used with.
		/// </summary>
		public virtual SocialNetwork Network { get { return SocialNetwork.None; } }
	}
	
	public class FacebookConfig : NetworkConfig
	{
		/// <summary>
		/// Social network the configuration is used with.
		/// </summary>
		public override SocialNetwork Network { get { return SocialNetwork.Facebook; } }
		/// <summary>
		/// App ID from Facebook app page.
		/// </summary>
		public string AppID { get; set; }
		/// <summary>
		/// Extra facebook permissions to ask for with the returned access token.
		/// </summary>
		public string[] Permissions { get; set; }
	}
	
	public class TwitterConfig : NetworkConfig
	{
		/// <summary>
		/// Social network the configuration is used with.
		/// </summary>
		public override SocialNetwork Network { get { return SocialNetwork.Twitter; } }
		/// <summary>
		/// OAuth consumer key from Twitter app page.
		/// </summary>
		public string ConsumerKey { get; set; }
		/// <summary>
		/// OAuth callback URL, do NOT leave blank.
		/// </summary>
		public string Callback { get; set; }
		/// <summary>
		/// OAuth consumer secret from Twitter app page.
		/// </summary>
		public string ConsumerSecret { get; set; }
	}	
}

