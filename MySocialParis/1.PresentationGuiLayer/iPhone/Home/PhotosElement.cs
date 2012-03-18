using System.Collections.Generic;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public class PhotosElement : Element
	{
		static NSString ikey = new NSString ("PhotosElement");

		private int _RowIndex = -1;
		private TimelineViewController _Timeline;

		public PhotosElement (TimelineViewController timeline, int rowIndex) : base("")
		{
			_Timeline = timeline;
			_RowIndex = rowIndex;
		}

		public override UITableViewCell GetCell (UITableView tableView)
		{									
			List<ImageInfo> images = _Timeline.GetImages (_RowIndex);			

			var cell = tableView.DequeueReusableCell (ikey) as PhotoCell;
			if (cell == null)
				cell = new PhotoCell (UITableViewCellStyle.Default, ikey, images, _RowIndex, _Timeline.OnPhotoClicked);
			else
				cell.UpdateCell (images, _RowIndex);
			
			return cell;
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing)
			{
				/*
				if (Value != null){
					Value.Dispose ();
					Value = null;
				}
				*/
			}
			base.Dispose (disposing);
		}				
	}
}
