using System;
using System.Collections.Generic;
using MSP.Client.DataContracts;
using System.Threading;
using System.Json;
using System.Net;
using System.IO;

namespace MSP.Client
{	 
	public class LikesService : AppServiceBase
	{
		public IEnumerable<Like> GetLikesOfImage(int id)
		{
			//http://88.168.232.190/xml/syncreply/Likes?ImageId=4
			var likes = new List<Like>();			
			var we = new ManualResetEvent(false);
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/Likes?ImageId={0}", id);
			
			JsonUtility.Launch(uri, false, s =>
	        {
				try
				{
					var json = JsonArray.Load (s);
					foreach (JsonObject obj in json["Likes"])
					{		
						try
						{
							likes.Add(JsonToLike(obj));
						}
						catch (Exception ex)
						{
							Util.LogException("GetLikesOfImage", ex);
						}
					}
				}
				catch (Exception ex)
				{
					Util.LogException("GetLikesOfImage", ex);
				}
				we.Set();
			});
			
			we.WaitOne();
			
			return likes;			
		}
		
		public IEnumerable<FullLike> GetFullLikesOfImage(int id)
		{
			List<FullLike> likes = null;
			var we = new ManualResetEvent(false);
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/Likes?FullImageId={0}", id);
			
			JsonUtility.Launch(uri, false, s =>
	        {
				try
				{
					likes = new List<FullLike>();
					var json = JsonArray.Load (s);
					foreach (JsonObject obj in json["Likes"])
					{		
						try
						{
							var like = JsonToLike((JsonObject)obj["Like"]);
							var user = UsersService.JsonToUser((JsonObject)obj["User"]);
							var fLike = new FullLike() { Like = like, User = user };
							likes.Add(fLike);
						}
						catch (Exception ex)
						{
							Util.LogException("GetFullLikesOfImage", ex);
						}
					}
				}
				catch (Exception ex)
				{
					Util.LogException("GetFullLikesOfImage", ex);
				}
				we.Set();
			});
			
			we.WaitOne(5000);
			
			return likes;			
		}		
		
		public static Like JsonToLike(JsonObject obj)
		{
			var like = new Like()
			{				
				Id = Convert.ToInt32(obj["Id"].ToString()),
				ParentId= Convert.ToInt32(obj["ParentId"].ToString()),
				UserId = Convert.ToInt32(obj["UserId"].ToString()),
				
				//Time =  DateTime.ParseExact (obj ["Time"], "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture),				
			};
			
			DateTime? time = ActivitiesService.JsonToTime(obj["Time"]);
			if (time.HasValue)
			{
				like.Time = time.Value;
			}
			
			return like;
		}
		
		public int LikeImage(Like like)
		{
			return SendLike(like, true);	
		}
		
		public int DislikeImage(Like like)
		{
			return SendLike(like, false);
		}
		
		public int SendLike(Like like, bool isLike)
		{
			var cms = new SendLikeOp(){ Like = like, IsLike = isLike };
			var uri = string.Format("http://storage.21offserver.com/json/syncreply/SendLikeOp");
			
			var request = (HttpWebRequest) WebRequest.Create (uri);
			request.Method = "PUT"; // OR POST	
			using (var reqStream = request.GetRequestStream())
			{
				ServiceStack.Text.JsonSerializer.SerializeToStream(cms, typeof(SendLikeOp), reqStream);
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
