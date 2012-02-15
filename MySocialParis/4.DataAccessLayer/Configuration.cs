using System;
using System.Linq;
using System.Runtime.Serialization;
using SQLite;

namespace MSP.Client
{
	
	
	public class Configuration : IConfiguration
	{
		[AutoIncrementAttribute]
		[PrimaryKey]
		[DataMember]
		public int Id {get; set;}	

		[Indexed]
		[DataMember]
		public string Login {set; get;}
		[DataMember]
		public string Password {set; get;}
		[DataMember]
		public string Email {set; get;}
	}
}
