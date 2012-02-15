
using System;
using System.Collections.Generic;
using MSP.Client.DataContracts;
namespace MSP.Client
{
	public class DBFollowersService : AppServiceBase
	{
		public IEnumerable<Follow> GetFollowersOfUser(int id)
		{
			lock (Database.Main)
				return Database.Main.Query<Follow>("select * from Follow where UserId = ?", id);
		}
		
		public IEnumerable<Follow> GetFollowedByUser(int id)
		{
			lock (Database.Main)
				return Database.Main.Query<Follow>("select * from Follow where FollowerId = ?", id);
		}
		
		public void FollowUser(Follow follow)
		{
			lock (Database.Main)
			{
				var followers = GetFollowersOfUser(follow.UserId);
				
				bool alreadyFollowing = false;
				Follow existentRel = null;
				foreach (Follow follower in followers)
				{
					if (follower.FollowerId == follow.FollowerId)
					{
						alreadyFollowing = true;
						existentRel = follower;
						break;
					}
				}
				
				if (alreadyFollowing)
					Database.Main.Delete<Follow>(existentRel);
				else
					Database.Main.Insert(follow);
			}			
		}
	}
}
