using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;
using System.Threading;

namespace MSP.Client
{
	public partial class PhotoLocationViewController : UIViewController, IReverseGeo
	{		
		#region Members
		
		private Map mapView;
		private MKPointAnnotation ann;
		private bool loaded = false;
		private UINavigationController _navController;
		private UIImage _photo;
		
		#endregion
		
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public PhotoLocationViewController (IntPtr handle) : base(handle)
		{
			
		}

		[Export("initWithCoder:")]
		public PhotoLocationViewController (NSCoder coder) : base(coder)
		{
			
		}
		
		public PhotoLocationViewController (UINavigationController navController, UIImage photo) 
			: base("PhotoLocationViewController", null)
		{
			_navController = navController;
			_photo = photo;
		}		
		
		#endregion
		
		CLLocationManager locationManager;
		
		void Initialize ()
		{
			mapView.OnTapped += HandleMapViewOnTouchEnded;
			mapView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			mapView.WillStartLoadingMap += (s, e) => 
			{ 
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true; 
			};
			mapView.MapLoaded += (s, e) => 
			{ 
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false; 
			};
			mapView.LoadingMapFailed += (s, e) => 
			{ 
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false; 
			};
			
			//mapView.MapType = MKMapType.Hybrid;
			mapView.ShowsUserLocation = false;
			//Set up the text attributes for the user location annotation callout
			string desc = "Your photo is here";
			mapView.UserLocation.Title = desc;
			mapView.UserLocation.Subtitle = "";
			
			locationManager = new CLLocationManager();
			locationManager.DesiredAccuracy = -1;
			//Be as accurate as possible
			locationManager.DistanceFilter = 50;
			//Update when we have moved 50 m			
			locationManager.UpdatedLocation += UpdatedLocation;
			
			/*
			if (!CLLocationManager.LocationServicesEnabled)
				ShowLocalisationFailedMessage();
			else
				locationManager.StartUpdatingLocation();
			*/
				
			locationManager.StartUpdatingLocation();					
		}
		
		private void ShowLocalisationFailedMessage()
		{
			var actionSheet = new UIActionSheet("Localisation failed. Go to Settings/Location Services/21 Off")
			{
				Style = UIActionSheetStyle.Default,
			};

			actionSheet.DestructiveButtonIndex = actionSheet.AddButton("Cancel");
			actionSheet.AddButton("Ok");
			actionSheet.CancelButtonIndex = actionSheet.DestructiveButtonIndex;
			actionSheet.Clicked += delegate (object s, UIButtonEventArgs args)
			{
				switch (args.ButtonIndex)
				{
					case 0: // Delete
					{			
						AppDelegateIPhone.AIphone.GotoToBuzz();
						break;
					}
					case 1: // Share
					{
						AppDelegateIPhone.AIphone.GotoToBuzz();
						break;
					}
					case 2: // Cancel
					{
						break;
					}
				}
			};
			
			actionSheet.ShowFromTabBar (AppDelegateIPhone.tabBarController.TabBar);
		}
		
		public override void ViewDidAppear (bool animated)
		{
			/*
			if (CLLocationManager.LocationServicesEnabled)
			{
				locationManager.StopUpdatingLocation();
				locationManager.StartUpdatingLocation();
			}
			*/
			base.ViewDidAppear (animated);
		}

		private void UpdatedLocation(object sender, CLLocationUpdatedEventArgs args)
		{
			const double LatitudeDelta = 0.002;
			//no. of degrees to show in the map
			const double LongitudeDelta = LatitudeDelta;
			
			var PosAccuracy = args.NewLocation.HorizontalAccuracy;
			if (PosAccuracy >= 0)
			{
				var coord = args.NewLocation.Coordinate;
				HandleUpdatedLocation(args.NewLocation);
			}
		}
		
		private void UnloadLocationManager()
		{			
			try
			{
				if (locationManager != null)
				{
					locationManager.StopUpdatingLocation();
					locationManager.Dispose();
					locationManager = null;
				}
			}
			catch (Exception ex)
			{
				Util.LogException("UnloadLocationManager", ex);
			}
		}
		
