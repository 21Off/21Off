
using System;
using System.Drawing;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;
namespace MSP.Client
{
	public interface IMapPresenter
	{
		MKMapView MapView { get; }
		CLLocation mylocation { get; set;}	
	}	
}
