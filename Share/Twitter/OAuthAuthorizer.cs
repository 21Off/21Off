//
// OAuth framework for TweetStation
//
// Author;
//   Miguel de Icaza (miguel@gnome.org)
//
// Possible optimizations:
//   Instead of sorting every time, keep things sorted
//   Reuse the same dictionary, update the values
//
// Copyright 2010 Miguel de Icaza
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Net;
using System.Web;
using System.Security.Cryptography;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace TweetStation
{
	//
	// Configuration information for an OAuth client
	//
	public class OAuthConfig {
		// keys, callbacks
		public string ConsumerKey, Callback, ConsumerSecret, TwitPicKey, BitlyKey;
		
		// Urls
		public string RequestTokenUrl, AccessTokenUrl, AuthorizeUrl;
	}
		
	//
	// The authorizer uses a config and an optional xAuth user/password
	// to perform the OAuth authorization process as well as signing
	// outgoing http requests
	//
	// To get an access token, you use these methods in the workflow:
	// 	  AcquireRequestToken
	//    AuthorizeUser
	//
	// These static methods only require the access token:
	//    AuthorizeRequest
	//    AuthorizeTwitPic
	//
	public class OAuthAuthorizer {
		// Settable by the user
		public string xAuthUsername, xAuthPassword;
		
		OAuthConfig config;
		string RequestToken, RequestTokenSecret;
		string AuthorizationToken, AuthorizationVerifier;
		public string AccessToken, AccessTokenSecret, AccessScreenname;
		public long AccessId;
		
		// Constructor for standard OAuth
		public OAuthAuthorizer (OAuthConfig config)
		{
			this.config = config;
		}
		
		// Constructor for xAuth
		public OAuthAuthorizer (OAuthConfig config, string xAuthUsername, string xAuthPassword)
		{
			this.config = config;
			this.xAuthUsername = xAuthUsername;
			this.xAuthPassword = xAuthPassword;
		}

		static Random random = new Random ();
		static DateTime UnixBaseTime = new DateTime (1970, 1, 1);

		// 16-byte lower-case or digit string
#if true // fel
		public
#endif
		static string MakeNonce ()
		{
			var ret = new char [16];
			for (int i = 0; i < ret.Length; i++){
				int n = random.Next (35);
				if (n < 10)
					ret [i] = (char) (n + '0');
				else
					ret [i] = (char) (n-10 + 'a');
			}
			return new string (ret);
		}
		
#if true // fel
		public
#endif
		static string MakeTimestamp ()
		{
			return ((long) (DateTime.UtcNow - UnixBaseTime).TotalSeconds).ToString ();
		}
		
		// Makes an OAuth signature out of the HTTP method, the base URI and the headers
#if true // fel
		public
#endif
		static string MakeSignature (string method, string base_uri, Dictionary<string,string> headers)
		{
			var items = from k in headers.Keys orderby k 
				select k + "%3D" + OAuth.PercentEncode (headers [k]);

			return method + "&" + OAuth.PercentEncode (base_uri) + "&" + 
				string.Join ("%26", items.ToArray ());
		}
		
#if true // fel
		public
#endif
		static string MakeSigningKey (string consumerSecret, string oauthTokenSecret)
		{
			return OAuth.PercentEncode (consumerSecret) + "&" + (oauthTokenSecret != null ? OAuth.PercentEncode (oauthTokenSecret) : "");
		}
		
#if true // fel
		public
#endif
		static string MakeOAuthSignature (string compositeSigningKey, string signatureBase)
		{
			var sha1 = new HMACSHA1 (Encoding.UTF8.GetBytes (compositeSigningKey));
			
			return Convert.ToBase64String (sha1.ComputeHash (Encoding.UTF8.GetBytes (signatureBase)));
		}
		
#if true // fel
		public
#endif
		static string HeadersToOAuth (Dictionary<string,string> headers)
		{
			return "OAuth " + String.Join (",", (from x in headers.Keys select String.Format ("{0}=\"{1}\"", x, headers [x])).ToArray ());
		}
		
		public bool AcquireRequestToken ()
		{
			var headers = new Dictionary<string,string> () {
				{ "oauth_callback", OAuth.PercentEncode (config.Callback) },
				{ "oauth_consumer_key", config.ConsumerKey },
				{ "oauth_nonce", MakeNonce () },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", MakeTimestamp () },
				{ "oauth_version", "1.0" }};
				
			string signature = MakeSignature ("POST", config.RequestTokenUrl, headers);
			string compositeSigningKey = MakeSigningKey (config.ConsumerSecret, null);
			string oauth_signature = MakeOAuthSignature (compositeSigningKey, signature);
			
			var wc = new WebClient ();
			headers.Add ("oauth_signature", OAuth.PercentEncode (oauth_signature));
			wc.Headers [HttpRequestHeader.Authorization] = HeadersToOAuth (headers);
			
			try {
				var str = wc.UploadString (new Uri (config.RequestTokenUrl), "");
				var result = HttpUtility.ParseQueryString (str);

				if (result ["oauth_callback_confirmed"] != null){
					RequestToken = result ["oauth_token"];
					RequestTokenSecret = result ["oauth_token_secret"];
					
					return true;
				}
			} catch (WebException wex){
#if true // fel
				Console.WriteLine(wex);
				if (wex.Response==null)
					return false;
#endif
				byte[] buffer = new byte[wex.Response.ContentLength];
				wex.Response.GetResponseStream().Read(buffer,0,buffer.Length);
				Console.WriteLine(Encoding.UTF8.GetString(buffer));
			} catch (Exception e) {
				Console.WriteLine (e);
				// fallthrough for errors
			}
			return false;
		}
		
		// Invoked after the user has authorized us
		//
		// TODO: this should return the stream error for invalid passwords instead of
		// just true/false.
		public bool AcquireAccessToken ()
		{
			var headers = new Dictionary<string,string> () {
				{ "oauth_consumer_key", config.ConsumerKey },
				{ "oauth_nonce", MakeNonce () },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", MakeTimestamp () },
				{ "oauth_version", "1.0" }};
			var content = "";
			if (xAuthUsername == null){
				headers.Add ("oauth_token", OAuth.PercentEncode (AuthorizationToken));
				headers.Add ("oauth_verifier", OAuth.PercentEncode (AuthorizationVerifier));
			} else {
				headers.Add ("x_auth_username", OAuth.PercentEncode (xAuthUsername));
				headers.Add ("x_auth_password", OAuth.PercentEncode (xAuthPassword));
				headers.Add ("x_auth_mode", "client_auth");
				content = String.Format ("x_auth_mode=client_auth&x_auth_password={0}&x_auth_username={1}", OAuth.PercentEncode (xAuthPassword), OAuth.PercentEncode (xAuthUsername));
			}
			
			string signature = MakeSignature ("POST", config.AccessTokenUrl, headers);
			string compositeSigningKey = MakeSigningKey (config.ConsumerSecret, RequestTokenSecret);
			string oauth_signature = MakeOAuthSignature (compositeSigningKey, signature);
			
			var wc = new WebClient ();
			headers.Add ("oauth_signature", OAuth.PercentEncode (oauth_signature));
			if (xAuthUsername != null){
				headers.Remove ("x_auth_username");
				headers.Remove ("x_auth_password");
				headers.Remove ("x_auth_mode");
			}
			wc.Headers [HttpRequestHeader.Authorization] = HeadersToOAuth (headers);
			
			try {
				var result = HttpUtility.ParseQueryString (wc.UploadString (new Uri (config.AccessTokenUrl), content));

				if (result ["oauth_token"] != null){
					AccessToken = result ["oauth_token"];
					AccessTokenSecret = result ["oauth_token_secret"];
					AccessScreenname = result ["screen_name"];
					AccessId = Int64.Parse (result ["user_id"]);
					
					return true;
				}
			} catch (WebException e) {
				var x = e.Response.GetResponseStream ();
				var j = new System.IO.StreamReader (x);
				Console.WriteLine (j.ReadToEnd ());
				Console.WriteLine (e);
				// fallthrough for errors
			}
			return false;
		}
		
		// 
		// Assign the result to the Authorization header, like this:
		// request.Headers [HttpRequestHeader.Authorization] = AuthorizeRequest (...)
		//
		public static string AuthorizeRequest (OAuthConfig config, string oauthToken, string oauthTokenSecret, string method, Uri uri, string data)
		{
			var headers = new Dictionary<string, string>() {
				{ "oauth_consumer_key", config.ConsumerKey },
				{ "oauth_nonce", MakeNonce () },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", MakeTimestamp () },
				{ "oauth_token", oauthToken },
				{ "oauth_version", "1.0" }};
			var signatureHeaders = new Dictionary<string,string> (headers);

			// Add the data and URL query string to the copy of the headers for computing the signature
			if (data != null && data != ""){
				var parsed = HttpUtility.ParseQueryString (data);
				foreach (string k in parsed.Keys){
					signatureHeaders.Add (k, OAuth.PercentEncode (parsed [k]));
				}
			}
			
			var nvc = HttpUtility.ParseQueryString (uri.Query);
			foreach (string key in nvc){
				if (key != null)
					signatureHeaders.Add (key, OAuth.PercentEncode (nvc [key]));
			}
			
			string signature = MakeSignature (method, uri.GetLeftPart (UriPartial.Path), signatureHeaders);
			string compositeSigningKey = MakeSigningKey (config.ConsumerSecret, oauthTokenSecret);
			string oauth_signature = MakeOAuthSignature (compositeSigningKey, signature);

			headers.Add ("oauth_signature", OAuth.PercentEncode (oauth_signature));
			
			return HeadersToOAuth (headers);
		}

		//
		// Used to authorize an HTTP request going to TwitPic
		//
		public static void AuthorizeTwitPic (OAuthConfig config, HttpWebRequest wc, string oauthToken, string oauthTokenSecret)
		{
			var headers = new Dictionary<string, string>() {
				{ "oauth_consumer_key", config.ConsumerKey },
				{ "oauth_nonce", MakeNonce () },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", MakeTimestamp () },
				{ "oauth_token", oauthToken },
				{ "oauth_version", "1.0" },
				//{ "realm", "http://api.twitter.com" }
			};
			string signurl = "http://api.twitter.com/1/account/verify_credentials.xml";
			// The signature is not done against the *actual* url, it is done against the verify_credentials.json one 
			string signature = MakeSignature ("GET", signurl, headers);
			string compositeSigningKey = MakeSigningKey (config.ConsumerSecret, oauthTokenSecret);
			string oauth_signature = MakeOAuthSignature (compositeSigningKey, signature);

			headers.Add ("oauth_signature", OAuth.PercentEncode (oauth_signature));

			//Util.Log ("Headers: " + HeadersToOAuth (headers));
			wc.Headers.Add ("X-Verify-Credentials-Authorization", HeadersToOAuth (headers));
			wc.Headers.Add ("X-Auth-Service-Provider", signurl);
		}
		
		class AuthorizationViewController : CustomWebViewController {
			NSAction callback;
			OAuthAuthorizer container;
			string url;
			
			public AuthorizationViewController (OAuthAuthorizer oauth, string url, NSAction callback)
			{
				this.url = url;
				this.container = oauth;
				this.callback = callback;
			
				SetupWeb (url);
			}
				  
			protected override string UpdateTitle ()
			{
				return "Authorization";
			}
			
			public override void ViewWillAppear (bool animated)
			{
				SetupWeb ("Authorization");
				WebView.ShouldStartLoad = LoadHook;
#if true // fel
				WebView.LoadFinished += delegate(object sender, EventArgs e)
				{
					UIWebView webView = sender as UIWebView;
					
					//webView.EvaluateJavascript("window.scrollTo(0,document.body.scrollHeight);");
					
					string pin = webView.EvaluateJavascript("document.getElementById(\"oauth_pin\").getElementsByTagName(\"code\")[0].innerHTML;");
					if (!string.IsNullOrEmpty(pin))
					{
						Console.WriteLine("pin="+pin);
						container.AuthorizationVerifier = pin;
						container.AcquireAccessToken ();
						callback ();
						DismissModalViewControllerAnimated (false);
					}
				};
				
				WebView.LoadError += delegate(object sender, UIWebErrorArgs e)
				{
					using(UIAlertView alert = new UIAlertView("Twitter",e.Error.Code + ": " + e.Error.LocalizedDescription,null,"OK",null))
						{ alert.Show();}
					DismissModalViewControllerAnimated (false);
				};
				
				base.ViewWillAppear(animated);
			}
			
			public override void ViewDidAppear (bool animated)
			{
				base.ViewDidAppear(animated);
				WebView.LoadRequest (new NSUrlRequest (new NSUrl (url)));
#else
				WebView.LoadRequest (new NSUrlRequest (new NSUrl (url)));
				base.ViewWillAppear (animated);
#endif
			}
			
			bool LoadHook (UIWebView sender, NSUrlRequest request, UIWebViewNavigationType navType)
			{
				var requestString = request.Url.AbsoluteString;
				if (requestString.StartsWith (container.config.Callback)){
					var results = HttpUtility.ParseQueryString (requestString.Substring (container.config.Callback.Length+1));
					
					container.AuthorizationToken = results ["oauth_token"];
					container.AuthorizationVerifier = results ["oauth_verifier"];
					DismissModalViewControllerAnimated (false);
					
					container.AcquireAccessToken ();
					callback ();
				}
#if true // fel
				else
				if (requestString.StartsWith (container.config.AuthorizeUrl)&&
					requestString.Length>container.config.AuthorizeUrl.Length)
				{
					var results = HttpUtility.ParseQueryString (requestString.Substring (container.config.AuthorizeUrl.Length+1));
					container.AuthorizationToken = results ["oauth_token"];
				}
#endif
				return true;
			}
		}
		
		public void AuthorizeUser (UIViewController parent, NSAction callback)
		{
			var authweb = new AuthorizationViewController (this, config.AuthorizeUrl + "?oauth_token=" + RequestToken, callback);
			
			parent.PresentModalViewController (authweb, true);
		}
	}
	
	public static class OAuth {
		
		// 
		// This url encoder is different than regular Url encoding found in .NET 
		// as it is used to compute the signature based on a url.   Every document
		// on the web omits this little detail leading to wasting everyone's time.
		//
		// This has got to be one of the lamest specs and requirements ever produced
		//
		public static string PercentEncode (string s)
		{
			var sb = new StringBuilder ();
			
			foreach (byte c in Encoding.UTF8.GetBytes (s)){
				if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '-' || c == '_' || c == '.' || c == '~')
					sb.Append ((char) c);
				else {
					sb.AppendFormat ("%{0:X2}", c);
				}
			}
			return sb.ToString ();
		}
	}
}

