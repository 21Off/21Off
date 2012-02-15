using System;
using System.Threading;
using MonoTouch.CoreLocation;

namespace MSP.Client
{
	
	public static class MapExtensions{
		
		public static double ToRadian(this double d)
		{
			return d * (Math.PI /180);
		}
		public static double ToDegree(this double r)
		{
			return r * (180 / Math.PI);
		}
	}
	public enum UnitsOfLength
	{Miles,Kilometer,NauticalMiles }
	
	/// <remarks>
	/// Code from
	/// http://bryan.reynoldslive.com/post/Latitude2c-Longitude2c-Bearing2c-Cardinal-Direction2c-Distance2c-and-C.aspx
	/// </remarks>
	public class MapHelper
	{
		private static double _MilesToKilometers = 1.609344 ;
		private static double _MilesToNautical = 0.868976242;
		public MapHelper ()
		{}
		
		/// <summary>
		/// Calculates the distance between two points of latitude and longitude.
		/// Great Link - http://www.movable-type.co.uk/scripts/latlong.html
		/// </summary>
		/// <param name="coordinate1">First coordinate.</param>
		/// <param name="coordinate2">Second coordinate.</param>
		/// <param name="unitsOfLength">Sets the return value unit of length.</param>
		public static Double Distance(Coordinate coordinate1, Coordinate coordinate2, UnitsOfLength unitsOfLength)
		{
		   var theta = coordinate1.Longitude - coordinate2.Longitude;
		   var distance = Math.Sin(coordinate1.Latitude.ToRadian()) * Math.Sin(coordinate2.Latitude.ToRadian()) +
		                  Math.Cos(coordinate1.Latitude.ToRadian()) * Math.Cos(coordinate2.Latitude.ToRadian()) *
		                  Math.Cos(theta.ToRadian());
		
		   distance = Math.Acos(distance);
		   distance = distance.ToDegree();
		   distance = distance * 60 * 1.1515;
		
		   if (unitsOfLength == UnitsOfLength.Kilometer)
		       distance = distance * _MilesToKilometers;
		   else if (unitsOfLength == UnitsOfLength.NauticalMiles)
		       distance = distance * _MilesToNautical;
		
		   return (distance);
		}
		
	}
	
	public class Coordinate
      {
          private double latitude, longitude;
    
			public Coordinate(){}
			
			public Coordinate(CLLocationCoordinate2D mapCoord)
			{
				latitude = mapCoord.Latitude;
				longitude = mapCoord.Longitude;
			}
			
          /// <summary>
          /// Latitude in degrees. -90 to 90
          /// </summary>
          public Double Latitude
          {
              get { return latitude; }
              set
              {
                  if (value > 90) throw new ArgumentOutOfRangeException("value", "Latitude value cannot be greater than 90.");
                  if (value < -90) throw new ArgumentOutOfRangeException("value", "Latitude value cannot be less than -90.");
                  latitude = value;
              }
          }
   
          /// <summary>
          /// Longitude in degree. -180 to 180
         /// </summary>
          public Double Longitude
          {
              get { return longitude; }
              set
             {
                  if (value > 180) throw new ArgumentOutOfRangeException("value", "Longitude value cannot be greater than 180.");
                  if (value < -180) throw new ArgumentOutOfRangeException("value", "Longitude value cannot be less than -180.");
                  longitude = value;
              }
          }
      }	
	
	public static class MapUtilities
	{		
		public static double DistanceBetween(CLLocationCoordinate2D coord1, CLLocationCoordinate2D coord2)
		{
			try
			{
				double lat1 = coord1.Latitude;
				double lat2 = coord2.Latitude;
				
				double lon1 = coord1.Longitude;
				double lon2 = coord2.Longitude;
				
				var R = 6371; // km
				var dLat = (lat2-lat1).toRad();
				var dLon = (lon2-lon1).toRad();
				lat1 = lat1.toRad();
				lat2 = lat2.toRad();
				
				var a = Math.Sin(dLat/2) * Math.Sin(dLat/2) +
				        Math.Sin(dLon/2) * Math.Sin(dLon/2) * Math.Cos(lat1) * Math.Cos(lat2); 
				var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1-a)); 
				var d = R * c;
				
				return d;
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				return double.MaxValue;
			}
		}
		
		public static double toRad(this double d)
		{
			return d * Math.PI / 180;
		}
		
		public static CLLocation GetActualLocation()
		{
			CLLocation location = null;
			
			try
			{
				var waitEvent = new ManualResetEvent(false);
				
				ThreadPool.QueueUserWorkItem(o =>
             	{
					try
					{
						Util.RequestLocation(el =>
		             	{
							location = el;
							waitEvent.Set();
						});
					}
					catch (Exception ex)
					{
						Util.LogException("GetActualLocation", ex);
						waitEvent.Set();
					}
						
				});
				
				waitEvent.WaitOne(TimeSpan.FromSeconds(10));
			}
			catch (Exception ex)
			{
				Util.LogException("GetActualLocation", ex);
			}
			
			return location;
		}
	}
}
