using System;
using System.Linq;
using System.Runtime.Serialization;
using SQLite;

namespace MSP.Client
{
	public class Comment : IComment
	{		
		//[AutoIncrementAttribute]
		[PrimaryKey]
		[DataMember]
		public int Id {get; set;}
		
		[DataMember]
		public int UserId {get; set;}
		
		[DataMember]
		public int ParentId {set; get;}
		
		[Indexed]
		[DataMember]
		public string Name {set; get;}
		
		[DataMember]
		public DateTime Time {set; get;}
	}
}
