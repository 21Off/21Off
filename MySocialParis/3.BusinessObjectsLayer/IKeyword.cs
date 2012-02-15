
using System;
namespace MSP.Client
{
	public interface IKeyword
	{
		#region Properties
		
		/// <summary>Keyword identity - Primary key</summary>
		int Id {get;}	
		
		/// <summary>Keyword</summary>
		string Name {set;get;}
		
		/// <summary>Image id</summary>
		int ParentId {set;get;}
		
		#endregion Properties			
	}
}
