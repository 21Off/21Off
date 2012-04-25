
using MonoTouch.Foundation;
using MonoTouch.MapKit;

namespace MSP.Client
{
	public interface IReverseGeo
	{
		void HandleGeoCoderDelOnFailedWithError (MKReverseGeocoder arg1, NSError arg2);
		void HandleGeoCoderDelOnFoundWithPlacemark (MKReverseGeocoder arg1, MKPlacemark placemark);
		
		void OnFoundAddress(string address);
	}
}
