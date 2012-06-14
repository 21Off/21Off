using System;
using System.Drawing;

using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using MonoTouch.Dialog;
using System.Threading;
using System.Collections.Generic;

namespace MSP.Client
{
	public partial class AlbumDetailsViewController : UIViewController, IMapLocationRequest
	{
		public UINavigationController Nav { get; set; }
		private AppDelegateIPhone _AppDel;
		private Albums album;
		
		public AlbumDetailsViewController () : base ("AlbumDetailsViewController", null)
		{
		}
		
		public AlbumDetailsViewController(UINavigationController nav, AppDelegateIPhone appDel, Albums album) :this()
		{
			Nav = nav;
			_AppDel = appDel;
			this.album = album;
		}
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			backBtn.TouchDown += HandleBackBtnTouchDown;
			
			// Perform any additional setup after loading the view, typically from a nib.
			Initialize();
			
			this.request = new RequestInfo(RequestType.FirstLoad);			
			InitializeMap(request);			
		}

		void HandleBackBtnTouchDown (object sender, EventArgs e)
		{
			Nav.PopViewControllerAnimated(true);
		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}
		
		private TimelineViewController likedMediaView;
		private RequestInfo request;		
		
		private void Initialize()
		{
			LocType = LocalisationType.Global;
			
			float width = UIScreen.MainScreen.Bounds.Width;
			float topY = 40;
			var size = new SizeF(width, 480 - topY);
			
			var likedRootElement = new RootElement ("album") { UnevenRows = true };
			var likedSection = new Section(album.Title);
			likedRootElement.Add(likedSection);			
			
			likedMediaView = new TimelineViewController(FilterType.Liked, true, Nav, this) 
			{ 
				Root = likedRootElement, 
				ShowLoadMorePhotos = false,				
			};
			
			likedMediaView.View.Frame = new System.Drawing.RectangleF(new PointF(0, topY),  size);
			this.Add(likedMediaView.View);
		}
		
		private void LoadTimelineImages()
		{
			try
			{
				ImagesResponse response = AppDelegateIPhone.AIphone.ImgServ.GetAlbumImages(0, 21, 0, album.Id);
				likedMediaView.ShowLoadedImages(response == null ? null : response.Images, request);
			}
			catch (Exception ex)
			{
				Util.LogException("LoadTimelineImages",ex);			
			}
		}

		#region IMapLocationRequest implementation
		public bool InitializeMap (RequestInfo request)
		{
			ThreadPool.QueueUserWorkItem(o => LoadTimelineImages());
			
			return true;
		}

		public IEnumerable<MSP.Client.DataContracts.Image> GetDbImages (MSP.Client.DataContracts.FilterType filterType, int start, int count)
		{
			ImagesResponse response = AppDelegateIPhone.AIphone.ImgServ.GetAlbumImages(start, count, 0, album.Id);
			return response == null ? null : response.Images;
		}

		public FilterType GetFilterType ()
		{
			return FilterType.Events; 
		}

		public System.Collections.Generic.List<ImageInfo> GetCurrentLoadedImages ()
		{
			return likedMediaView.GetLoadedImages();
		}

		public MSP.Client.DataContracts.LocalisationType LocType {
			get;set;
		}

		public MonoTouch.CoreLocation.CLLocation Location {
			get;set;
		}
		#endregion
	}
}

