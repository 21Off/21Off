using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using System;

namespace MSP.Client
{
	public class UserElementII : Element
	{
		public readonly User User;
		public readonly RelationType Relation;
		static NSString ukey = new NSString ("UserElement");
		
		public UserElementII (User user, RelationType relation) : this (user)
		{
			Relation = relation;
		}
		
		public UserElementII (User user) : base (user.Name)
		{
			User = user;
		}		
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (ukey);
			if (cell == null){
				cell = new UserCell (ukey) {
					SelectionStyle = UITableViewCellSelectionStyle.Blue,
					Accessory = UITableViewCellAccessory.DisclosureIndicator
				};
			}
			((UserCell) cell).UpdateFromUser (User);
			
			return cell;
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, MonoTouch.Foundation.NSIndexPath path)
		{
			tableView.DeselectRow (path, true);
			
			var uinav = AppDelegateIPhone.AIphone.GetCurrentNavControler();
			if (uinav != null)
			{
				var membersPhotoView = new MembersPhotoViewControler(uinav, User.Id, false);
				uinav.PushViewController(membersPhotoView, true);
			
				//dvc.ActivateController (membersPhotoView);
			}
		}
	}	
}
