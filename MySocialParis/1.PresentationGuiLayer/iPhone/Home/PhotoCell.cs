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
		
		private List<ImageInfo> oldFilenames = new List<ImageInfo>();		
		private PhotoCellView photoRowView;
		private int _RowIndex;

		// Create the UIViews that we will use here, layout happens in LayoutSubviews
		public PhotoCell (UITableViewCellStyle style, NSString ident, List<ImageInfo> filenames, 
		                  int rowIndex, Action<BuzzPhoto> onPhotoClicked) : base(style, ident)
		{
			SelectionStyle = UITableViewCellSelectionStyle.None;
			_RowIndex = rowIndex;			
			oldFilenames = filenames;
				
			photoRowView = new PhotoCellView (filenames, rowIndex, onPhotoClicked);
			ContentView.Add (photoRowView);
		}

		public void UpdateCell (List<ImageInfo> filenames, int rowIndex)
		{
			if (filenames == null)
				return;
			
			if (photoRowView == null)
				return;
			
			bool changed = false;
						
			if (oldFilenames.Count == 0)
			{
				changed = true;
			}
			else
			{
				for (int i = 0; i < 3; i++)
				{
					ImageInfo oldf = oldFilenames[i];
					ImageInfo newf = filenames[i];
					
					if (oldf.Img == null && newf.Img != null)
						changed = true;
					
					if (oldf.Img != null && newf.Img == null)
						changed = true;
					
					if (oldf.Img != null && newf.Img != null)
					{
						if (oldf.Img.Id != newf.Img.Id)
							changed = true;
					}
				}
			}
			
			if (_RowIndex != rowIndex || changed)
			{
				_RowIndex = rowIndex;
				oldFilenames = filenames;
				
				photoRowView.Update (filenames, rowIndex);
				SetNeedsDisplay ();
			}
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

