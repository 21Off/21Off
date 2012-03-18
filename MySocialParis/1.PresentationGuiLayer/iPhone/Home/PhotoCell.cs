using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public class PhotoCell : UITableViewCell
	{
		// Should never happen
		public PhotoCell (IntPtr handle) : base(handle)
		{
			//
		}
		
		private PhotoCellView photoRowView;

		// Create the UIViews that we will use here, layout happens in LayoutSubviews
		public PhotoCell (UITableViewCellStyle style, NSString ident, List<ImageInfo> filenames, 
		                  int rowIndex, Action<BuzzPhoto> onPhotoClicked) : base(style, ident)
		{
			SelectionStyle = UITableViewCellSelectionStyle.None;
				
			photoRowView = new PhotoCellView (filenames, rowIndex, onPhotoClicked);
			ContentView.Add (photoRowView);
		}

		public void UpdateCell (List<ImageInfo> filenames, int rowIndex)
		{
			if (filenames == null)
				return;
			
			if (photoRowView == null)
				return;
			
			photoRowView.Update (filenames, rowIndex);
		}

		private bool updated = false;

		public override void Draw (RectangleF rect)
		{
			base.Draw (rect);
			
			if (photoRowView == null || ContentView == null)
				return;
			
			if (!updated) {
				// object is not set to instance
				photoRowView.Frame = ContentView.Bounds;
				updated = true;
			}
		}
	}
}

