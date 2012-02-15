using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public class UserElement : OwnerDrawnElement
	{
		private User _User;
		private UIFont fromFont = UIFont.BoldSystemFontOfSize(14.0f);
		private UIImage userImage;
		
		public UserElement(User user) : base(UITableViewCellStyle.Default, "UserElement")
		{
			_User = user;
		}
		
		public override void Draw (RectangleF bounds, CGContext context, UIView view)
		{
			UIColor.White.SetFill ();
			context.FillRect (bounds);
			
			//if (userImage == null)
			//	userImage = UIImageUtils.GetPreview (string.Format("Images/Profiles/{0}.jpg", _User.Id), new SizeF (40, 40));

			if (userImage != null)
				context.DrawImage(new RectangleF(0, 0, 40, 40), userImage.CGImage);
			
			UIColor.Black.SetColor ();
			view.DrawString(_User.Name, new RectangleF(50, 5, bounds.Width/2, 10 ), fromFont, UILineBreakMode.TailTruncation);
		}
		
		public override float Height (RectangleF bounds)
		{
			return 40.0f;
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			var uinav = (UINavigationController) AppDelegateIPhone.tabBarController.SelectedViewController;			
			dvc.ActivateController (new MembersPhotoViewControler(uinav, _User.Id, true));
		}
	}
}