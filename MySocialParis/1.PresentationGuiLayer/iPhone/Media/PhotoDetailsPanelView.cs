using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;

namespace MSP.Client
{
	public class PhotoDetailsPanelView : TapView, IImageUpdated
	{
		#region Members
		
		const int userSize = 19;
		
		static UIFont userFont = UIFont.BoldSystemFontOfSize (userSize);
		
		private float PanelHeight = 75/2;
		
		int relation = 0;
		UIImageView profilePic;
		FullUserResponse fullUser;
		User user;
		bool _IsModal;
		Action backAction;
		
		#endregion

		public PhotoDetailsPanelView (RectangleF rect, FullUserResponse photoOwner, Image image, bool isModal, 
			Action backAction) : base(rect)
		{			
			this.backAction = backAction;
			fullUser = photoOwner;
			
			relation = fullUser.InRelation;
			//TODO user.HasPhoto is not assigned!!!
			user = photoOwner.User;
			_IsModal = isModal;
			
			Initialize (rect);
			UpdateFromUserId();
			
			TappedBlock += OnBlockTapped;
			Tapped += HandleTapped;
		}
		
		private RectangleF profileImgFrame;
		
		private void Initialize (RectangleF rect)
		{
			Layer.BackgroundColor = UIColor.Black.CGColor;				
			Layer.ShadowColor = UIColor.LightGray.CGColor;
			Layer.ShadowRadius = 1.0f;
			Layer.ShadowOffset = new SizeF(0, -1);
			Layer.ShadowOpacity = 0.8f;
			
			profileImgFrame = new RectangleF (20 + 26, (PanelHeight - 26)/2, 26, 26);
			profilePic = new UIImageView (profileImgFrame);
			profilePic.BackgroundColor = UIColor.Clear;
			
			var backPic = UIButton.FromType (UIButtonType.Custom);
			
			backPic.TouchUpInside += HandleBackPicTouchUpInside;
			backPic.SetImage(Graphics.GetImgResource("back"), UIControlState.Normal);
			backPic.Frame = new RectangleF (5, (PanelHeight - 26)/2, 26, 26);
			
			AddSubview (backPic);
			backPic.BackgroundColor = UIColor.Clear;
			
			AddSubview (profilePic);			
			AddSubview (backPic);
		}			

		public UIViewController prevController { get; set; }

		void HandleBackPicTouchUpInside (object sender, EventArgs e)
		{
			if (!_IsModal)
			{
				var navController = prevController as UINavigationController;								
				navController.PopViewControllerAnimated(true);
			}
			else
				prevController.DismissModalViewControllerAnimated (true);
			
			if (backAction != null)
			{
				backAction();
				//BeginInvokeOnMainThread(() => backAction());
			}
		}

		// Used to update asynchronously our display when we get enough information about the tweet detail
		public void UpdateFromUserId ()
		{							
			int userId = user.Id;
			UIImage profileImage = null;
			
			if (user.HasPhoto == 1)
			{
				profileImage = ImageStore.RequestFullPicture(userId, userId, SizeDB.SizeProfil, this);
			}
			
			profileImage = profileImage ?? ImageStore.EmptyProfileImage;			
			profileImage = UIImageUtils.resizeImage(profileImage, new SizeF (26, 26));
			profilePic.Image = GraphicsII.RemoveSharpEdges(profileImage);
		}

		public event NSAction PictureTapped;
		
		#region Overrides
		
		private void OnBlockTapped(Block block)
		{
			if (PictureTapped != null)
				PictureTapped ();
		}
		
		void HandleTapped (string value)
		{
			
		}		
		
		public override void Draw (RectangleF rect)
		{
			// If we have a partialTweet, we do not have this information yet.
			if (user == null)
				return;
			
			blocks.Clear();
			
			UIColor txtColor = UIColor.FromRGB(239, 239, 239);
			txtColor.SetColor();			
			
			using (var nss = new NSString (user.Name))
			{
				var dim = nss.StringSize (userFont);
				var placement = new RectangleF (30 + 2 * 26, 2, dim.Width, dim.Height);
				
				var userBlock = new Block()
	            {
					Value = user.Name,
					Bounds = placement,
					Font = userFont, 
					TextColor = txtColor,
				};
				
				blocks.Add(userBlock);
				
				DrawString (user.Name, placement, userFont, UILineBreakMode.TailTruncation);
			};
			
			UIColor relColor = UIColor.LightGray;
			string relString = "";			
			
			if (relation == (int)Relation.UserFollowsYou)
			{
				relColor = UIColor.Red;
				relString = "admirer";
			}
			if (relation == (int)Relation.YouFollowUser)
				relString = "charmer";
			if (relation == (int)Relation.MutualFollowing)
				relString = "friend";
			
			if (relString != "")
			{
				relColor.SetColor();
				using (var nss = new NSString (relString))
				{
					var font = UIFont.FromName("Helvetica", 9);
					var dim = nss.StringSize(font);					
					var placement = new RectangleF (30 + 2 * 26, 4 + userSize, dim.Width, dim.Height);
					
					var userBlock = new Block()
		            {
						Value = relString,
						Bounds = placement,
						Font = font, 
						TextColor = relColor,
					};
					
					blocks.Add(userBlock);
					
					DrawString (relString, placement, font, UILineBreakMode.TailTruncation);
				};				
				
			}
			
			var imgBlock = new Block()
			{ 
				 Value = "profile",
				 Bounds = profileImgFrame,
				 Type = BlockType.Image,
			};
			blocks.Add(imgBlock);			
		}
		
		#endregion						

		#region IImageUpdated implementation
		
		public void UpdatedImage (long id, long userid, SizeDB sizeDB)
		{
			var profileImage = ImageStore.GetLocalFullPicture (id, userid, SizeDB.SizeProfil);
			if (profileImage != null)
			{
				profileImage = UIImageUtils.resizeImage(profileImage, new SizeF (26, 26));
				profilePic.Image = GraphicsII.RemoveSharpEdges(profileImage);
				SetNeedsDisplay();
			}
		}
		
		#endregion
	}	
}
