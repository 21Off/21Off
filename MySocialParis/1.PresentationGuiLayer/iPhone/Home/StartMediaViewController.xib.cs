using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using MonoTouch.CoreLocation;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using MonoTouch.MapKit;

namespace MSP.Client
{
	public partial class StartMediaViewController : UIViewController, IMapLocationRequest
	{		
		#region Private members
		
		private TimelineViewController likedMediaView;
		private TimelineViewController friendsMediaView;
		private TimelineViewController recentMediaView;
		private double _CurrentPage;
		private MSPNavigationController _MSP;
		
		#endregion
					
		public StartMapViewController Map {get;set;}
		public bool IsMap {get;set;}		

		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public StartMediaViewController (IntPtr handle) : base(handle)
		{
			IsMap = false;
		}

		[Export("initWithCoder:")]
		public StartMediaViewController (NSCoder coder) : base(coder)
		{

		}

		public StartMediaViewController () : base("StartMediaViewController", null)
		{
		}

		public StartMediaViewController (MSPNavigationController msp) 
			: base("StartMediaViewController", null)
		{
			_MSP = msp;
			LocType = LocalisationType.Global;	
			//mapView = new MKMapView();
		}
		
		//private MKMapView mapView;
		private CLLocationManager locationManager;
		
		#endregion		
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();			
			
			Initialize();
			SetEmptyImages();
			InitializeMap(new RequestInfo(RequestType.FirstLoad));
		}		
		
		public static string GetSubTitle(LocalisationType locType, int screen)
		{
			if (locType == LocalisationType.Global)
			{				
				if (screen == 1) return "       trendy posts worldwide       >";
				if (screen == 2) return "<       friend's posts worldwide     >";
				if (screen == 3) return "<       recent posts worldwide";
			}
			if (locType == LocalisationType.Local)
			{
				if (screen == 1) return "      trendy posts around you        >";
				if (screen == 2) return "<     friend's posts around you    >";
				if (screen == 3) return "<      recent posts around you";
			}
			return string.Empty;
		}
		
		private void Initialize()
		{
			//mapView.ShowsUserLocation = false;
			
			locationManager = new CLLocationManager();
			locationManager.DesiredAccuracy = -1;
			//Be as accurate as possible
			locationManager.DistanceFilter = 50;
			//Update when we have moved 50 m
			locationManager.HeadingFilter = 1;
			//Update when heading changes 1 degree 
			//locationManager.UpdatedHeading += UpdatedHeading;
			locationManager.UpdatedLocation += UpdatedLocation;		
			
			globalBtn.SetImage(Graphics.GetImgResource("global"), UIControlState.Normal);
			localBtn.SetImage(Graphics.GetImgResource("local"), UIControlState.Normal);
			mapBtn.SetImage(Graphics.GetImgResource("map"), UIControlState.Normal);
			
			globalBtn.TouchUpInside += HandleGlobalBtnTouchUpInside;
			localBtn.TouchUpInside += HandleLocalBtnTouchUpInside;
			
			var likedRootElement = new RootElement ("liked") { UnevenRows = true };
			var recentRootElement = new RootElement ("recent") { UnevenRows = true };
			var friendsRootElement = new RootElement ("friends") { UnevenRows = true };		
				
			var likedSection = new Section(GetSubTitle(LocType, 1));
			var friendsSection = new Section(GetSubTitle(LocType, 2));
			var recentSection = new Section(GetSubTitle(LocType, 3));			
			
			likedRootElement.Add(likedSection);
			recentRootElement.Add(recentSection);
			friendsRootElement.Add(friendsSection);
			
			float width = UIScreen.MainScreen.Bounds.Width;
			float topY = 42;
			SizeF size = new SizeF(width, 480 - topY);
			
			likedMediaView = new TimelineViewController(FilterType.Liked, true, _MSP, this) { Root = likedRootElement, };
			friendsMediaView = new TimelineViewController(FilterType.Friends, true, _MSP, this) { Root = friendsRootElement, };			
			recentMediaView = new TimelineViewController(FilterType.Recent, true, _MSP, this) { Root = recentRootElement, };
			
			likedMediaView.View.Frame = new System.Drawing.RectangleF(new PointF(0, 0),  size);
			friendsMediaView.View.Frame = new System.Drawing.RectangleF(new PointF(width, 0),  size);
			recentMediaView.View.Frame = new System.Drawing.RectangleF(new PointF(2 * width, 0),  size);
						
			var hsv = new HorizontalScrollView(likedMediaView)
			{
				Frame = new RectangleF(new PointF(0, topY), size),
				ContentSize = new SizeF(width * 3, size.Height - 20 - topY),				
			};
			hsv.Scrolled += HandleHsvScrolled;
			
			hsv.Add(likedMediaView.View);
			hsv.Add(friendsMediaView.View);
			hsv.Add(recentMediaView.View);
			
			this.View.AddSubview(hsv);
			
			UpdateLabels(LocalisationType.Global);
		}
		
