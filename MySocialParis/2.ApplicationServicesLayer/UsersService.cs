using System;
using System.Json;
using System.Threading;
using MSP.Client.DataContracts;
using System.Collections.Generic;
using System.Reflection;
using MonoTouch.Foundation;

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

		public GetSubscribersBySocialIdsResponse GetSocialIds (List<long> socialIds, int socialType)
		{			
			var request = new GetSubscribersBySocialIds()
			{
				SocialIds = string.Join(",", socialIds),
				SocialType = socialType,
			};

			return GetServiceResponse<GetSubscribersBySocialIdsResponse>(request);
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
		
		public GetUserInfoResponse GetUserInfo(int userId)
		{
			return GetServiceResponse<GetUserInfoResponse>(new GetUserInfo() { UserId = userId });
		}
		
		public User Authentificate (string userName, string password)
		{
		    var bundleVersion = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString();
			var authUser = new AuthUser() { Password = password, UserName = userName, Version = bundleVersion };
			
			return GetServiceResponse(authUser, json=>
            {
				int Id = json.TryGetInt("UserId");
				return new User(){ Id = Id, Name = userName };
			});		
		}
		
		public User AuthentificateFacebook (string userName, decimal userId, string signedRequest = "password")
		{
			var bundleVersion = NSBundle.MainBundle.ObjectForInfoDictionary("CFBundleVersion").ToString();
			var authUser = new AuthFacebookUser() 
			{ 
				SignedRequest = signedRequest, 
				UserName = userName, 
				Version = bundleVersion, 
				UserId = userId, 
			};
			
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
			return GetServiceResponse<FullUserResponse>(new GetUsers() {FullUserId = id, AskerId = userId });				
		}
		
		public List<User> GetAllUsersByName(string name)
		{				
			var aaa = ServiceStack.Text.JsonSerializer.SerializeToString(new GetAllUsers(){ByName = name});
			List<User> users = GetServiceResponse<GetAllUsersResponse>(new GetAllUsers(){ByName = name}).Users;
			return users;
		}
		
		public User GetUserById(int id)
		{
			User user = GetServiceResponse<GetUsersResponse>(new GetUsers(){ UserId = id }).User;
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
