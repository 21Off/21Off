using System;
using System.Drawing;
using System.Linq;
using System.Threading;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;

namespace MSP.Client
{
	public class EditingSource : DialogViewController.Source {
		BaseTimelineViewController parent;
		
		public EditingSource (BaseTimelineViewController dvc) : base (dvc) 
		{
			parent = dvc as BaseTimelineViewController;
		}
		
		public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
		{			
			var section = Container.Root [indexPath.Section];
			var element = section [indexPath.Row];
			if (element is AddLoadMoreWithImageElement)
			{
				return true;
			}
			if (element is CommentElement)
			{
				var cEl = (CommentElement)element;
				UIComment uiComment = cEl._comment;
				
				return (uiComment.CommentOwner.Id == AppDelegateIPhone.AIphone.MainUser.Id);								
			}
			if (element is UserElementII)
			{
				var uElem = (UserElementII)element;
				return (uElem.Relation != RelationType.Admirers);
			}
			if (element is KeywordElement)
			{
				return true;
			}			
			
			//int rows = tableView.NumberOfRowsInSection(0);
				
			// Trivial implementation: we let all rows be editable, regardless of section or row
			return false;
		}
		
		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			return parent.GetHeightForRow(tableView, indexPath);
		}
		
		public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
		{
			var section = Container.Root [indexPath.Section];
			var element = section [indexPath.Row];
			if (element is AddLoadMoreWithImageElement)
			{
				return UITableViewCellEditingStyle.None;
			}
			if (element is CommentElement)
			{
				var cEl = (CommentElement)element;
				UIComment uiComment = cEl._comment;
				
				if (uiComment.CommentOwner.Id == AppDelegateIPhone.AIphone.MainUser.Id)				
					return UITableViewCellEditingStyle.Delete;
				else
					return UITableViewCellEditingStyle.None;				
				
			}
			if (element is UserElementII)
			{
				var uElem = (UserElementII)element;
				if (uElem.Relation == RelationType.Admirers)
					return UITableViewCellEditingStyle.None;
				
				return UITableViewCellEditingStyle.Delete;
			}
			if (element is KeywordElement)
			{
				return UITableViewCellEditingStyle.Delete;
			}
			
			// trivial implementation: show a delete button always
			return UITableViewCellEditingStyle.None;
		}
		
		public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, NSIndexPath indexPath)
		{
			if (AppDelegateIPhone.AIphone.MainUser == null)
				return;
			
			//
			// In this method, we need to actually carry out the request
			//
			var section = Container.Root [indexPath.Section];
			var element = section [indexPath.Row];
			if (element is CommentElement)
			{
				var commentElement = (CommentElement)element;
				var idComment = commentElement._comment.Comment.Id;
				AppDelegateIPhone.AIphone.CommentServ.DeleteComment(idComment);
			}
			if (element is KeywordElement)
			{
				var kEl = (KeywordElement)element;
				var idKeyword = kEl.keyword.Id;
				AppDelegateIPhone.AIphone.KeywServ.DeleteKeyword(idKeyword);
			}
			if (element is UserElementII)
			{
				var uElem = (UserElementII)element;
				if (uElem.Relation != RelationType.Admirers)
				{
					var myId = AppDelegateIPhone.AIphone.MainUser.Id;
					var otherId = uElem.User.Id;
					
					var flw = new Follow()
					{
						FollowerId = myId,
						UserId = otherId,
					};
					AppDelegateIPhone.AIphone.FlwServ.UnFollowUser(flw);
				}
			}
			section.Remove (element);
		}
	}
}
