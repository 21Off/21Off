using System;
using MonoTouch.Dialog;
using Share;
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using System.Drawing;
using System.Collections.Generic;
using MonoTouch.CoreGraphics;
using System.Threading;
using TweetStation;
using System.Net;
using System.Web;

namespace MSP.Client
{
	public class FbUserElement : OwnerDrawnElement, IImageUpdated
	{
		private static object lock_graph = new object();
		private static Dictionary<decimal, GraphUser> graph = new Dictionary<decimal, GraphUser>();
		
		private GraphUser _User;
		private UIFont fromFont = UIFont.BoldSystemFontOfSize(14.0f);
		private UIImage userImage;
		private UIButton _inviteButton;
		private Action<string> _openUrl;
		
		public FbUserElement(GraphUser user, Action<string> openUrl) : base(UITableViewCellStyle.Value1, "FbUserElement", 
            UITableViewCellSelectionStyle.None, UITableViewCellAccessory.None)
		{
			_openUrl = openUrl;
			_User = user;
			_inviteButton = UIButton.FromType (UIButtonType.RoundedRect);
			_inviteButton.Frame = new RectangleF(0, (_height - 30)/2, 60, 30);
			_inviteButton.SetTitleColor(UIColor.LightGray, UIControlState.Normal);
			_inviteButton.SetTitle("invite", UIControlState.Normal);
			_inviteButton.TouchDown += Handle_inviteButtonTouchDown;
		}		
		
		private NSString nss = new NSString("x");
		
		public override void DrawImageView (UIImageView view)
		{				
			return;
			
			if (userImage == null)
			{
				userImage = ImageStore.RequestFullPicture((long)_User.id, (long)_User.id, SizeDB.SizeFacebook, this);
				userImage = userImage ?? ImageStore.EmptyProfileImage;
				userImage = UIImageUtils.resizeImage(userImage, new SizeF (35, 35));
				userImage = GraphicsII.RemoveSharpEdges(userImage);
				if (userImage != null)
					view.Image = userImage;
			}
			else
				view.Image = userImage;
			
			view.SetNeedsDisplay();
			
			base.DrawImageView (view);
		}
			
		/*
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			base.Selected (dvc, tableView, path);
		}
		*/
		
		public override void Draw (RectangleF bounds, CGContext context, UIView view)
		{			
			UIColor.White.SetFill ();
			context.FillRect (bounds);
			
			RectangleF frame = _inviteButton.Frame;
			frame.X = view.Frame.Width - frame.Width - 10;
			_inviteButton.Frame = frame;
			view.Add(_inviteButton);
			
			if (userImage != null)
			{
				//context.DrawImage(new RectangleF((_height - 35)/2, (_height - 35)/2, 35, 35), userImage.CGImage);
				userImage.Draw(new RectangleF((_height - 35)/2, (_height - 35)/2, 35, 35));
			}
			
			if (graph.ContainsKey(_User.id))
				_User = graph[_User.id];
			
			UIColor.Black.SetColor ();
			if (_User.name != null)
			{
				view.DrawString(_User.name, new RectangleF(50, 5, bounds.Width/2, 10 ), fromFont, UILineBreakMode.TailTruncation);
				
				if (userImage == null)
				{
					userImage = ImageStore.RequestFullPicture((long)_User.id, (long)_User.id, SizeDB.SizeFacebook, this);
					userImage = userImage ?? ImageStore.EmptyProfileImage;
					userImage = UIImageUtils.resizeImage(userImage, new SizeF (35, 35));
					userImage = GraphicsII.RemoveSharpEdges(userImage);
					if (userImage != null)
						userImage.Draw(new RectangleF((_height - 35)/2, (_height - 35)/2, 35, 35));
				}				
			}
			else
			{
				ThreadPool.QueueUserWorkItem(o =>
				{					
					GraphUser gUser = AppDelegateIPhone.AIphone.FacebookServ.GetFriend(_User.id);
					if (gUser == null)
						return;
					
					lock (lock_graph)
					{
						graph[gUser.id] = gUser;
					}
					
					if (gUser.id == _User.id)
					{
						_User = gUser;
						nss.InvokeOnMainThread(()=> view.SetNeedsDisplay());
					}
				});
			}
		}
		
		private float _height = 40f;
		
		public override float Height (RectangleF bounds)
		{
			return _height;
		}
		
		void Handle_inviteButtonTouchDown (object sender, EventArgs e)
		{
			string accessToken = NSUserDefaults.StandardUserDefaults.StringForKey("FacebookAccessToken");
			string url = string.Format("https://www.facebook.com/dialog/apprequests?app_id={0}&message={1}&redirect_uri={2}&to={3}&access_token={4}",
			                           FaceBook.FaceBookApplication._appId, "Test message", "http://www.21off.net", _User.id, accessToken);
			
			if (_openUrl != null)
				_openUrl(url);			
		}

		#region IImageUpdated implementation
		public void UpdatedImage (long id, long userid, SizeDB sizeDB)
		{
			if (_User != null)
			{
				if (_User.id == userid)
				{
					userImage = ImageStore.GetLocalFullPicture((long)_User.id, (long)_User.id, SizeDB.SizeFacebook);
					if (userImage != null)
					{
						userImage = UIImageUtils.resizeImage(userImage, new SizeF (35, 35));
						userImage = GraphicsII.RemoveSharpEdges(userImage);
					
						this.GetContainerTableView ().SetNeedsLayout();
					}
				}
			}
		}
		#endregion
	}		
}

