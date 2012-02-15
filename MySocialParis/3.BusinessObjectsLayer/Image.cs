using System;
namespace MSP.Client
{
	public class Image : IImage
	{
		#region Constructors
		
		public Image ()
		{

		}
		
		#endregion Constructors
		
		#region Properties
		
		int Id {get;}
		int Category {set;get;}
		string Name {set;get;}
		DateTime Time {set;get;}
		int Latitude {set;get;}
		int Longitude {set;get;}
		int Altitude {set;get;}
		string UrlString {get;}
		
		#endregion Properties
		
		#region Methods

		#endregion Methods
	}
}

