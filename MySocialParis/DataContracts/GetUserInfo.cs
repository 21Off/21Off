using System.Collections.Generic;
using System.Runtime.Serialization;
using SQLite;

namespace MSP.Client.DataContracts
{
	
	public class GetUserInfo
	{
		public long UserId {get;set;}
	}
	
	public class UpdateUserInfo
	{
		public long UserId {get;set;}
		public string Email {get;set;}
		public string UserName {get;set;}		
	}
	
	
	
	public class GeneralResponse
	{
		public GeneralResponse()
		{
			this.ResponseStatus = new ResponseStatus();
		}
		
		public ResponseStatus ResponseStatus { get; set; }
	}	
	
	public class GetUserInfoResponse
	{
		public GetUserInfoResponse()
		{
			this.ResponseStatus = new ResponseStatus();
		}
		
		public string Email {get;set;}
		public string UserName {get;set;}
		
		public ResponseStatus ResponseStatus { get; set; }
	}
		
}
