using System;
using System.Linq;
using System.Runtime.Serialization;
using SQLite;

namespace MSP.Client
{
	public class Follow : IFollow
	{
		//[AutoIncrementAttribute]
		[PrimaryKey]
		[DataMember]
		public int Id {get; set;}
		
		[DataMember]
		public int UserId {get; set;}
		
		[DataMember]
		public int FollowerId {set; get;}
		
		[DataMember]
		public DateTime Time {set; get;}		
	}
}
