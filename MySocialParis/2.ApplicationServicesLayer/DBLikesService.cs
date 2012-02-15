
using System;
using System.Collections.Generic;
using MSP.Client.DataContracts;
using System.Threading;
using System.Json;
using System.Net;
using System.IO;
namespace MSP.Client
{
	public class DBLikesService : AppServiceBase
	{
		public IEnumerable<Like> GetLikesOfImage(int id)
		{
			lock (Database.Main)
				return Database.Main.Query<Like> ("SELECT * FROM Like where ParentId = ?", id);
		}
	}
}
