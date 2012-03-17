using System;
using System.Json;
using System.Threading;
using MSP.Client.DataContracts;
using System.Collections.Generic;

namespace MSP.Client
{
	public class UsersService : AppServiceBase, IApplicationUsersService
	{
		#region IApplicationUsersService implementation
						
		public void UpdateSocialdId(decimal userId, long id, int socialType)
		{
			var we = new ManualResetEvent(false);			
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/UpdateSocialId?Id={0}&SocialType={1}&UserId={2}",
				id, socialType, userId);
			
			JsonUtility.Launch(uri, false, s =>
			{
				try
				{
					var json = JsonArray.Load (s);
				}
				catch (Exception ex)
				{
					Util.LogException("UpdateSocialdId", ex);
				}
				we.Set();
			});
			
			we.WaitOne(5000);
		}
		
		public List<User> GetSocialIds (List<long> socialIds, int socialType)
		{			
			var request = new GetSubscribersBySocialIds()
			{
				SocialIds = string.Join(",", socialIds),
				SocialType = socialType,
			};			
				
			List<User> res = null;
			
			var we = new ManualResetEvent(false);			
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/GetSubscribersBySocialIds");
			
			JsonUtility.Upload (uri, request, false, s =>
			{
				try
				{
					var json = JsonArray.Load (s);
					JsonArray subscribersObj = json.ContainsKey("Subscribers") ? (JsonArray)json["Subscribers"] : null;
					if (subscribersObj != null)
					{						
						res = new List<User>();
						foreach (JsonObject obj in subscribersObj)
						{
							res.Add(JsonToUser(obj));
						}
					}
				}
				catch (Exception ex)
				{
					Util.LogException("GetSocialIds", ex);
				}
				we.Set();
			});
			
			we.WaitOne(15000);
			
			return res;
		}		
		
		public User CreateUser (string userName, string password, string emailAddress, string profileImagePath)
		{
			IOFile profileImage = profileImagePath == null ? null : ImagesService.GetIOFile(profileImagePath);
			var storeNewUser = new StoreNewUser() 
			{ 
				Email = emailAddress, 
				Password = password, 
				UserName = userName, 
				ImageFile = profileImage,
				Version = "1.1.6"
			};
				
			User user = null;
			
			var we = new ManualResetEvent(false);			
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/StoreNewUser");
			
			JsonUtility.Upload (uri, storeNewUser, false, s =>
			{
				try
				{
					var json = JsonArray.Load (s);
					Console.WriteLine(json.ToString());	
					int Id = json.ContainsKey("UserId") ? Convert.ToInt32(json["UserId"].ToString()) : 0;
					user = new User(){ Id = Id, Name = userName };
				}
				catch (Exception ex)
				{
					Util.LogException("StoreNewUser", ex);
				}
				we.Set();
			});
			
			we.WaitOne(5000);
			
			return user;
		}
		
		public User UpdateUser (int userId, string userName, string password, string emailAddress, string profileImagePath)
		{
			IOFile profileImage = profileImagePath == null ? null : ImagesService.GetIOFile(profileImagePath);
			var updateNewUser = new UpdateNewUser() 
			{ 
				UserId = userId,
				Email = emailAddress, 
				Password = password, 
				UserName = userName, 
				ImageFile = profileImage 
			};
				
			User user = null;
			
			var we = new ManualResetEvent(false);			
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/UpdateNewUser");
			
			JsonUtility.Upload (uri, updateNewUser, false, s =>
			{
				try
				{
					var json = JsonArray.Load (s);
					Console.WriteLine(json.ToString());	
					int Id = json.ContainsKey("UserId") ? Convert.ToInt32(json["UserId"].ToString()) : 0;
					user = new User(){ Id = Id, Name = userName };
				}
				catch (Exception ex)
				{
					Util.LogException("UpdateUser", ex);
				}
				we.Set();
			});
			
			we.WaitOne(5000);
			
			return user;
		}		
		
