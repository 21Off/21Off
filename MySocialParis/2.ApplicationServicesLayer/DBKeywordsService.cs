
using System;
using System.Collections.Generic;
using MSP.Client.DataContracts;
using System.Threading;
using System.Json;
namespace MSP.Client
{
	public class DBKeywordsService : AppServiceBase
	{
		public IEnumerable<Keyword> GetKeywordsOfImage(int id)
		{
			lock (Database.Main)
				return Database.Main.Query<Keyword> ("SELECT * FROM Keyword where ParentId = ?", id);			
		}
	}
}
