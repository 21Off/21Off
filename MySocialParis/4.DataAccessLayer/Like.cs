using System;
using System.Linq;
using SQLite;
using System.Runtime.Serialization;

namespace MSP.Client
{
	public class Like : ILike
	{		
		//[AutoIncrementAttribute]
		[PrimaryKey]
		[DataMember]
		public int Id {get; set;}
		
		[DataMember]
		public int UserId {get; set;}
		
		[DataMember]
		public int ParentId {set; get;}
		
		[DataMember]
		public DateTime Time {set; get;}
	}
}
