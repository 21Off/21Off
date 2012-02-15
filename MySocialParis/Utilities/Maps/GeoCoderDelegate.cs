using System;
using MonoTouch.MapKit;
using MonoTouch.Foundation;

namespace MSP.Client
{
	public class GeoCoderDelegate : MKReverseGeocoderDelegate
	{
		IMapPresenter _MapPresenter;
		
		public event Action<MKReverseGeocoder, MKPlacemark> OnFoundWithPlacemark;
		public event Action<MKReverseGeocoder, NSError> OnFailedWithError;		

		public GeoCoderDelegate (IMapPresenter mapPresenter)
		{
			_MapPresenter = mapPresenter;
		}
		
		public GeoCoderDelegate()
		{
		}

		/// <summary>
		/// When the reverse geocode finds a location, it calls this method
		/// which puts the placemark on the map as an Annotation
		/// </summary>
		public override void FoundWithPlacemark (MKReverseGeocoder geocoder, MKPlacemark placemark)
		{
			/*
			Console.WriteLine (placemark.PostalCode + "\n" + placemark.SubThoroughfare + "\n" 
			                   + placemark.Thoroughfare + "\n" + placemark.SubLocality + "\n"
			                   + placemark.Locality + "\n" + placemark.SubAdministrativeArea + "\n"
			                   + placemark.AdministrativeArea + "\n" + placemark.Country + "\n");
		    */               
			
			if (OnFoundWithPlacemark == null)
			{							
				try 
				{
					_MapPresenter.MapView.AddAnnotationObject (placemark);
				} 
				catch (Exception ex) 
				{
					Console.WriteLine ("FoundWithPlacemark" + ex.Message);
				}
			}
			else
				OnFoundWithPlacemark(geocoder, placemark);
					
		}
		/// <summary>
		/// Exposed by MonoTouch, just override to make it work
		/// </summary>
		public override void FailedWithError (MKReverseGeocoder gc, NSError error)
		{
			if (OnFailedWithError == null)
			{
				Console.WriteLine ("Reverse Geocoder failed");
			}
			else
				OnFailedWithError(gc, error);
		}
	}
}

