using System;

namespace MSP.Client
{
	public interface ILike
	{
		#region Properties
		
		/// <summary>Image identity - Primary key</summary>
		int Id {get;}
		
		/// <summary>Identity of user who made the like</summary>
		int UserId {get;set;}
		
		/// <summary>Image identity</summary>
		int ParentId {set;get;}
				
		/// <summary>Image shot time</summary>
		DateTime Time {set;get;}
		
		#endregion Properties			
	}	
}
