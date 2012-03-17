using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public partial class StartMapViewController : UIViewController
	{
		#region Properties

		UIScrollView scrollView;
		private int h = 83;
		private int w = 79;
		private int img_cnt = 0;
		private bool loaded = false;
		private Image image;
		private List<Image> images;
		
		private TilingView selectedTV;				
		private List<ImageInfo> _list;
		
		private List<UIView> photosViews = new List<UIView>();		
		private List<TilingView> tillingViews = new List<TilingView>();
		
		private UIView indicator;		
		private UIViewController _MSP;
		private IMapLocationRequest _MaplocationRequest;
		public HeaderInfos HeaderInfos {get;set;}

		#endregion
		
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public StartMapViewController (IntPtr handle) : base(handle)
		{
			
		}

		[Export("initWithCoder:")]
		public StartMapViewController (NSCoder coder) : base(coder)
		{
			
		}
		
		public StartMapViewController (UIViewController msp, IMapLocationRequest maplocationRequest, List<Image> images)
			: this(msp, maplocationRequest)
		{
			this.images = images;						
		}		
		
		public StartMapViewController (UIViewController msp, IMapLocationRequest maplocationRequest, Image image) 
			: this(msp, maplocationRequest)
		{
			this.image = image;						
		}		
		
		public StartMapViewController (UIViewController msp, IMapLocationRequest maplocationRequest) 
			: base("StartMapViewController", null)
		{
			_MaplocationRequest = maplocationRequest;
			_MSP = msp;
			_list = new List<ImageInfo> ();									
		}

		#endregion

		#region Overrides		

		public override void ViewWillAppear (bool animated)
		{		
			base.ViewWillAppear (animated);
			
			if (indicator == null) {
				indicator = new TriangleView (UIColor.FromRGB (0.0f, 0.0f, 0.0f), UIColor.Black);
				View.AddSubview (indicator);
				UpdatePosition (-100, false);
			}
			
			UpdateTitle();
		}	                        
		
		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			
			Reset();
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();			
			
			backBtn.SetImage(Graphics.GetImgResource("back"), UIControlState.Normal);
			backBtn.TouchUpInside += HandleBackBtnTouchUpInside;
				
			UpdateTitle();
						
			scrollView = new UIScrollView (new RectangleF (0, this.View.Bounds.Height - h, 320, h));												
			this.View.AddSubview (scrollView);
			
			mapView.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight;
			
			foreach (var subView in mapView.Subviews)
			{
				if (subView is UIImageView)
				{
					subView.Frame = new RectangleF(5, 5, subView.Frame.Width, subView.Frame.Height);
					//((UIImageView)subView).Image.SaveToPhotosAlbum(null);
				}
			}
			
			var view = new UIView(new RectangleF(0, 40, 320, 1));
			view.Layer.BackgroundColor = UIColor.LightGray.CGColor;
			this.View.AddSubview(view);
			
			scrollView.PagingEnabled = false;
			scrollView.Bounces = true;
			scrollView.DelaysContentTouches = true;
			scrollView.ShowsHorizontalScrollIndicator = true;
			
			scrollView.BackgroundColor = new UIColor (1, 1, 1, 0.6f);
			scrollView.ScrollRectToVisible (new RectangleF (w, 0, w, h), true);
			scrollView.Scrolled += HandleScrollViewScrolled;

			Initialize ();
			
			if (indicator == null) {
				indicator = new TriangleView (UIColor.Black, UIColor.Black);
				View.AddSubview (indicator);
				UpdatePosition (-100, false);
			}			
		}
		
		#endregion

		#region Events		

		void HandleBackBtnTouchUpInside (object sender, EventArgs e)
		{
			if (image == null && images == null)
			{
				DismissModalViewControllerAnimated (true);
				var nav = (UINavigationController)_MSP;
				
				if (nav.ViewControllers.Count() > 0 && nav.ViewControllers[0] is StartMediaViewController)
				{
					var startMediaView = (StartMediaViewController)nav.ViewControllers[0];				
					startMediaView.GotoMapState(false);
				}
			}
			else
			{
				(_MaplocationRequest as PhotoMapViewController).DismissModalViewControllerAnimated(true);
			}
		}
		
		public void OnPhotoClicked(Image photo)
		{
			if (photo == null || image != null)
			{
				return;
			}			
			
			if (_MSP != null)
				_MSP.DismissModalViewControllerAnimated(true);
			
			Action act = () =>
			{
				int askerId = AppDelegateIPhone.AIphone.MainUser.Id;
				FullUserResponse fullUser = AppDelegateIPhone.AIphone.UsersServ.GetFullUserById(photo.UserId, askerId);
				InvokeOnMainThread(()=>				                   
				{
					var nav = (UINavigationController)AppDelegateIPhone.tabBarController.SelectedViewController;
					var a = new PhotoDetailsViewController(nav, fullUser, photo, false);
					nav.PushViewController(a, true);
				});
			};
			
			AppDelegateIPhone.ShowRealLoading(View, "Loading photo details", null, act);
		}

		void HandleScrollViewScrolled (object sender, EventArgs e)
		{
			if (selectedTV != null) {
				float left = selectedTV.ParentFrame.Left - scrollView.ContentOffset.X;
				UpdatePosition (left, true);
			}
		}

		void HandleTillingViewTouchUpInside (object sender, EventArgs e)
		{
			try
			{
				var tv = (TilingView)sender;
				selectedTV = tv;
				float left = tv.ParentFrame.Left - scrollView.ContentOffset.X;
				UpdatePosition (left, true);
				
				if (tv.Annotation != null)
				{
					GotoLocation (tv.Annotation.Coordinate);
					mapView.SelectAnnotation (tv.Annotation, true);
				}
				else
				{
					//TAKE PICTURE
					//this.DismissModalViewControllerAnimated(true);				
				}
			}
			catch (Exception ex)
			{
				Util.LogException("HandleTillingViewTouchUpInside", ex);
			}
		}

		private void AnnotationClicked (MyAnnotation a)
		{
			try
			{
				scrollView.ScrollRectToVisible (new RectangleF (a.TV.ParentFrame.Left, 0, w, h), false);
				float left = a.TV.ParentFrame.Left - scrollView.ContentOffset.X;
				UpdatePosition (left, true);
				
				//a.TV.Layer.BackgroundColor = UIColor.Black.CGColor;
				//a.TV.SetNeedsDisplay();				
			}
			catch (Exception ex)
			{
				Util.LogException("AnnotationClicked", ex);
			}
		}
		
		void UpdatePosition (float x, bool animate)
		{
			try
			{
				if (animate) {
					UIView.BeginAnimations (null);
					UIView.SetAnimationCurve (UIViewAnimationCurve.EaseInOut);
				}
				
				indicator.Frame = new RectangleF (x + ((w - 10) / 2), View.Bounds.Height - h, 10, 6);
								
				if (animate)
					UIView.CommitAnimations ();
			}
			catch (Exception ex)
			{
				Util.LogException("UpdatePosition", ex);
			}
		}			

		#endregion
		
		#region Public methods	
		
		public void UpdateTitle()
		{
			try
			{
				if (this.image == null)
				{
					if (this.images != null)
					{
						if (HeaderInfos != null)
							UIUtils.SetTitle(HeaderInfos.Title, HeaderInfos.SubTitle, titleTxt, worldTxt);	
					}
					else
					{
						bool isGlobal = _MaplocationRequest.LocType == LocalisationType.Global;
						FilterType filter = _MaplocationRequest.GetFilterType();
						int page = filter == FilterType.Friends ? 2 : (filter == FilterType.Liked ? 1 : 3);
						string subTitle = StartMediaViewController.GetSubTitle(_MaplocationRequest.LocType, page);
						subTitle = subTitle.Replace(">", "").Replace("<", "").Trim();
						
						UIUtils.SetTitle(isGlobal ? "global" : "local", subTitle, titleTxt, worldTxt);
					}
				}
				else
				{						
					UIUtils.SetTitle("location", image.Name ?? "", titleTxt, worldTxt);				
				}
				
				View.SetNeedsDisplay();
			}
			catch (Exception ex)
			{
				Util.LogException("UpdateTitle", ex);
			}
		}				
		
		public void Reset()
		{
			try
			{
				foreach (UIView photoView in photosViews)
				{
					scrollView.WillRemoveSubview(photoView);
				}
				foreach (TilingView tilingView in tillingViews)
				{
					tilingView.TouchUpInside -= HandleTillingViewTouchUpInside;
				}
				tillingViews.Clear();
				photosViews.Clear();
				
				MKAnnotation[] annotations = mapView.Annotations.Select(el => el as MKAnnotation).ToArray();
				mapView.RemoveAnnotations(annotations);
			}
			catch (Exception ex)
			{
				Util.LogException("Reset", ex);
			}
		}
		
		#endregion
		
		#region Private methods
		
		private void AddImageWithName (ImageInfo imageInfo)
		{
			try
			{
				int position = imageInfo.Index;
				var view = new UIView ()
				{
					ContentMode = UIViewContentMode.Center,
					Frame = new RectangleF ((position - 1) * w, (h - 75) / 2, w, h),
				};
							
				Image img = imageInfo.Img;
				
				var tilingView = new TilingView (img, new SizeF (75, 75)) { ParentFrame = view.Frame, };
				tilingView.ParentHolder = view;
				tilingView.TouchUpInside += HandleTillingViewTouchUpInside;							
				
				if (img != null)
				{
					string title = img.Name ?? "No title";
					var location = new CLLocationCoordinate2D (img.Latitude, img.Longitude);
					
					var a = new MyAnnotation (location, title, "Evenement") 
					{ 
						TV = tilingView,
						AssocImage = img,				
					};
					a.OnAnnotationClicked += AnnotationClicked;
					
					mapView.AddAnnotationObject (a);
					
					tilingView.Annotation = a;					
				}
								
				view.AddSubview (tilingView);
				scrollView.AddSubview (view);
				
				tillingViews.Add(tilingView);
				photosViews.Add(view);
			}
			catch(Exception ex)
			{
				Util.LogException("AddImageWithName", ex);
			}
		}
		
		public void Initialize ()
		{
			//mapView.WillStartLoadingMap += (s, e) => { UIApplication.SharedApplication.NetworkActivityIndicatorVisible = true; };
			//mapView.MapLoaded += (s, e) =>  { UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false; };
			//mapView.LoadingMapFailed += (s, e) => { UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false; };
			
			//if (image == null && _MaplocationRequest.LocType == LocalisationType.Local)
			//	mapView.ShowsUserLocation = true;
			mapView.ShowsUserLocation = true;
			
			//Set up the text attributes for the user location annotation callout
			//mapView.UserLocation.Title = "You are here";
			//mapView.UserLocation.Subtitle = "Paris";
			
			mapView.MapType = MKMapType.Standard;
			var mapViewDel = new MapViewDelegate ();
			mapViewDel.OnPhotoClicked += OnPhotoClicked;
			mapView.Delegate = mapViewDel;
			
			SetPosition();
		}
		
		public void SetPosition()
		{
			loaded = false;
			
			Action action = ()=>
			{
				if (!loaded) {
					loaded = true;
					if (image != null)
						RepositionMapAndInit(new CLLocation(image.Latitude, image.Longitude));
					else
					{
						if (images != null)
						{
							RepositionMapAndInit(null);
						}
						else
							RepositionMapAndInit(_MaplocationRequest.Location);
					}
				}				
			};
			action.BeginInvoke(null, null);
		}	
		
		private void InitList()
		{
			try
			{
				if (_list == null)
					return;
				
				_list.Clear();
				
				if (image == null && images == null)
				{				
					//var resa = _MaplocationRequest.GetDbImages(_MaplocationRequest.GetFilterType(), 0, 21).ToArray();
					
					var resa = new List<Image>();
					List<ImageInfo> imageInfos = _MaplocationRequest.GetCurrentLoadedImages();
					foreach (ImageInfo imgInfo in imageInfos)
					{
						if (imgInfo.Img != null)
							resa.Add(imgInfo.Img);
					}					
					
					int count = resa.Count();
					img_cnt = count;
					
					//	"Add photo" Icon
					/*
					if (count < 21)
						img_cnt = count + 1;
					else
						img_cnt = count;
					*/	
					
					for (int i = 1; i <= img_cnt; i++)
					{
						var imgInfo = new ImageInfo() { Index = i };
						if (i <= count)
						{
							imgInfo.Img = resa[i - 1];
						}
						_list.Add(imgInfo);
					}
				}
				else
				{
					if (images == null)
					{
						img_cnt = 6;
						_list.Add(new ImageInfo(){Index = 1, Img = this.image });
					}
					else
					{
						img_cnt = Math.Max(images.Count, 6);
						for (int i = 0; i < images.Count; i++)
						{
							var imgInfo = new ImageInfo() { Index = i + 1, Img = images[i] };
							_list.Add(imgInfo);							
						}
					}
				}
			}
			catch (Exception ex)
			{
				Util.LogException("InitList", ex);
			}
		}			
		
		private void RepositionMapAndInit(CLLocation newLocation)
		{
			try
			{
				InitList();
				
				double maxLat = - 1000, minLat = 1000, maxLong = -1000, minLong = 1000;
				foreach (ImageInfo imgInfo in _list)
				{
					if (imgInfo.Img != null)
					{
						Image img = imgInfo.Img;
						if (img.Latitude > maxLat)
							maxLat = img.Latitude;
						if (img.Latitude < minLat)
							minLat = img.Latitude;
						
						if (img.Longitude > maxLong)
							maxLong = img.Longitude;
						if (img.Longitude < minLong)
							minLong = img.Longitude;
					}
				}
				
				double d = 0.02;
				double zoomLevel = 30;
				if (minLat == 1000)
				{
					maxLat = d;
					minLat = 0.0;
					maxLong = d;
					minLong = 0.0;
				}
				zoomLevel = Math.Max(maxLat - minLat, maxLong - minLong);				
				
				minLat -= d * zoomLevel;
				maxLat += d * zoomLevel;
				minLong -= d * zoomLevel;
				maxLong += d * zoomLevel;
				
				double latDelta = Math.Max(maxLat - minLat, d);
				double longDelta = Math.Max(maxLong - minLong, d);				
				
				MKCoordinateSpan spanLocal = _MaplocationRequest.LocType == LocalisationType.Global ?					
					new MKCoordinateSpan (latDelta, longDelta) : new MKCoordinateSpan (d, d);
								
				if (newLocation != null)
				{
					var region = new MKCoordinateRegion (newLocation.Coordinate, spanLocal);
					
					this.InvokeOnMainThread(()=>
					{									
						mapView.SetRegion (region, false);
						mapView.SetCenterCoordinate (newLocation.Coordinate, false);				
					});
				}
				else
				{
					var location = new CLLocationCoordinate2D(minLat + latDelta / 2, minLong + longDelta /2);
					var region = new MKCoordinateRegion (location, spanLocal);
					
					this.InvokeOnMainThread(()=>
					{						
						mapView.SetRegion (region, false);
						mapView.SetCenterCoordinate (location, false);
					});
				}
	
				this.InvokeOnMainThread(()=>
				{
					scrollView.ContentSize = new System.Drawing.SizeF (img_cnt * w, h);				
						
					foreach (ImageInfo imageInfo in _list)
					{
						AddImageWithName (imageInfo);
					}
					scrollView.SetNeedsLayout();
				});
			}
			catch (Exception ex)
			{
				Util.LogException("RepositionMapAndInit", ex);
			}			      
		}

		private void GotoLocation (CLLocationCoordinate2D newLocation)
		{
			MKCoordinateSpan span = mapView.Region.Span;
			//var region = new MKCoordinateRegion (newLocation, span);
			
			mapView.SetCenterCoordinate(newLocation, false);
			//mapView.SetRegion (region, false);
			//mapView.SetCenterCoordinate(newLocation,true); 
		}
		
		#endregion
	}
}

