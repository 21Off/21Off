
using System;
using System.Collections.Generic;
using System.Json;
using System.Threading;
using MSP.Client.DataContracts;
namespace MSP.Client
{
	public class DBCommentsService : AppServiceBase
	{
		public IEnumerable<Comment> GetCommentsOfImage(int id)
		{
			lock (Database.Main)
			{
				 return Database.Main.Query<Comment> ("SELECT * FROM Comment where ParentId = ?", id);
			}			
		}
		
		public void PutNewComment(Comment comment)
		{
			lock (Database.Main)
			{
				Database.Main.Insert(comment);
			}
		}
	}
}
