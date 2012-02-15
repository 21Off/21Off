using System;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TweetStation;

namespace MSP.Client
{
	public class CommentElement : Element, IElementSizing {
		static NSString key = new NSString ("CommentElement");
		public UIComment _comment;

		public CommentElement (UIComment comment)
			: base (null)
		{
			_comment = comment;
		}
		
		// Gets a cell on demand, reusing cells
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (key) as CommentCell;
			if (cell == null)
				cell = new CommentCell (UITableViewCellStyle.Default, key, _comment, null);
			else
				cell.UpdateCell (_comment);
			
			return cell;
		}		
		
		#region IElementSizing implementation
		public float GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			//return 50;
			return CommentCell.GetCellHeight (tableView.Bounds, _comment);
		}
		#endregion
	}	
}