		public override void ViewDidDisappear(bool animated)
		{
			UnloadLocationManager();
			base.ViewDidDisappear(animated);
		}		
		
		#region Events
		
		void HandleMapViewOnTouchEnded (PointF obj)
		{
			ignoreMapUpdate = true;
			
			if (!loaded || !geolocalisationDone)
				return;
			
			mapView.ExclusiveTouch = true;
			var actualCenter = mapView.Region.Center;
			MKCoordinateSpan span = mapView.Region.Span;						
			
			double xPerc = (obj.X - mapView.Frame.Width / 2) / mapView.Frame.Width;
			double yPerc = (obj.Y - mapView.Frame.Height / 2) / mapView.Frame.Height;
			
			actualCenter.Latitude -= yPerc * span.LatitudeDelta;
			actualCenter.Longitude += xPerc * span.LongitudeDelta;
			
			PhotoLocation = new CLLocation(actualCenter.Latitude, actualCenter.Longitude);
			mapView.SetCenterCoordinate(actualCenter, true);
			
			RepositionAnnotation(actualCenter);
		}		
		
		private bool firstUpdate = true;
		private bool ignoreMapUpdate;
		private int localisationRetryCount = 0;
		
		void HandleUpdatedLocation (CLLocation newLocation)
		{
			if (ignoreMapUpdate)
				return;
			
			if (newLocation == null)
			{
				if (localisationRetryCount == 0)
				{
					localisationRetryCount++;
					
					Action action = () =>
					{
						Util.RequestLocation(HandleUpdatedLocation);
					};
					Util.ShowAlertSheet("No reply from network! Give another try? :)", View, action);				
				}
				else
				{
					localisationRetryCount++;
					Thread.Sleep(300);					
					Util.RequestLocation(HandleUpdatedLocation);
					
					if (localisationRetryCount == 4)
						localisationRetryCount = 0;
				}		
			}
			else
			{
				loaded = true;
				PhotoLocation = newLocation;
				
				var spanLocal = new MKCoordinateSpan (0.02, 0.02);
				var span = firstUpdate ? spanLocal : mapView.Region.Span;
				firstUpdate = false;
				
				var region = new MKCoordinateRegion (newLocation.Coordinate, span);
				mapView.SetRegion (region, false);
				mapView.SetCenterCoordinate (newLocation.Coordinate, false);
				//mapView.SelectAnnotation(mapView.UserLocation, false);				
				
				double radiusInMeters = 100d;    
				MKCircle circle = MKCircle.Circle(newLocation.Coordinate, radiusInMeters);
				mapView.AddOverlay(circle);					
				
				RepositionAnnotation(newLocation.Coordinate);
			}
		}
		
		void HandleOkBtnTouchUpInside (object sender, EventArgs e)
		{			
			if (PhotoLocation != null)
			{
				var photoPost = new PhotoPostViewController(_navController, _photo, PhotoLocation, LocationMapPhotoCapture);
				_navController.PushViewController(photoPost, true);
			}
		}

		void HandleBackBtnTouchUpInside (object sender, EventArgs e)
		{
			AppDelegateIPhone.AIphone.GotoToShare();
		}	

		#endregion		
		
		private void SetAddress(string address)
		{
			UnHandleGeocode();
			
			InvokeOnMainThread(()=>
            {
				if (ann != null)
					ann.Subtitle = address;
			});
		}
				
		public void ReverseGeocode (CLLocationCoordinate2D coord)
		{
			geolocalisationDone = false;
			
			string address = ReverseGeocoder.ReverseGeocode(coord, this);
			if (!string.IsNullOrWhiteSpace(address))
				InvokeOnMainThread(()=> SetAddress(address));
		}
		
		private void UnHandleGeocode()
		{
			geolocalisationDone = true;
		}
		