		void HandleHsvScrolled (object sender, EventArgs e)
		{
			var hsv = (HorizontalScrollView)sender;
			double page = Math.Floor((hsv.ContentOffset.X - hsv.Frame.Width / 2) / hsv.Frame.Width) + 1;
			_CurrentPage = page;
		}

		void HandleLocalBtnTouchUpInside (object sender, EventArgs e)
		{
			UpdateLabels(LocalisationType.Local);
			SetEmptyImages();
			InitializeMap(new RequestInfo(RequestType.LocalBtn));
			
			Action act = ()=>
			{
				Util.Log("Wait");
				if (!loadWaitHandle.Wait(5000))
				{
					Util.Log("HandleLocManagerDelOnFailed");
					BeginInvokeOnMainThread(()=>
					{
						LoadImagesOnTimelines(null);
					});
					
					//Util.ShowAlertSheet("Localisation failed", View);
				}
			};
			AppDelegateIPhone.ShowRealLoading(View, "Loading local photos", null, act);			
		}
		
		void SetEmptyImages()
		{
			likedMediaView.SetEmptyImages();
			friendsMediaView.SetEmptyImages();
			recentMediaView.SetEmptyImages();
		}
		
		void HandleGlobalBtnTouchUpInside (object sender, EventArgs e)
		{	
			UpdateLabels(LocalisationType.Global);
			SetEmptyImages();
			InitializeMap(new RequestInfo(RequestType.GlobalBtn));
			
			Action act = ()=>
			{
				if (!loadWaitHandle.Wait(5000))
				{
				}
			};
			AppDelegateIPhone.ShowRealLoading(View, "Loading global photos", null, act);
		}		
			
		partial void GoToMap (MonoTouch.UIKit.UIButton sender)
		{
			GotoMapState(true);
		}
		
		private void UpdateLabels(LocalisationType locType)
		{
			LocType = locType;
			this.InvokeOnMainThread(()=>
            {
				globalBtn.Hidden = locType == LocalisationType.Global;
				localBtn.Hidden = locType == LocalisationType.Local;
				
				likedMediaView.Root[0].Caption = GetSubTitle(LocType, 1);
				friendsMediaView.Root[0].Caption = GetSubTitle(LocType, 2);
				recentMediaView.Root[0].Caption = GetSubTitle(LocType, 3);
			});
			
			if (locType == LocalisationType.Global)
				UIUtils.SetTitle("global", "posts around the world", titleTxt, null);
			
			if (locType == LocalisationType.Local)
				UIUtils.SetTitle("local", "posts around you", titleTxt, null);				
		}		

		public void GotoMapState(bool showMap)
		{
			if (LocType == LocalisationType.Local && Location == null)
				return;
			
			if (showMap)
			{				
				bool isNull = false;
				if (Map == null)
				{
					Map = new StartMapViewController (_MSP, this);
					isNull = true;
				}
				
				if (showMap == IsMap)
				{
					this.PresentModalViewController(Map, true);
				}
				else
					_MSP.PresentModalViewController(Map, true);
				
				if (!isNull)
				{
					Map.Reset();
					Map.SetPosition();
				}				
								
				IsMap = true;
			}
			else
				IsMap = false;
		}			
		
		private void UpdatedLocation(object sender, CLLocationUpdatedEventArgs args)
		{			
			//Util.Log("UpdatedLocation");
			var PosAccuracy = args.NewLocation.HorizontalAccuracy;
			//if (PosAccuracy >= 0)
			{
				locationManager.StopUpdatingLocation();
				HandleLocManagerDelOnUpdatedLocation(args.NewLocation);
			}
		}
		
		void HandleLocManagerDelOnUpdatedLocation (CLLocation newLocation)
		{
			//newLocation = null;
			
			if (newLocation == null)
			{
				Util.Log("HandleLocManagerDelOnFailed");
				
				loadWaitHandle.Set();
				LoadImagesOnTimelines(null);				
				
				Util.ShowAlertSheet("Localisation failed", AppDelegateIPhone.AIphone.MainWnd);
			}
			else
			{				
				//if (newLocation.Timestamp.SecondsSinceReferenceDate >= 8)
				if (newLocation.HorizontalAccuracy >= 3000f)
				{
					//Util.Log("Outdated localisation");
					Util.Log("Horizontal accuracy too small");
					if (request.Type == RequestType.Refresh)
					{	
						loadWaitHandle.Set();
						
						LoadImagesOnTimelines(null);
						/*
						BeginInvokeOnMainThread(()=>
						{
							AppDelegateIPhone.ShowMessage(View, "image stream loading failed", null);
						});
						*/
					}				
					return;
				}				
				
				Location = newLocation;
				Util.Log("Altitude: {0} Longitude: {1}", newLocation.Coordinate.Latitude, newLocation.Coordinate.Longitude);
				
				//Util.ShowAlertSheet("Done", View);
				ThreadPool.QueueUserWorkItem(o=> LoadTimelineImages());
			}
		}
		
