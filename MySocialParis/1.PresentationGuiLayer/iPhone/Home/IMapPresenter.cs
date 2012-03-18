using MonoTouch.CoreLocation;
using MonoTouch.MapKit;

namespace MSP.Client
{
	public interface IMapPresenter
	{
		MKMapView MapView { get; }
		CLLocation mylocation { get; set;}	
	}	
}
