using System;

namespace MSP.Client
{
	/// <summary>IImage</summary>
	public interface IImage
	{
		#region Properties
		
		/// <summary>Image identity - Primary key</summary>
		int Id {get;}
		
		/// <summary>Identity of user who took the image</summary>
		int UserId {get;set;}
		
		/// <summary>Image name</summary>
		string Name {set;get;}
		
		/// <summary>Image shot time</summary>
		DateTime Time {set;get;}
		
		/// <summary>Image shot position latitude</summary>
		double Latitude {set;get;}
		
		/// <summary>Image shot position longitude</summary>
		double Longitude {set;get;}
		
		/// <summary>Image shot position altitude</summary>
		double Altitude {set;get;}
		
		#endregion Properties
	}
}