		private void LoadImagesOnTimelines(AllImagesResponse allImgResp)
		{
			try
			{
				likedMediaView.ShowLoadedImages(allImgResp == null ? null : allImgResp.LikedImages, request);
				friendsMediaView.ShowLoadedImages(allImgResp == null ? null : allImgResp.FriendsImages, request);
				recentMediaView.ShowLoadedImages(allImgResp == null ? null : allImgResp.RecentImages, request);
			}
			catch (Exception ex)
			{
				Util.LogException("LoadImagesOnTimelines", ex);
			}
			finally
			{
				request = null;
			}
		}
		
		private ManualResetEventSlim loadWaitHandle = new ManualResetEventSlim(true);
		
		/// <summary>
		/// Executed in a background thread
		/// </summary>
		public void LoadTimelineImages()
		{
			AllImagesResponse allImgResp = GetAllImages();
			loadWaitHandle.Set();
			
			if (allImgResp == null)
			{
				BeginInvokeOnMainThread(()=>
				{
					AppDelegateIPhone.ShowMessage(View, "image stream loading failed", null);
				});
			}
			LoadImagesOnTimelines(allImgResp);
		}		

		#region IMapLocationRequest implementation
		
		public LocalisationType LocType {get;set;}
		
		public CLLocation Location {get;set;}
		
		public FilterType GetFilterType()
		{
			if (_CurrentPage == 0)
				return FilterType.Liked;
			if (_CurrentPage == 1)
				return FilterType.Friends;
			if (_CurrentPage == 2)
				return FilterType.Recent;
			
			return FilterType.Liked;
		}
					
		public List<ImageInfo> GetCurrentLoadedImages()
		{
			if (_CurrentPage == 0)
				return likedMediaView.GetLoadedImages();
			if (_CurrentPage == 1)
				return friendsMediaView.GetLoadedImages();
			if (_CurrentPage == 2)
				return recentMediaView.GetLoadedImages();
			
			return null;
		}
		
		private RequestInfo request;
		
		/// <summary>
		/// Executed in the UI thread
		/// </summary>
		public bool InitializeMap(RequestInfo request)
		{
			//
			//	If the guid is not empty, the request comes from
			//	the reloading process
			//
			
			this.request = request;
			
			loadWaitHandle.Reset();
			
			if (LocType == LocalisationType.Local)
			{
				locationManager.StartUpdatingLocation();
				Util.Log("Start updating location");
				
				/*
				NSTimer.CreateScheduledTimer (0, delegate {
					Util.RequestLocation(HandleLocManagerDelOnUpdatedLocation);
				});
				*/
			}
			else
				ThreadPool.QueueUserWorkItem(o => LoadTimelineImages());

			return true;
		}
		
		public AllImagesResponse GetAllImages()
		{
			try
			{
				User user = AppDelegateIPhone.AIphone.MainUser;
				GeoLoc geoLoc = LocType == LocalisationType.Global ? null : (Location == null ? null :
					new GeoLoc()
				{
					Latitude = Location.Coordinate.Latitude,
					Longitude = Location.Coordinate.Longitude,
				});				
				
				return AppDelegateIPhone.AIphone.ImgServ.GetFullImageList(FilterType.All, geoLoc, 0, 21, user.Id);
			}
			catch (Exception ex)
			{
				Util.LogException("GetAllImages", ex);
				return null;
			}
		}
		
		public IEnumerable<Image> GetDbImages(FilterType _FilterType, int start, int count)
		{			
			User user = AppDelegateIPhone.AIphone.MainUser;
			GeoLoc geoLoc = LocType == LocalisationType.Global ? null : (Location == null ? null :
				new GeoLoc()
			{
				Latitude = Location.Coordinate.Latitude,
				Longitude = Location.Coordinate.Longitude,
			});				
			
			return AppDelegateIPhone.AIphone.ImgServ.GetImageList(_FilterType, geoLoc, start, count, user.Id);	
		}		
		
		#endregion
	}
}
