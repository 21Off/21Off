using System;
using System.Collections.Generic;
using System.Threading;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.MapKit;

namespace MSP.Client
{
	
	public static class ReverseGeocoder
	{
		public static Dictionary<CLLocationCoordinate2D, MKPlacemark> coordinates;
		public static Dictionary<CLLocationCoordinate2D, List<IReverseGeo>> pendingRequests;
		// A queue used to avoid flooding the network stack with HTTP requests
		static Stack<CLLocationCoordinate2D> requestQueue;
		static object lockGeocoder = new object();
		
		static NSString nsDispatcher = new NSString ("x");
		
		public static GeoCoderDelegate geoCoderDel;
		
		static ReverseGeocoder()
		{
			coordinates = new Dictionary<CLLocationCoordinate2D, MKPlacemark>();
			pendingRequests = new Dictionary<CLLocationCoordinate2D, List<IReverseGeo>>();
			requestQueue = new Stack<CLLocationCoordinate2D>();
			
			geoCoderDel = new GeoCoderDelegate ();
			geoCoderDel.OnFoundWithPlacemark += HandleGeoCoderDelOnFoundWithPlacemark;
			geoCoderDel.OnFailedWithError+= HandleGeoCoderDelOnFailedWithError;			
		}
		
		public static MKPlacemark ReverseGeocode (CLLocationCoordinate2D coord, IReverseGeo reverseGeo)
		{
			lock (lockGeocoder)
			{
				if (coordinates.ContainsKey(coord))
					return coordinates[coord];
				
				if (pendingRequests.ContainsKey(coord))
				{
					pendingRequests[coord].Add(reverseGeo);
					return null;
				}
				pendingRequests[coord] = new List<IReverseGeo>() { reverseGeo };
				
				if (geoCoderDownloaders >= 1)
				{
					requestQueue.Push(coord);
				}
				else
				{
					StartAsyncGeolocalisation(coord);
				}
			}
			
			return null;
		}
		
		private static MKReverseGeocoder geoCoder;
		
		private static void StartAsyncGeolocalisation(CLLocationCoordinate2D coord)
		{
			//Util.Log("StartAsyncGeolocalisation for coord: Lat-{0}, Long-{1}", coord.Latitude, coord.Longitude);
			
			Interlocked.Increment (ref geoCoderDownloaders);
			try
			{				
				geoCoder = new MKReverseGeocoder (coord);
				geoCoder.Delegate = geoCoderDel;
				waitEnd.Reset();
				
				geoCoder.Start ();
				
				ThreadPool.QueueUserWorkItem(WaitGeolocCallback, geoCoder);
			}
			catch (Exception ex)
			{
				Util.LogException("ReverseGeocode", ex);
			}			
		}
				
		private static void WaitGeolocCallback(object state)
		{
			try
			{
				MKReverseGeocoder g = (MKReverseGeocoder)state;
				bool endedRight = waitEnd.WaitOne(5000);
				if (!endedRight)
				{
					HandleGeoCoderDelOnFailedWithError(g, new NSError(new NSString("Geolocalisation takes more that 5 seconds"), 666));						
					g = null;
				}
			}
			catch (Exception ex)
			{
				Util.LogException("WaitGeolocCallback", ex);
			}
		}
		
		private static ManualResetEvent waitEnd = new ManualResetEvent(true);
		
		private static int geoCoderDownloaders = 0;
		
		static void HandleGeoCoderDelOnFailedWithError (MKReverseGeocoder arg1, NSError arg2)
		{
			try
			{
				var coordinate = arg1.coordinate;
				Interlocked.Decrement (ref geoCoderDownloaders);
				
				//Util.Log("FailedWithError for coord: Lat-{0}, Long-{1}", coordinate.Latitude, coordinate.Longitude);							
				
				var list = new List<IReverseGeo>();
				lock (lockGeocoder)
				{			      
					list = pendingRequests[arg1.coordinate];
					pendingRequests.Remove(arg1.coordinate);
				}
				
				nsDispatcher.BeginInvokeOnMainThread(()=>
	            {
					foreach (IReverseGeo reverseGeo in list)
					{
						reverseGeo.HandleGeoCoderDelOnFailedWithError(arg1, arg2);
					}
				});
				
				//Go to the next geolocalisation request
				lock (lockGeocoder)
				{
					if (requestQueue.Count > 0)
					{
						CLLocationCoordinate2D coord = requestQueue.Pop();
						StartAsyncGeolocalisation(coord);
					}
				}
			}
			catch (Exception ex)
			{
			}
		}

		static void HandleGeoCoderDelOnFoundWithPlacemark (MKReverseGeocoder arg1, MKPlacemark placemark)
		{
			try
			{
				var coordinate = arg1.coordinate;
				waitEnd.Set();
				
				//Dont take in account the geolocalisations that arrive after x seconds
				if (arg1 != geoCoder)
					return;
				
				//Util.Log("FoundWithPlacemark for coord: Lat-{0}, Long-{1}", coordinate.Latitude, coordinate.Longitude);
				
				if (arg1 == geoCoder)
					Interlocked.Decrement (ref geoCoderDownloaders);
				
				var list = new List<IReverseGeo>();
				lock (lockGeocoder)
				{
					coordinates[arg1.coordinate] = placemark;
					
					list = pendingRequests[arg1.coordinate];
					pendingRequests.Remove(arg1.coordinate);
				}
				
				nsDispatcher.BeginInvokeOnMainThread(()=>
	            {
					foreach (IReverseGeo reverseGeo in list)
					{
						reverseGeo.HandleGeoCoderDelOnFoundWithPlacemark(arg1, placemark);
					}
				});
							
				//Go to the next geolocalisation request
				lock (lockGeocoder)
				{
					if (requestQueue.Count > 0)
					{
						CLLocationCoordinate2D coord = requestQueue.Pop();
						StartAsyncGeolocalisation(coord);
					}
				}
			}
			catch (Exception ex)
			{
			}
		}		
	}
}

