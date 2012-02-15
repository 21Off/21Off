
using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Net;
using System.Threading;
using System.Web;
using FacebookN;
using MonoTouch.Foundation;
using MSP.Client.DataContracts;
using Share;


namespace MSP.Client
{	
	public class FacebookService
	{
		public GraphUser GetFriend(decimal id)
		{
			string accessToken = NSUserDefaults.StandardUserDefaults.StringForKey("FacebookAccessToken");
			NSDate expirationDate = (NSDate)NSUserDefaults.StandardUserDefaults.ValueForKey(new NSString("FacebookExpirationDate"));
			if (!string.IsNullOrEmpty(accessToken) && expirationDate!=null)
			{
				var url = string.Format("https://graph.facebook.com/{0}?access_token={1}&format=json", id, accessToken);
	            var request = WebRequest.Create(url) as HttpWebRequest;
				
				try
				{
		            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
		            {
		                var reader = new StreamReader(response.GetResponseStream());
						string str = reader.ReadToEnd();
						Dictionary<string, string> dict = GetJsonDict(str);
						if (dict.ContainsKey("name"))
						{
							var gUser = new GraphUser()
							{
								name = dict["name"],
								id = id,
							};
							return gUser;
						}
					}
				}
				catch (Exception ex)
				{
					Util.LogException("GetFriend", ex);
				}
				
				/*
	            var api = new FacebookN.FacebookAPI(accessToken);
				
				//https://graph.facebook.com/1121317006/picture
								
				JSONObject me = api.Get(string.Format("/{0}", id));
	            Console.WriteLine(me.Dictionary["name"].String);
	            */
			}
			return null;
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
		
		
		public GraphUser GetMyProfile()
		{						
			string accessToken = NSUserDefaults.StandardUserDefaults.StringForKey("FacebookAccessToken");
			//GetOauthTokens(accessToken);
			
			NSDate expirationDate = (NSDate)NSUserDefaults.StandardUserDefaults.ValueForKey(new NSString("FacebookExpirationDate"));
			if (!string.IsNullOrEmpty(accessToken) && expirationDate!=null)
			{				
				var url = string.Format("https://graph.facebook.com/me?access_token={0}&format=json", accessToken);				
	            var request = WebRequest.Create(url) as HttpWebRequest;
				
				try
				{
		            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
		            {
		                var reader = new StreamReader(response.GetResponseStream());
						string str = reader.ReadToEnd();
						Dictionary<string, string> dict = GetJsonDict(str);
						
						var gUser = new GraphUser();
						
						if (dict.ContainsKey("name"))
						{
							gUser.name = dict["name"];
						}
						if (dict.ContainsKey("id"))
						{
							decimal d;
							if (decimal.TryParse(dict["id"], out d))
							{
								gUser.id = d;
							}
						}						
						
						return gUser;
					}
				}
				catch (Exception ex)
				{
					Console.WriteLine(ex.Message);
					Util.LogException("GetMyProfile", ex);
				}
			}
			return null;			
		}
		
		public List<decimal> GetFriends()
		{			
			var res = new List<decimal>();
			
			string accessToken = NSUserDefaults.StandardUserDefaults.StringForKey("FacebookAccessToken");
			NSDate expirationDate = (NSDate)NSUserDefaults.StandardUserDefaults.ValueForKey(new NSString("FacebookExpirationDate"));
			if (!string.IsNullOrEmpty(accessToken) && expirationDate!=null)
			{				
				var url = string.Format("https://api.facebook.com/method/friends.get?access_token={0}&format=json", accessToken);
				
				try
				{
		            var request = WebRequest.Create(url) as HttpWebRequest;
					
		            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
		            {
		                var reader = new StreamReader(response.GetResponseStream());
		                string retVal = reader.ReadToEnd();
						retVal = retVal.Replace("]", "").Replace("[", "");
						string[] ids = retVal.Split(new string[]{","}, StringSplitOptions.None);
						foreach (string id in ids)
						{
							res.Add(Convert.ToDecimal(id));
						}
					}
				}
				catch (Exception ex)
				{
					Util.LogException("GetFriends", ex);
				}
			}
			
			return res;
		}
		
		private NSString s = new NSString("x");
		
		private string GetCode()
		{
			string clientId = "168889879843414";
			string redirectUrl = "http://www.21off.net";
 
    		string url = string.Format("https://graph.facebook.com/oauth/authorize?client_id={0}&redirect_uri={1}", clientId, redirectUrl);
			
			s.InvokeOnMainThread(()=>
			{
				var req = new NSUrlRequest(new NSUrl(url));
				new NSUrlConnection(req, new CDel(req), true);
			});
			
			return "";
			
            var request = WebRequest.Create(url) as HttpWebRequest;
			
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                var reader = new StreamReader(response.GetResponseStream());
                string retVal = reader.ReadToEnd();
				return retVal;
			}
		}
		
		private Dictionary<string, string> GetOauthTokens(string code)
        {
            var tokens = new Dictionary<string, string>();
 
            string clientId = "168889879843414";
            string redirectUrl = "http://www.21off.net";
            string clientSecret = "904270b68a2cc3d54485323652da4d14";
            string scope = "read_friendlists,user_status";
 
            string url = string.Format("https://graph.facebook.com/oauth/access_token?client_id={0}&redirect_uri={1}&client_secret={2}&code={3}&scope={4}",
                            clientId, redirectUrl, clientSecret, code, scope);
 
            var request = WebRequest.Create(url) as HttpWebRequest;
            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                string retVal = reader.ReadToEnd();
 
                foreach (string token in retVal.Split('&'))
                {
                    tokens.Add(token.Substring(0, token.IndexOf("=")),
                        token.Substring(token.IndexOf("=") + 1, token.Length - token.IndexOf("=") - 1));
                }
            }
 
            return tokens;
        }		
	}
}
