using System;
using System.Collections.Generic;
using System.Json;
using System.Threading;
using MSP.Client.DataContracts;
using SQLite;
using System.Net;
using System.IO;

namespace MSP.Client
{
	public class CommentsService : AppServiceBase
	{
		public IEnumerable<Comment> GetCommentsOfImage(int id)
		{	
			var comments = new List<Comment>();
			
			var we = new ManualResetEvent(false);
			
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/Comments?ImageId={0}", id);
			JsonUtility.Launch(uri, false, s =>
	        {
				var json = JsonArray.Load (s);
				foreach (JsonObject obj in json["Comments"])
				{		
					try
					{
						comments.Add(JsonToComment(obj));
					}
					catch (Exception ex)
					{
						Util.LogException("GetCommentsOfImage", ex);
					}
				}
				we.Set();
			});
			
			we.WaitOne();
			
			return comments;
		}
		
		public static Comment JsonToComment(JsonObject obj)
		{
			var comment = new Comment()
			{
				Id = Convert.ToInt32(obj["Id"].ToString()),
				UserId = Convert.ToInt32(obj["UserId"].ToString()),
				ParentId = Convert.ToInt32(obj["ParentId"].ToString()),
				Name = obj["Name"].ToString().Replace("\"", ""),
				//Time =  DateTime.ParseExact (obj ["Time"], "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture),
			};
			
			DateTime? time = ActivitiesService.JsonToTime(obj["Time"]);
			if (time.HasValue)			
			{
				comment.Time = time.Value;
			}
			
			return comment;
		}
		
		public void PutNewComment(Comment comment)
		{
			var cms = new Comments() { Comment = comment };
			var uri = string.Format("http://storage.21offserver.com/json/syncreply/Comments");
			
			var request = (HttpWebRequest) WebRequest.Create (uri);
			request.Method = "PUT";	
			using (var reqStream = request.GetRequestStream())
			{
				ServiceStack.Text.JsonSerializer.SerializeToStream(cms, typeof(Comments), reqStream);
			};
			using (var response = request.GetResponse())
			{
				using (var stream = response.GetResponseStream())
				{
					var responseString = new StreamReader(stream).ReadToEnd();
				}
			}
		}
		
		public void DeleteComment(int idComment)
		{
			var we = new ManualResetEvent(false);			
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/comments?Id={0}", idComment);			
			
			JsonUtility.Launch(uri, false, s =>
			{
				try
				{
					var json = JsonArray.Load (s);
					Console.WriteLine(json.ToString());
				}
				catch (Exception ex)
				{
					Util.LogException("DeleteKeyword", ex);
				}
				we.Set();
			}, "DELETE");
			
			we.WaitOne(5000);
		}		
	}
		
	public class QueuedTask2 {
		[PrimaryKey, AutoIncrement]
		public int TaskId { get; set; }
		public long AccountId { get; set; }
		public string Url { get; set; }

		public string PostData { get; set; }
		public string Verb { get; set; }
	}	
}
