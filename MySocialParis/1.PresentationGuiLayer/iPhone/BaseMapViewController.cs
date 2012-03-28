using System;
using System.Threading;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public abstract class BaseMapViewController : UIViewController, IMapLocationRequest
	{		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		public abstract void Initialize ();

		public abstract WaitCallback LoadTimelineImages ();
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
			Initialize();
			
			this.request = new RequestInfo(RequestType.FirstLoad);
			
			ThreadPool.QueueUserWorkItem(o => LoadTimelineImages());			
		}

		public void ReleaseDesignerOutlets ()
		{
			
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
		
		#region IMapLocationRequest implementation
		public bool InitializeMap (RequestInfo request)
		{
			throw new NotImplementedException ();
		}

		public System.Collections.Generic.IEnumerable<Image> GetDbImages (FilterType filterType, int start, int count)
		{
			throw new NotImplementedException ();
		}

		public FilterType GetFilterType ()
		{
			throw new NotImplementedException ();
		}

		public System.Collections.Generic.List<ImageInfo> GetCurrentLoadedImages ()
		{
			throw new NotImplementedException ();
		}

		public LocalisationType LocType { get;set; }

		public MonoTouch.CoreLocation.CLLocation Location { get;set; }
		#endregion
	}
}

