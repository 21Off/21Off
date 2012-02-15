using System;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TweetStation;
using MSP.Client.DataContracts;
using System.Collections.Generic;

namespace MSP.Client
{
	public class Images2Element : Element, IElementSizing {
		static NSString key = new NSString ("Images2Element");		
		private List<ImageInfo> _images;
		private int cellIndex;	
		private ImagesCellInfo imgCellInfo;

		public Images2Element (List<ImageInfo> images, int rowIndex)
			: base (null)
		{
			_images = images;
			cellIndex = rowIndex;
			
			imgCellInfo = new ImagesCellInfo()
			{
				Images = _images,
				RowIndex = cellIndex,
			};
		}
		
		// Gets a cell on demand, reusing cells
		public override UITableViewCell GetCell (UITableView tv)
		{
			
			var cell = tv.DequeueReusableCell (key) as ImagesCell;
			if (cell == null)
				cell = new ImagesCell (UITableViewCellStyle.Default, key, imgCellInfo);
			else
				cell.UpdateCell (imgCellInfo);
			
			return cell;
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			//var profile = new DetailTweetViewController (Tweet);
			//dvc.ActivateController (profile);
		}
		
		#region IElementSizing implementation
		public float GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			return 70;
		}
		#endregion
	}
}
