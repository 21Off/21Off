using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Threading;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public class ActivitiesService : AppServiceBase
	{		
		private int GetCountFromStream(Stream s)
		{
			try
			{
				JsonObject json = (JsonObject)JsonArray.Load (s);
			 	return Convert.ToInt32(json["Count"].ToString());
			}
			catch (Exception ex)
			{
				return 0;
			}			
		}
		
		private static List<ActivityResponse> LoadActivitiesFromStream(Stream s)
		{	
			var activityResponses = new List<ActivityResponse>();
			JsonObject json = (JsonObject)JsonArray.Load (s);
			foreach (JsonObject obj in json["Activities"])
			{
				try
				{
					JsonObject act = (JsonObject)obj["Activity"];
					JsonObject cmt = null;
					if (obj.ContainsKey("Comment"))
					{
					 	cmt = (JsonObject)obj["Comment"];
					}
					JsonObject usr = null;
					if (obj.ContainsKey("User"))
					{
					    usr = (JsonObject)obj["User"];
					}					
					JsonObject image = null;
					if (obj.ContainsKey("Image"))
					{
					    image = (JsonObject)obj["Image"];
					}
					
					//JsonObject respStat = (JsonObject)obj["ResponseStatus"];
					var actResp = new ActivityResponse()
					{							
						Activity = JsonToActivity(act),
						Comment = cmt == null ? null : CommentsService.JsonToComment(cmt),
						User = usr == null ? null : UsersService.JsonToUser(usr),
						Image = image == null ? null : ImagesService.JsonToImage(image),
					};
					activityResponses.Add(actResp);
				}
				catch (Exception ex)
				{
					Util.LogException("GetActivities", ex);
				}
			}
			return activityResponses;
		}	
		
		public int GetNotificationsCountSince(int userId, long since)
		{
			int count = 0;
			var we = new ManualResetEventSlim(false);
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/Activities?UserId={0}&CountSince={1}", 
							userId, since);
			
			JsonUtility.Launch(uri, false, s =>
	        {
				count = GetCountFromStream(s);				
				we.Set();
			});	
			
			we.Wait(6000);
			return count;
		}
		
		public IEnumerable<ActivityResponse> GetActivities(int userId, DateTime since)		
		{
			//string sinceTime = ServiceStack.Text.JsonSerializer.SerializeToString(DateTime.UtcNow, typeof(DateTime));
			var we = new ManualResetEventSlim(false);
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/Activities?UserId={0}&Since={1}&Count=20", 
							userId, since.Ticks);
			
			List<ActivityResponse> activities = null;
			JsonUtility.Launch(uri, false, s =>
	        {
				activities = LoadActivitiesFromStream(s);
				we.Set();
			});
			
			we.Wait(6000);
			if (activities == null)
				return null;
			
			var dbCmd = Database.Main;
			lock (Database.Main)
			{
				foreach (ActivityResponse actResp in activities)
				{	
					/*
					dbCmd.Insert(actResp.Activity, "OR REPLACE");
					*/
				}
			}
			return activities;
		}
		
		private static readonly DateTime unixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);		
		
		public static DateTime? JsonToTime(JsonValue json)
		{
 			//"/Date(1311694174896+0000)/"
			string jsonStr = json.ToString();
			jsonStr = jsonStr.Substring(7);
			jsonStr = jsonStr.Substring(0, jsonStr.IndexOf("+"));
			double ms = 0;
			if (double.TryParse(jsonStr, out ms))
			{
				return unixEpoch.AddMilliseconds(ms);				
			}
			return null;
		}
		
		public static Activity JsonToActivity(JsonObject obj)
		{
			/*
			<Id>2</Id>
			<IdComment i:nil="true"/>
			<IdOwner>1</IdOwner>
			<IdPhoto>5</IdPhoto>
			<Time>2011-07-26T18:12:01.5371093+02:00</Time>
			<Type>0</Type>
			<UserId>2</UserId>
			*/
			
			var act = new Activity()
			{ 
				Id = Convert.ToInt32(obj["Id"].ToString()),
				UserId = Convert.ToInt32(obj["UserId"].ToString()),
				Type = Convert.ToInt32(obj["Type"].ToString()),
				//Time =  DateTime.ParseExact (obj ["Time"], "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture),
			};
			
			DateTime? time = JsonToTime(obj["Time"]);
			if (time.HasValue)
			{
				act.Time = time.Value;
			}
			
			int id;
			if (obj.ContainsKey("IdComment"))
			{
				if (int.TryParse(obj["IdComment"].ToString(), out id))
					act.IdComment = id;					
			}
			if (obj.ContainsKey("IdOwner"))
			{
				if (int.TryParse(obj["IdOwner"].ToString(), out id))
					act.IdOwner = id;					
			}
			if (obj.ContainsKey("IdPhoto"))
			{
				if (int.TryParse(obj["IdPhoto"].ToString(), out id))
					act.IdPhoto = id;				                                      
			}
			if (obj.ContainsKey("IdShare"))
			{
				if (int.TryParse(obj["IdShare"].ToString(), out id))
					act.IdShare = id;				                                      
			}			
			
			return act;
		}
	}	
}