		private void RepositionAnnotation(CLLocationCoordinate2D location)
		{			
			string desc = "Your photo is here";
			
			if (ann == null)
			{
				ann = new MKPointAnnotation { Title = desc, Coordinate = location };				
			}
			else
			{
			 	mapView.RemoveAnnotation (ann);
				ann.Coordinate = location;
				SetAddress("...");
			}
			
			LocationMapPhotoCapture = ScreenCapture ();
			
			mapView.AddAnnotationObject (ann);	
			
			Action act = () => 
			{				
				ReverseGeocode(location);			                   
			};
			
			AppDelegateIPhone.ShowRealLoading(null, "Getting location", null, act, () => mapView.SelectAnnotation (ann, false));
		}		

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();			
			
			okBtn.SetImage(Graphics.GetImgResource("ok"), UIControlState.Normal);
			backBtn.SetImage(Graphics.GetImgResource("back"), UIControlState.Normal);			
			
			okBtn.TouchUpInside += HandleOkBtnTouchUpInside;
			backBtn.TouchUpInside += HandleBackBtnTouchUpInside;
						
			mapView = new Map()
			{
				UserInteractionEnabled = true,
				Frame = new RectangleF(0, 45, 320, 480 - 40 - 25),
			};
			this.View.Add(mapView);
			
			var topBar = new UIView(new RectangleF(0, 45, 320, 1));
			topBar.Layer.BackgroundColor = UIColor.LightGray.CGColor;
			this.View.AddSubview(topBar);
			
			/*
			if (!CLLocationManager.LocationServicesEnabled)
			{
				ShowLocalisationFailedMessage();	
			}
			else
			*/
			Initialize ();
		}

		/// <summary>
		/// Capture a copy of the current View and:
		/// * re-display in a UIImage control
		/// * save to the Photos collection
		/// * save to disk in the application's Documents folder
		/// </summary>
		public string ScreenCapture ()
		{
			var size = new SizeF (MemberPhotoCell.MiniMapWidth, MemberPhotoCell.MiniMapHeight);
			var documentsDirectory = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			
			UIGraphics.BeginImageContext (View.Frame.Size);
			var ctx = UIGraphics.GetCurrentContext ();
			if (ctx != null) {
				View.Layer.RenderInContext (ctx);
				UIImage img = UIGraphics.GetImageFromCurrentImageContext ();
				UIGraphics.EndImageContext ();
				
				var location = new PointF ((UIScreen.MainScreen.Bounds.Width - size.Width) / 2, 35 + (425 - size.Height) / 2);
				
				using (var screenImage = img.CGImage.WithImageInRect (new RectangleF (location, size))) {
					img = UIImage.FromImage (screenImage);
				}
				//UIImage img = UIImage.FromImage(CGImage.ScreenImage.WithImageInRect(new RectangleF(location, size)));
				
				 /*
			      img.SaveToPhotosAlbum(
			         (sender, args)=>{Console.WriteLine("image saved to Photos");}
			      );
			      */

				string png = Path.Combine (documentsDirectory, "Screenshot.jpg");
				NSData imgData = img.AsJPEG (0.8f);				
				NSError err = null;
				if (imgData.Save (png, false, out err)) {
					return png;
				} else {
					//Console.WriteLine ("NOT saved as" + png + " because" + err.LocalizedDescription);
					return null;
				}
			} else {
				Util.Log("ctx null - doesn't seem to happen");
				return null;
			}
		}
				
		private bool geolocalisationDone = false;
		private CLLocation PhotoLocation { get; set; }
		private string LocationMapPhotoCapture {get;set;}		

		#region IReverseGeo implementation
		public void OnFoundAddress (string address)
		{
			SetAddress(address);
		}
		
		void IReverseGeo.HandleGeoCoderDelOnFailedWithError (MKReverseGeocoder arg1, NSError arg2)
		{
			throw new NotImplementedException ();
		}

		void IReverseGeo.HandleGeoCoderDelOnFoundWithPlacemark (MKReverseGeocoder arg1, MKPlacemark placemark)
		{
			throw new NotImplementedException ();
		}
		#endregion
	}

}

