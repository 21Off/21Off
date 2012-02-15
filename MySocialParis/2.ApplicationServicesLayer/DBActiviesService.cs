using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using System.Threading;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public class DBActiviesService
	{
		public IEnumerable<ActivityResponse> GetActivities(int userId)
		{
			return GetActivities(userId, DateTime.MinValue);
		}
		
		public IEnumerable<ActivityResponse> GetActivities(int userId, DateTime since)		
		{
			Console.WriteLine("DBActivities User: " +  userId + "  " + since);
			var dbCmd = Database.Main;
			
			IEnumerable<Activity> res = dbCmd.Table<Activity>().Where(el => el.UserId == userId);
			var activities = new List<Activity>();
			foreach (Activity act in res)
				activities.Add(act);
						
			if (since != DateTime.MinValue)
			{
               res = activities.Where(el => (el.IdOwner.HasValue && el.IdOwner.Value != userId) && 
				                       el.Time.Subtract(since) > TimeSpan.FromSeconds(1));                            			
			}
			else
				res = activities.Where(el => el.UserId == userId && (el.IdOwner.HasValue && el.IdOwner.Value != userId));								
						
			var responses = new List<ActivityResponse>();
			foreach (Activity act in res.OrderByDescending(a => a.Time))
			{
				var actResp = ActivityTransform(act, dbCmd);
				if (actResp != null)
					responses.Add(actResp);
			}
			
			Console.WriteLine("DBActivities " +  responses.Count);
			return responses;
		}
		
		public DateTime GetLastActivityTime(int userId)
		{			
			
			lock (Database.Main)
			{
				var lastAct = Database.Main.Table<Activity>().Where(a => a.UserId == userId)
					.OrderByDescending(a => a.Time).FirstOrDefault();
				
				if (lastAct != null)
				{
					return lastAct.Time;
				}
			}
			
			return DateTime.MinValue;
		}
		
		private ActivityResponse ActivityTransform(Activity el, Database dbCmd)
        {
            try
            {
                var actResp = new ActivityResponse
                {
                    Activity = el
                };

                if (el.IdOwner != null)
                    actResp.User = dbCmd.Get<User>(el.IdOwner);
                if (el.IdComment != null)
                    actResp.Comment = dbCmd.Get<Comment>(el.IdComment);
                if (el.IdPhoto != null)
                    actResp.Image = dbCmd.Get<Image>(el.IdPhoto);				

                return actResp;
            }
            catch (Exception ex)
            {
				Util.LogException("ActivityTransform", ex);
				return null;
            }

        }		
	}
}
