using SQLite;
using System.Runtime.Serialization;

namespace MSP.Client
{
	public class Keyword : IKeyword
	{
		//[AutoIncrementAttribute]
		[PrimaryKey]
		[DataMember]
		public int Id {get; set;}
				
		[Indexed]
		[DataMember]
		public string Name {set; get;}
		
		[DataMember]
		public int ParentId {set;get;}
	}
}
