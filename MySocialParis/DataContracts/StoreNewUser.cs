
using System.Collections.Generic;
using System.Runtime.Serialization;
using SQLite;
namespace MSP.Client.DataContracts
{
	/// <summary>
	/// Use Plain old DataContract's Define your 'Service Interface'
	/// 
	/// This example introduces the concept of a generic 'ResponseStatus' that 
	/// your service client can use to assert that the request was successful.
	/// The ResponseStatus DTO also enables you to serialize an exception in your service.
	/// </summary>
	public class StoreNewUser
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
        public IOFile ImageFile { get; set; }
		public string Version { get; set; }
	}
}
