
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public partial class PhotoAlbumViewController : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public PhotoAlbumViewController (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public PhotoAlbumViewController (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public PhotoAlbumViewController () : base("PhotoAlbumViewController", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
		}
		
		#endregion
	}
}

