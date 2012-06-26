// WARNING
//
// This file has been generated automatically by MonoDevelop to store outlets and
// actions made in the Xcode designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using MonoTouch.Foundation;

namespace MSP.Client
{
	[Register ("AlbumDetailsViewController")]
	partial class AlbumDetailsViewController
	{
		[Outlet]
		MonoTouch.UIKit.UIButton okBtn { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton backBtn { get; set; }

		[Outlet]
		MonoTouch.UIKit.UIButton toolBtn { get; set; }
		
		void ReleaseDesignerOutlets ()
		{
			if (okBtn != null) {
				okBtn.Dispose ();
				okBtn = null;
			}

			if (backBtn != null) {
				backBtn.Dispose ();
				backBtn = null;
			}

			if (toolBtn != null) {
				toolBtn.Dispose ();
				toolBtn = null;
			}
		}
	}
}
