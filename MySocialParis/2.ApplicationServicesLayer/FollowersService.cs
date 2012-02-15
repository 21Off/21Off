using System;
using System.Collections.Generic;
using System.Json;
using System.Net;
using System.Threading;
using MSP.Client.DataContracts;

namespace MSP.Client
{	
	public class FollowersService : AppServiceBase
	{
		#region Private methods
		
		private IEnumerable<User> GetFollowersList(string uri)
		{
			var users = new List<User>();
			var we = new ManualResetEvent(false);
			
			JsonUtility.Launch(uri, false, s =>
	        {
				try
				{
					var json = JsonArray.Load (s);
					foreach (JsonObject obj in json["Users"])
					{		
						try
						{
							users.Add(UsersService.JsonToUser(obj));
						}
						catch (Exception ex)
						{
							Util.LogException("GetFollowersList", ex);
						}
					}
				}
				catch (Exception ex)
				{
					Util.LogException("GetFollowersList", ex);
				}
				we.Set();
			});
			
			we.WaitOne(5000);
			
			return users;
		}
		
		#endregion
		
		public IEnumerable<User> GetFriendsOfUser(int id)
		{
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/Followers?FriendsOfId={0}", id);
			
			return GetFollowersList(uri);
		}		
		
		public IEnumerable<User> GetFollowersOfUser(int id)
		{
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/Followers?FollowersOfId={0}", id);
			
			return GetFollowersList(uri);
		}
		
		public IEnumerable<User> GetFollowedByUser(int id)
		{
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/Followers?FollowedById={0}", id);
			
			return GetFollowersList(uri);		
		}
						
		public static Follow JsonToFollow(JsonObject obj)
		{
			var follow = new Follow()
			{						
				Id = Convert.ToInt32(obj["Id"].ToString()),
				FollowerId= Convert.ToInt32(obj["FollowerId"].ToString()),
				UserId = Convert.ToInt32(obj["UserId"].ToString()),
				
				//Time =  DateTime.ParseExact (obj ["Time"], "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture),				
			};
			
			return follow;
		}
		
		public int FollowUser(Follow follow)
		{
			return SendFollow(follow, true);
		}		
		
		public int UnFollowUser(Follow follow)
		{
			return SendFollow(follow, false);
		}
		
		private int SendFollow(Follow follow, bool isFollow)
		{			
			var cms = new SendFollowOp(){ Follow = follow, IsFollow = isFollow };
			var uri = string.Format("http://storage.21offserver.com/json/syncreply/SendFollowOp");
				
			var request = (HttpWebRequest) WebRequest.Create (uri);
			request.Method = "PUT"; // OR POST	
			using (var reqStream = request.GetRequestStream())
			{				
				ServiceStack.Text.JsonSerializer.SerializeToStream(cms, typeof(SendFollowOp), reqStream);
			};
			
			var response = request.GetResponse();			
			var stream = response.GetResponseStream();
			//var responseString = new StreamReader(stream).ReadToEnd();
			var jsonObj = JsonObject.Load(stream);
			if (jsonObj.ContainsKey("Count"))
		    {
				return Convert.ToInt32(jsonObj["Count"].ToString());
			}
			
			return 0;			
		}
	}
}
