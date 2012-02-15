using System;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TweetStation;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public class ActivityElement : Element, IElementSizing {
		static NSString key = new NSString ("ActivityElement");
		public UIActivity activity;
		private Action<int> _GoToMembersPhotoAction;
		private Action<UIActivity> _GoToPhotoDetailsAction;

		public ActivityElement (UIActivity tweet, Action<int> goToMembersPhotoAction, Action<UIActivity> goToPhotoDetailsAction)
			: base (null)
		{
			activity = tweet;	
			_GoToMembersPhotoAction = goToMembersPhotoAction;
			_GoToPhotoDetailsAction = goToPhotoDetailsAction;
		}
		
		// Gets a cell on demand, reusing cells
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (key) as ActivityCell;
			if (cell == null)
				cell = new ActivityCell (UITableViewCellStyle.Default, key, activity, _GoToMembersPhotoAction, _GoToPhotoDetailsAction);
			else
				cell.UpdateCell (activity);
			
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
			return 50;
			return ActivityCell.GetCellHeight (tableView.Bounds, activity);
		}
		#endregion
	}
	
	
}

