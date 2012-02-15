
using System;
namespace MSP.Client
{
	public interface IFollow
	{
		#region Properties
		
		/// <summary>Image identity - Primary key</summary>
		int Id {get;}
		
		/// <summary>Identity of user who is followed</summary>
		int UserId {get;set;}
		
		/// <summary>Identity of user who follows</summary>
		int FollowerId {set;get;}
				
		/// <summary>Image shot time</summary>
		DateTime Time {set;get;}
		
		#endregion Properties			
	}	
}
