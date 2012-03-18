using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;

namespace MSP.Client
{
	public class UserCell : UITableViewCell, ISizeImageUpdated {
		User user;

		public UserCell (NSString key) : base (UITableViewCellStyle.Subtitle, key)
		{
		}
		
		public void UpdateFromUser (User user)
		{
			if (user == this.user)
				return;
			
			this.user = user;
			TextLabel.Text = user.Name;
			//DetailTextLabel.Text = string.Format("{0} photos", user.PhotoCount);
			
			UIImage profileImage = null;
			if (user.HasPhoto == 1)
				profileImage = ImageStore.RequestFullPicture(user.Id, user.Id, SizeDB.SizeProfil, this);
			
			profileImage = profileImage ?? ImageStore.EmptyProfileImage;			
			profileImage = UIImageUtils.resizeImage(profileImage, new SizeF (35, 35));
			
			ImageView.Image = GraphicsII.RemoveSharpEdges(profileImage);
			SetNeedsDisplay ();
		}
		
		public void UpdatedImage (long id, long userid, SizeDB sizeDB)
		{
			if (userid == user.Id)
			{
				UIImage profileImage = ImageStore.GetLocalFullPicture(id, userid, sizeDB);
				if (profileImage != null)
				{
					profileImage = UIImageUtils.resizeImage(profileImage, new SizeF (35, 35));			
					ImageView.Image = GraphicsII.RemoveSharpEdges(profileImage);
					SetNeedsDisplay();
				}
			}
		}		
	}	
}
