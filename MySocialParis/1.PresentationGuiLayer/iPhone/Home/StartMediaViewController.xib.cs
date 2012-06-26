using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using MonoTouch.CoreLocation;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public partial class StartMediaViewController : UIViewController, IMapLocationRequest
	{		
		#region Private members

		private ManualResetEventSlim loadWaitHandle = new ManualResetEventSlim(true);
		private MSPNavigationController _MSP;
		private CLLocationManager locationManager;		
		private int _CurrentPage;
		
		#endregion

		#region Properties
		
		public StartMapViewController Map {get;set;}
		public bool IsMap {get;set;}
		
		#endregion

		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code
		public StartMediaViewController (IntPtr handle) : base(handle)
		{
			
		}

		[Export("initWithCoder:")]
		public StartMediaViewController (NSCoder coder) : base(coder)
		{

		}

		public StartMediaViewController () : base("StartMediaViewController", null)
		{
			IsMap = false;
		}

		public StartMediaViewController (MSPNavigationController msp) 
			: this()
		{
			_MSP = msp;
			LocType = LocalisationType.Global;
		}
						
		#endregion		
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();			
			
			Initialize();
			SetEmptyImages();
			InitializeMap(new RequestInfo(RequestType.FirstLoad));
		}
		
		#region Events
		
		private void HandleHsvScrolled (object sender, EventArgs e)
		{
			var hsv = (HorizontalScrollView)sender;
			double page = Math.Floor((hsv.ContentOffset.X - hsv.Frame.Width / 2) / hsv.Frame.Width) + 1;
			_CurrentPage = (int)Math.Round(page);
		}

		private void HandleLocalBtnTouchUpInside (object sender, EventArgs e)
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
		
		private void HandleGlobalBtnTouchUpInside (object sender, EventArgs e)
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
		
		private void UpdatedLocation(object sender, CLLocationUpdatedEventArgs args)
		{			
			//var PosAccuracy = args.NewLocation.HorizontalAccuracy;
			//if (PosAccuracy >= 0)
			{
				locationManager.StopUpdatingLocation();
				HandleLocManagerDelOnUpdatedLocation(args.NewLocation);
			}
		}
		
		private void HandleLocManagerDelOnUpdatedLocation (CLLocation newLocation)
		{
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
					}				
					return;
				}								
				Location = newLocation;
				Util.Log("Altitude: {0} Longitude: {1}", newLocation.Coordinate.Latitude, newLocation.Coordinate.Longitude);
				
				ThreadPool.QueueUserWorkItem(o=> LoadTimelineImages());
			}
		}
		
		#endregion
		
		#region Private members
		
		partial void GoToMap (MonoTouch.UIKit.UIButton sender)
		{
			GotoMapState(true);
		}		
		
		private void LoadImagesOnTimelines(AllImagesResponse allImgResp)
		{
			try
			{
				for (int i = 0; i < mediaViews.Count; i++)
				{
					var mediaView = mediaViews[i];
					List<Image> images = null;
					if (allImgResp != null)
					{
						if (i == 0)
							images = allImgResp.LikedImages;
						if (i == 1)
							images = allImgResp.FriendsImages;
						if (i == 2)
							images = allImgResp.RecentImages;
						if (i == 3)
							images = allImgResp.EventsImages;
					}

					mediaView.ShowLoadedImages(images, request);
				}
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
		
		public static string GetSubTitle(LocalisationType locType, int screen)
		{
			if (locType == LocalisationType.Global)
			{				
				if (screen == 1) return "       trendy posts worldwide       >";
				if (screen == 2) return "<       friend's posts worldwide     >";
				if (screen == 3) return "<        recent posts worldwide      >";
				if (screen == 4) return "<       recent events worldwide";
			}
			if (locType == LocalisationType.Local)
			{
				if (screen == 1) return "      trendy posts around you        >";
				if (screen == 2) return "<     friend's posts around you    >";
				if (screen == 3) return "<       recent posts around you     >";
				if (screen == 4) return "<      recent events around you";
			}
			return string.Empty;
		}
		
		private void Initialize()
		{			
			locationManager = new CLLocationManager();
			locationManager.DesiredAccuracy = -1;
			locationManager.DistanceFilter = 50;
			locationManager.HeadingFilter = 1;
			locationManager.UpdatedLocation += UpdatedLocation;		
			
			globalBtn.SetImage(Graphics.GetImgResource("global"), UIControlState.Normal);
			localBtn.SetImage(Graphics.GetImgResource("local"), UIControlState.Normal);
			mapBtn.SetImage(Graphics.GetImgResource("map"), UIControlState.Normal);
			
			globalBtn.TouchUpInside += HandleGlobalBtnTouchUpInside;
			localBtn.TouchUpInside += HandleLocalBtnTouchUpInside;

			float width = UIScreen.MainScreen.Bounds.Width;
			float topY = 42;
			SizeF size = new SizeF(width, 480 - topY);
						
			var hsv = new HorizontalScrollView()
			{
				Frame = new RectangleF(new PointF(0, topY), size),
				ContentSize = new SizeF(width * 4, size.Height - 20 - topY),				
			};
			hsv.Scrolled += HandleHsvScrolled;

			CreateMediaView(1, "liked", hsv);
			CreateMediaView(2, "recent", hsv);
			CreateMediaView(3, "friends", hsv);
			CreateMediaView(4, "events", hsv);

			this.View.AddSubview(hsv);
			
			UpdateLabels(LocalisationType.Global);
		}

		private void CreateMediaView(int pos, string title, HorizontalScrollView hsv)
		{
			float width = UIScreen.MainScreen.Bounds.Width;
			float topY = 42;
			SizeF size = new SizeF(width, 480 - topY);

			var rootElement = new RootElement (title) { UnevenRows = true };
			var section = new Section(GetSubTitle(LocType, pos));
			rootElement.Add(section);

			FilterType filter = GetFilter(pos);

			var mediaView = new TimelineViewController(filter, true, _MSP, this) { Root = rootElement, };
		 	mediaView.View.Frame = new System.Drawing.RectangleF(new PointF((pos - 1)* width, 0),  size);
			hsv.Add(mediaView.View);
			mediaViews.Add(mediaView);
		}

		private List<TimelineViewController> mediaViews = new List<TimelineViewController>();
		
		private void UpdateLabels(LocalisationType locType)
		{
			LocType = locType;
			this.InvokeOnMainThread(()=>
            {
				globalBtn.Hidden = locType == LocalisationType.Global;
				localBtn.Hidden = locType == LocalisationType.Local;

				for (int i = 0; i < mediaViews.Count; i++)
				{
					mediaViews[i].Root[0].Caption = GetSubTitle(LocType, i + 1);
				}

				/*
				likedMediaView.Root[0].Caption = GetSubTitle(LocType, 1);
				friendsMediaView.Root[0].Caption = GetSubTitle(LocType, 2);
				recentMediaView.Root[0].Caption = GetSubTitle(LocType, 3);
				eventsMediaView.Root[0].Caption = GetSubTitle(LocType, 4);
				*/
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
					this.PresentModalViewController(Map, true);
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
		
		private void SetEmptyImages()
		{
			foreach (var mediaView in mediaViews)
			{
				mediaView.SetEmptyImages();
			}
		}		
		
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
		
		#endregion

		#region IMapLocationRequest implementation
		
		public LocalisationType LocType {get;set;}
		
		public CLLocation Location {get;set;}
		
		public FilterType GetFilterType()
		{
			return GetFilter(_CurrentPage + 1);
		}

		private FilterType GetFilter(int pos)
		{
			FilterType filter = FilterType.All;
			if (pos == 1)
				filter = FilterType.Liked;
			if (pos == 2)
				filter = FilterType.Friends;
			if (pos == 3)
				filter = FilterType.Recent;
			if (pos == 4)
				filter = FilterType.Events;

			return filter;
		}
					
		public List<ImageInfo> GetCurrentLoadedImages()
		{
			return (mediaViews[_CurrentPage]).GetLoadedImages();
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
			}
			else
				ThreadPool.QueueUserWorkItem(o => LoadTimelineImages());

			return true;
		}
		
		public AllImagesResponse GetAllImages()
		{
			try
			{
				var userId = AppDelegateIPhone.AIphone.GetMainUserId();
				GeoLoc geoLoc = LocType == LocalisationType.Global ? null : (Location == null ? null :
					new GeoLoc()
				{
					Latitude = Location.Coordinate.Latitude,
					Longitude = Location.Coordinate.Longitude,
				});				
				
				return AppDelegateIPhone.AIphone.ImgServ.GetFullImageList(FilterType.All, geoLoc, 0, 21, userId);
			}
			catch (Exception ex)
			{
				Util.LogException("GetAllImages", ex);
				return null;
			}
		}
		
		public IEnumerable<Image> GetDbImages(FilterType _FilterType, int start, int count)
		{			
			var userId = AppDelegateIPhone.AIphone.GetMainUserId();
			GeoLoc geoLoc = LocType == LocalisationType.Global ? null : (Location == null ? null :
				new GeoLoc()
			{
				Latitude = Location.Coordinate.Latitude,
				Longitude = Location.Coordinate.Longitude,
			});				
			
			return AppDelegateIPhone.AIphone.ImgServ.GetImageList(_FilterType, geoLoc, start, count, userId);	
		}		
		
		#endregion
	}
}

