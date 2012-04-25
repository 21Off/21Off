using System;
using System.Collections.Generic;
using System.Threading;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using System.Net;
using System.Threading.Tasks;

namespace MSP.Client
{	
	public static class ReverseGeocoder
	{
		public static Dictionary<CLLocationCoordinate2D, string> addressHist;
		public static Dictionary<CLLocationCoordinate2D, List<IReverseGeo>> pendingRequests;
		// A queue used to avoid flooding the network stack with HTTP requests
		static Stack<CLLocationCoordinate2D> requestQueue;
		static object lockGeocoder = new object();
		
		static NSString nsDispatcher = new NSString ("x");
		
		public static GeoCoderDelegate geoCoderDel;
		
		static ReverseGeocoder()
		{
			addressHist = new Dictionary<CLLocationCoordinate2D, string>();			
			pendingRequests = new Dictionary<CLLocationCoordinate2D, List<IReverseGeo>>();
			requestQueue = new Stack<CLLocationCoordinate2D>();
			
			geoCoderDel = new GeoCoderDelegate ();
			geoCoderDel.OnFoundWithPlacemark += HandleGeoCoderDelOnFoundWithPlacemark;
			geoCoderDel.OnFailedWithError+= HandleGeoCoderDelOnFailedWithError;			
		}
		
		public static string ReverseGeocode (CLLocationCoordinate2D coord, IReverseGeo reverseGeo)
		{
			lock (lockGeocoder)
			{
				if (addressHist.ContainsKey(coord))
					return addressHist[coord];			
				
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
				waitEnd.Reset();
				
				Task.Factory.StartNew(()=>
                {
					var res = GetCoordinates(coord);
					
					if (res != null && res.results != null && res.results.Count > 0)
						OnFoundAddress(coord, res.results[0].formatted_address);
				});
				
				
				return;
								
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
		
		public static Geo GetCoordinates(CLLocationCoordinate2D coord)
		{
			using (var client = new WebClient())
			{
				Uri uri = new Uri(string.Format("http://maps.googleapis.com/maps/api/geocode/json?latlng={0},{1}&sensor=false", coord.Latitude, coord.Longitude));
		
				/* The first number is the status code,
				* the second is the accuracy,
				* the third is the latitude,
				* the fourth one is the longitude.
				*/
				string geocodeInfo = client.DownloadString(uri);
		
				return Des<Geo>(geocodeInfo);
			}
		}
		
		public static T Des<T>(string s)
		{
			return ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(s);
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
		
		static void OnFoundAddress(CLLocationCoordinate2D coordinate, string address)
		{
			try
			{
				waitEnd.Set();
				
				//Dont take in account the geolocalisations that arrive after x seconds
				/*
				if (arg1 != geoCoder)
					return;
				*/					
				
				Interlocked.Decrement (ref geoCoderDownloaders);
				
				var list = new List<IReverseGeo>();
				lock (lockGeocoder)
				{
					addressHist[coordinate] = address;
					
					list = pendingRequests[coordinate];
					pendingRequests.Remove(coordinate);
				}
				
				nsDispatcher.BeginInvokeOnMainThread(()=>
	            {
					foreach (IReverseGeo reverseGeo in list)
					{
						reverseGeo.OnFoundAddress(address);
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
			if (placemark != null)
			{
				string address = placemark.SubThoroughfare + " " + placemark.Thoroughfare;
				OnFoundAddress(arg1.coordinate, address);
			}
		}
	}
			
	public class Geo
	{
		public string status {get;set;}
		public List<Result>results {get;set;}
	}
	
	public class Result
	{
		public string formatted_address {get;set;}
	}
}

