using System;
using System.Collections.Generic;
using MonoTouch.CoreLocation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public class HeaderInfos
	{
		public string Title {get;set;}
		public string SubTitle {get;set;}
	}
	
	public partial class PhotoMapViewController : UIViewController, IMapLocationRequest
	{		
		private UIViewController _MSP;
		private Image image;
		private List<Image> images;
		private HeaderInfos headerInfos;
		
		public PhotoMapViewController (IntPtr handle) : base(handle)
		{
			
		}

		[Export("initWithCoder:")]
		public PhotoMapViewController (NSCoder coder) : base(coder)
		{
			
		}
				
		public PhotoMapViewController (UIViewController msp, List<Image> images, HeaderInfos headerInfos) 
			: base("PhotoMapViewController", null)
		{
			_MSP = msp;
			this.images = images;
			this.headerInfos = headerInfos;
		}
		
		public PhotoMapViewController (UIViewController msp, Image image) : base("PhotoMapViewController", null)
		{
			_MSP = msp;
			this.image = image;
		}
		
		public override void ViewDidLoad ()
		{
			LocType = LocalisationType.Global;
			
			base.ViewDidLoad ();
			
			/*			
			var mapView = new MKMapView();
			var dv = new TimelineViewController(FilterType.Liked, false, null, this);
			
			// When the view goes out of screen, we fetch the data.
			dv.ViewDissapearing += delegate {

			};
			
			int mapHeight = 150;
			mapView.Frame = new RectangleF(0, 0, 320, mapHeight);
			dv.View.Frame = new RectangleF(0, mapHeight, 320, UIScreen.MainScreen.Bounds.Height - mapHeight);
			
			this.View.AddSubview(mapView);
			this.View.AddSubview(dv.View);
			*/			
			
			StartMapViewController startMap = null;
			if (image == null)
				startMap = new StartMapViewController(_MSP, this, images);
			else
				startMap = new StartMapViewController(_MSP, this, image);
			
			this.View.Add(startMap.View);
			
			startMap.HeaderInfos = headerInfos;
			startMap.UpdateTitle();
		}		
	

		#region IMapLocationRequest implementation
		
		public bool InitializeMap (RequestInfo request)
		{
			return true;
		}

		public IEnumerable<Image> GetDbImages (FilterType filterType, int start, int count)
		{
			return new Image[0];
		}

		public FilterType GetFilterType ()
		{
			return FilterType.Friends;
		}

		public LocalisationType LocType {
			get;
			set;
		}

		public MonoTouch.CoreLocation.CLLocation Location {
			get;
			set;
		}
		
		public List<ImageInfo> GetCurrentLoadedImages ()
		{
			return null;
		}
		
		
		#endregion
		
	}
}

