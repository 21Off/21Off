
using System.Collections.Generic;
using System.Runtime.Serialization;
using SQLite;
namespace MSP.Client.DataContracts
{
	public class StoreNewUserResponse
	{
		public StoreNewUserResponse()
		{
			this.ResponseStatus = new ResponseStatus();
		}

		public int UserId { get; set; }

		public ResponseStatus ResponseStatus { get; set; }
	}
}