		public User Authentificate (string userName, string password)
		{
			var authUser = new AuthUser() { Password = password, UserName = userName, Version = "1.1.6" };
			
			User user = null;
			
			var we = new ManualResetEvent(false);			
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/AuthUser");
			
			JsonUtility.Upload (uri, authUser, false, s =>
			{
				try
				{
					var json = JsonArray.Load (s);
					int Id = json.ContainsKey("UserId") ? Convert.ToInt32(json["UserId"].ToString()) : 0;
					user = new User(){ Id = Id, Name = userName };
				}
				catch (Exception ex)
				{
					Util.LogException("Authentificate", ex);
				}
				we.Set();
			});
			
			we.WaitOne(20000);
			
			return user;
		}
		
		public User AuthentificateFacebook (string userName, decimal userId, string signedRequest = "password")
		{
			var authUser = new AuthFacebookUser() { SignedRequest = signedRequest, UserName = userName, Version = "1.0.1", UserId = userId, };
			
			User user = null;
			
			var we = new ManualResetEvent(false);			
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/AuthFacebookUser");
			
			JsonUtility.Upload (uri, authUser, false, s =>
			{
				try
				{
					var json = JsonArray.Load (s);
					int Id = json.ContainsKey("UserId") ? Convert.ToInt32(json["UserId"].ToString()) : 0;
					user = new User(){ Id = Id, Name = userName, FBUser = true };
				}
				catch (Exception ex)
				{
					Util.LogException("Authentificate", ex);
				}
				we.Set();
			});
			
			we.WaitOne(20000);
			
			return user;
		}		
		
		public FullUserResponse GetFullUserById(int id, int userId)
		{
			
			FullUserResponse fullUser = null;			
			var we = new ManualResetEvent(false);
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/GetUsers?FullUserId={0}&AskerId={1}", id, userId);
			
			JsonUtility.Launch(uri, false, s =>
	        {				
				try
				{
					var json = (JsonObject)JsonObject.Load (s);
					var obj = (JsonObject)json["User"];					
					
					User user =  JsonToUser(obj);
				 	int friendsCount = Convert.ToInt32(json["FriendsCount"].ToString());
					int followersCount = Convert.ToInt32(json["FollowersCount"].ToString());
					int followedCount = Convert.ToInt32(json["FollowedCount"].ToString());
					int imagesCount = Convert.ToInt32(json["ImagesCount"].ToString());
					int inRelation = Convert.ToInt32(json["InRelation"].ToString());
					
					fullUser = new FullUserResponse()
					{
						User = user,
						FriendsCount = friendsCount,
						FollowedCount = followedCount,
						FollowersCount = followersCount,
						ImagesCount = imagesCount,
						InRelation = inRelation,
					};
				}
				catch (Exception ex)
				{
					Util.LogException("GetFullUserById", ex);
				}
				we.Set();
			});
			
			we.WaitOne(10000);
			
			return fullUser;			
		}
		
		public List<User> GetAllUsersByName(string name)
		{				
			List<User> users = null;
			var we = new ManualResetEvent(false);
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/GetAllUsers?ByName={0}", name);
			
			JsonUtility.Launch(uri, false, s =>
	        {
				try
				{
					users = new List<User>();
					
					var json = (JsonObject)JsonObject.Load (s);
					foreach (JsonObject obj in json["Users"])
					{
						users.Add(JsonToUser(obj));
					}
				}
				catch (Exception ex)
				{
					Util.LogException("GetAllUsersByName", ex);
				}
				we.Set();
			});
			
			we.WaitOne(5000);
			
			return users;
		}
		
		public User GetUserById(int id)
		{
			User user = null;			
			var we = new ManualResetEvent(false);
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/GetUsers?UserId={0}", id);
			
			JsonUtility.Launch(uri, false, s =>
	        {
				try
				{
					var json = (JsonObject)JsonObject.Load (s);
					var obj = (JsonObject)json["User"];
					user =  JsonToUser(obj);
				}
				catch (Exception ex)
				{
					Util.LogException("GetUserById", ex);
				}
				we.Set();
			});
			
			we.WaitOne();
			
			return user;					
		}
		
		#endregion
				
		public static User JsonToUser(JsonObject obj)
		{
			var user = new User()
			{
				FBUser = obj.ContainsKey("IsFb") ? Convert.ToBoolean(obj["IsFB"].ToString()) : false,
				Id = Convert.ToInt32(obj["Id"].ToString()),
				Name = obj["Name"].ToString().Replace("\"", ""),
				HasPhoto = Convert.ToInt32(obj["HasPhoto"].ToString()),
			};
			return user;
		}		
	}
}
