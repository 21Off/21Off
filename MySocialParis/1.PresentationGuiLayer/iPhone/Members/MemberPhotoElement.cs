using System;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TweetStation;

namespace MSP.Client
{
	public class MemberPhotoElement : Element, IElementSizing {
		static NSString key = new NSString ("MemberPhotoElement");
		public Tweet Tweet;
		private Action<int> _GoToMembersPhotoAction;
		
		public MemberPhotoElement (Tweet tweet, Action<int> goToMembersPhotoAction) : base (null)
		{
			Tweet = tweet;	
			_GoToMembersPhotoAction = goToMembersPhotoAction;
		}		
		
		// Gets a cell on demand, reusing cells
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (key) as MemberPhotoCell;
			if (cell == null)
				cell = new MemberPhotoCell (UITableViewCellStyle.Default, key, Tweet, _GoToMembersPhotoAction);
			else
				cell.UpdateCell (Tweet);
			
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
			return MemberPhotoCell.GetCellHeight (tableView.Bounds, Tweet);
		}
		
		#endregion
	}	
}

