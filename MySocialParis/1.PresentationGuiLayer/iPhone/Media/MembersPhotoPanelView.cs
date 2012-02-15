using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;

namespace MSP.Client
{
	public class MembersPhotoPanelView : TapView, IImageUpdated
	{
		#region Members
		
		const int userSize = 19;
		const int locationSize = 14;
		const int TextX = 60;
		
		private SizeF sizeProfile = new SizeF(75, 75);
		
		private MembersPhotoViewControler _MembersPhotoView;
		private CALayer imageLayer;
		private DarkUIView darkLayer;
		
		User user;
		bool followingUser = false;
		int relation = 0;
		private bool _IsDialogView;
		
		public UINavigationController prevController { get; set; }
		public RootElement MemberRoot {get;set;}
		
		#endregion
		
		#region Constructors
		
		public MembersPhotoPanelView (MembersPhotoViewControler membersPhotoView, bool isDialogView, 
		                              RectangleF rect, int userId) : this(rect)
		{			
			Tapped += HandleTapped;
			TapAndHold +=  HandleTapAndHold;

			
			_IsDialogView = isDialogView;
			_MembersPhotoView = membersPhotoView;
			prevController = AppDelegateIPhone.tabBarController.SelectedViewController as UINavigationController;
			UpdateFromUserId (userId);
		}
		
		private MembersPhotoPanelView (RectangleF rect) : base(rect)
		{
			blocks = new List<Block>();
			
			Initialize (rect);
		}
		
		#endregion

		void HandleTapAndHold (string value)
		{
		
		}
		
		void HandleTapped (string value)
		{
			if (value == "posts")
			{
				
			}
			
			if (value.Contains("admirer"))
				LikeUser();
			
			if (value == user.Name)
				LikeUser();
			
			if (value == "profile")
				LikeUser();
		}

		#region Overrides		

		public override void Draw (RectangleF rect)
		{
			if (user != null)
			{
				var context = UIGraphics.GetCurrentContext ();				
				context.SaveState ();
				
				UIColor.DarkGray.SetColor();
				DrawCaption("friends", 130, 42, 8, 50);
				DrawCaption("admirers", 196, 42, 8, 50);
				DrawCaption("charmers", 262, 42, 8, 50);
				
				DrawCaption(user.FriendsCount.ToString(), 130, 68, 12, 50);
				DrawCaption(user.FollowersCount.ToString(), 196, 68, 12, 50);
				DrawCaption(user.FollowedCount.ToString(), 262, 68, 12, 50);
				
				/*
				using (NSString nss = new NSString("posts"))
				{
					var font = UIFont.FromName("Helvetica", 9);
					var dim = nss.StringSize(font);
									
					var placement = new RectangleF((10 + 26 + 10 - dim.Width) / 2, 
	                           42, dim.Width, dim.Height);				
					
					DrawString("posts", placement, font);
					
					blocks.Add(new Block()
		            {
						Value = "posts",
						Bounds = placement,
						Font = font,
						LineBreakMode = UILineBreakMode.CharacterWrap,
						TextColor = UIColor.DarkGray,
					});
				}
				*/
				
				UIColor.Black.SetColor();
				//DrawCaption(user.PhotoCount.ToString(), 0, 44 + 9, 18, 10 + 26 + 10);
				DrawCaption(user.PhotoCount.ToString(), 0, 68, 12, 10 + 26 + 10);
				
				context.RestoreState ();

				/*
				context.TranslateCTM (10 + 26 + 10, 3);
				context.AddPath (borderPath);
				context.SetRGBStrokeColor (0.5f, 0.5f, 0.5f, 1);
				context.SetLineWidth (0.5f);
				context.StrokePath ();
				*/
			}			
		}
		
		private void HandleDarkLayerOnDraw (RectangleF rect, CGContext context, UIView view)
		{
			if (user == null)
				return;
			
			context.SaveState ();
			
			UIColor.LightGray.SetColor();			
									
			SizeF userSize = SizeF.Empty;
			//TODO Check the case when user informations downloading fails
			using (NSString nss = new NSString(user.Name))
			{
				var font = UIFont.FromName("Helvetica-Bold", 18);
				userSize = nss.StringSize(font);
				var placement = new RectangleF (130, 2, userSize.Width, userSize.Height);
				DrawString (user.Name, placement, font);
				
				blocks.Add(new Block()
	            {
					Value = user.Name,
					Bounds = placement,
					Font = font,
					LineBreakMode = UILineBreakMode.WordWrap,
					TextColor = UIColor.LightGray,
				});
			}
			
			int mainUserID = AppDelegateIPhone.AIphone.MainUser.Id;
				
			if (user.Id != mainUserID)				
			{			
				UIColor relColor = UIColor.LightGray;
				string relString = "";
				if (relation == (int)Relation.None)
				{
					relColor = UIColor.Red;
					relString = "tap to become admirer";
				}
				if (relation == (int)Relation.UserFollowsYou)
				{
					relColor = UIColor.Red;
					relString = "admirer: tap to become friend";
				}
				if (relation == (int)Relation.YouFollowUser)
					relString = "charmer";
				if (relation == (int)Relation.MutualFollowing)
					relString = "friend";
					
				relColor.SetColor();
				using (NSString nss = new NSString(relString))
				{
					var font = UIFont.FromName("Helvetica", 9);
					var dim = nss.StringSize(font);
					var placement = new RectangleF(130, 2 + userSize.Height, dim.Width, dim.Height);
					DrawString(relString, placement, font);
					
					blocks.Add(new Block()
		            {
						Value = relString,
						Bounds = placement,
						Font = font,
						LineBreakMode = UILineBreakMode.WordWrap,
						TextColor = relColor,
					});
				}
			}						
			
			context.RestoreState ();			
		}		
		
		private Block DrawCaption(string text, int xPos, int yPos, int yHeight, int xWidth)
		{
			using (NSString friends = new NSString(text))
			{				
				var font = UIFont.FromName("Helvetica-Bold", yHeight);
								
				var dim = friends.StringSize(font);
				var xPad = (xWidth - dim.Width) / 2;
				var yPad = (yHeight - dim.Height) / 2;
				var placement = new RectangleF (xPos + xPad, yPos + yPad, dim.Width, dim.Height);
				
				DrawString (text, placement, font);
				
				return new Block()
	            {
					Value = text,
					Bounds = placement,
					Font = font,
					LineBreakMode = UILineBreakMode.CharacterWrap,
					//TextColor = UIColor.LightGray,
				};
			}
		}
		
		#endregion		
		
		#region Events		
		
		void LikeUser()
		{
			if (user == null)
				return;
			
			int mainUserID = AppDelegateIPhone.AIphone.MainUser.Id;
				
			if (user.Id == mainUserID)
				return;
			
			Action act = ()=>
			{
				try
				{
					var follow = new Follow() { UserId = user.Id, FollowerId = mainUserID, Time = DateTime.UtcNow };
					
					if (!followingUser)
						AppDelegateIPhone.AIphone.FlwServ.FollowUser(follow);
					else
						AppDelegateIPhone.AIphone.FlwServ.UnFollowUser(follow);
					
					followingUser = !followingUser;
					
					UpdateFromUserId(user.Id);
				}
				catch (Exception ex)
				{
					Util.LogException("HandleFriendPicTouchUpOutside", ex);
				}
			};
			if (_MembersPhotoView != null && _MembersPhotoView.View != null)
			{
				AppDelegateIPhone.ShowRealLoading(_MembersPhotoView.View, 
                  	followingUser ? "Unfollowing user" : "Following user", null, act);
			}			
		}
		
		void HandleFriendPicTouchDown (object sender, EventArgs e)
		{
			Action act = ()=>
			{
				try
				{
					var friends = AppDelegateIPhone.AIphone.FlwServ.GetFriendsOfUser(user.Id).ToList();
					if (friends.Count == 0)
						return;
					
					InvokeOnMainThread(() => GotoList("friends", "you admire them, they admire you", friends, RelationType.Friends));
				}
				catch (Exception ex)
				{
					Util.LogException("HandleFollowersPicTouchDown", ex);
				}
			};
			AppDelegateIPhone.ShowRealLoading(_MembersPhotoView.View, "Loading friends", null, act);
		}			
		
		void HandleFollowersPicTouchDown (object sender, EventArgs e)
		{
			if (user == null)
				return;
			
			Action act = ()=>
			{
				try
				{
					var followers = AppDelegateIPhone.AIphone.FlwServ.GetFollowersOfUser(user.Id).ToList();					
					if (followers.Count == 0)
						return;				
					
					InvokeOnMainThread(()=>GotoList("admirers", "members that admire you", followers, RelationType.Admirers));
				}
				catch (Exception ex)
				{
					Util.LogException("HandleFollowersPicTouchDown", ex);
				}
			};
			AppDelegateIPhone.ShowRealLoading(_MembersPhotoView.View, "Loading admirers", null, act);
		}
		
		void HandleFollowedPicTouchUpInside (object sender, EventArgs e)
		{
			if (user == null)
				return;
			
			Action act = ()=>
			{
				try
				{
					var followed = AppDelegateIPhone.AIphone.FlwServ.GetFollowedByUser(user.Id).ToList();
					if (followed.Count == 0)
						return;							
					
					InvokeOnMainThread(() => GotoList("charmers", "members that you admire", followed, RelationType.Charmers));
					
				}
				catch (Exception ex)
				{
					Util.LogException("HandleFollowedPicTouchUpInside", ex);
				}
			};
			AppDelegateIPhone.ShowRealLoading(_MembersPhotoView.View, "Loading charmers", null, act);
		}		
		
		void HandleBackPicTouchUpInside (object sender, EventArgs e)
		{
			if (_IsDialogView)
			{
				_MembersPhotoView.DismissModalViewControllerAnimated(true);
				//prevController.DismissModalViewControllerAnimated(true);
			}
			else
			{			
				prevController = AppDelegateIPhone.tabBarController.SelectedViewController as UINavigationController;
				
				if (prevController == null)
					return;
				
				//prevController.DismissModalViewControllerAnimated (true);			
				prevController.PopViewControllerAnimated(false);
			}
		}
		
		#endregion
		
		#region Private methods
		
		// Used to update asynchronously our display when we get enough information about the tweet detail
		public void UpdateFromUserId (int userId)
		{
			try
			{
				int askerId = AppDelegateIPhone.AIphone.MainUser.Id;
				FullUserResponse fullUser = AppDelegateIPhone.AIphone.UsersServ.GetFullUserById(userId, askerId);
				if (fullUser == null) return;				
												
				followingUser = fullUser.InRelation == 1 || fullUser.InRelation == 3;
				relation = fullUser.InRelation;
				
				user = fullUser.User;
				user.FriendsCount = fullUser.FriendsCount;
				user.FollowersCount = fullUser.FollowersCount;
				user.FollowedCount = fullUser.FollowedCount;
				user.PhotoCount = fullUser.ImagesCount;
				
				UIImage profileImage = null;
				if (user.HasPhoto == 1)
					profileImage = ImageStore.RequestFullPicture(userId, userId, SizeDB.SizeProfil, this);
				
				profileImage = profileImage ?? ImageStore.EmptyProfileImage;
				
				imageLayer.Contents = UIImageUtils.resizeImage(profileImage, sizeProfile).CGImage;
				InvokeOnMainThread(()=>
                {				
					SetNeedsDisplay ();
					darkLayer.Update();
				});
			}
			catch (Exception ex)
			{
				Util.LogException("UpdateFromUserId", ex);
			}
		}						
		
		private void Initialize (RectangleF rect)
		{
			BackgroundColor = UIColor.White;
			
			this.darkLayer = new DarkUIView();
			darkLayer.OnDraw += HandleDarkLayerOnDraw;
			darkLayer.Frame = new RectangleF(0, 1, UIScreen.MainScreen.Bounds.Width, 40 - 1);
			this.AddSubview(darkLayer);
			
			imageLayer = new CALayer();
			imageLayer.CornerRadius = 2.0f;
			imageLayer.BorderColor = UIColor.LightGray.CGColor;
			imageLayer.BorderWidth = 2.0f;
			imageLayer.ShadowColor = UIColor.DarkGray.CGColor;
			imageLayer.ShadowRadius = 1.0f;
			imageLayer.ShadowOffset = new SizeF(0, 1);
			imageLayer.MasksToBounds = true;
			var placement = new RectangleF(new PointF(10 + 26 + 10, 3), sizeProfile);
			imageLayer.Frame = placement;
			this.Layer.AddSublayer(imageLayer);
			
			blocks.Add(new Block()
            {
				Value = "profile",
				Bounds = placement,	
				Type = BlockType.Image,
			});
			
			// Pics are 75x75, but we will add a border.
			//profilePic = new UIImageView (new RectangleF (new PointF(10 + 26 + 10 + 2, 3 + 2), sizeProfile));
			//profilePic.BackgroundColor = UIColor.Clear;			
			
			var backPic = UIButton.FromType (UIButtonType.Custom);
			backPic.TouchUpInside += HandleBackPicTouchUpInside;
			backPic.SetBackgroundImage (Graphics.GetImgResource("back"), UIControlState.Normal);
			backPic.Frame = new RectangleF (10, 7, 26, 26);
			backPic.BackgroundColor = UIColor.Clear;
			AddSubview (backPic);
			
			var mapPic = UIButton.FromType (UIButtonType.Custom);
			mapPic.TouchUpInside += HandleMapPicTouchUpInside;
			mapPic.SetBackgroundImage (Graphics.GetImgResource("map"), UIControlState.Normal);
			mapPic.Frame = new RectangleF (10, 42, 26, 26);
			mapPic.BackgroundColor = UIColor.Clear;
			AddSubview (mapPic);			
			
			var followersPic = UIButton.FromType (UIButtonType.Custom);
			var followedPic = UIButton.FromType (UIButtonType.Custom);
			var friendsPic = UIButton.FromType (UIButtonType.Custom);						
			
			followedPic.TouchUpInside += HandleFollowedPicTouchUpInside;
			followersPic.TouchDown += HandleFollowersPicTouchDown;		
			friendsPic.TouchDown+= HandleFriendPicTouchDown;
			
			followersPic.SetBackgroundImage (Graphics.GetImgResource("admirer"), UIControlState.Normal);
			followedPic.SetBackgroundImage (Graphics.GetImgResource("charmer"), UIControlState.Normal);
			friendsPic.SetBackgroundImage (Graphics.GetImgResource("friend"), UIControlState.Normal);			
						
			followersPic.Frame = new RectangleF (196, 48, 50, 20);
			followedPic.Frame = new RectangleF (262, 48, 50, 20);
			friendsPic.Frame = new RectangleF (130, 48, 50, 20);
						
			AddSubview (followersPic);
			AddSubview (followedPic);
			AddSubview (friendsPic);
		}

		void HandleMapPicTouchUpInside (object sender, EventArgs e)
		{
			Action act = () =>
			{
				//var fullImages = AppDelegateIPhone.AIphone.ImgServ.GetImagesOfUser(user.Id, DateTime.MaxValue).ToList();
				List<Image> images = new List<Image>();
				foreach (var element in MemberRoot[0])
				{
					if (element is MemberPhotoElement)
					{
						var memberPhoto = (MemberPhotoElement)element;
						images.Add(memberPhoto.Tweet.Image);
					}
				}
				
				InvokeOnMainThread(()=>
				{
					var _MSP = AppDelegateIPhone.aroundNavigationController;
					var headersInfos = new HeaderInfos() { SubTitle = user.Name, Title = "Posts" };					
					var b = new PhotoMapViewController(_MSP.VisibleViewController, images, headersInfos);
					b.View.Frame = UIScreen.MainScreen.Bounds;
					
					_MSP.VisibleViewController.PresentModalViewController(b, true);
				});
			};
			AppDelegateIPhone.ShowRealLoading(null, "Localizing photos", null, act);
		}		
		
		private void GotoList(string title, string subTitle, IEnumerable<User> list, RelationType relation)
		{
			try
			{
				var root = new RootElement(title) {
					new Section() 					
				};
				
				foreach (User user in list)
				{
					root[0].Add(new UserElementII(user, relation));
				}
				root.UnevenRows = true;
				
				var selectedVC = AppDelegateIPhone.tabBarController.SelectedViewController as UINavigationController;
				var rvc = new RelationsViewController(selectedVC, title, subTitle, root);
				selectedVC.PushViewController (rvc, false);
			}
			catch (Exception ex)
			{
				Util.LogException("GotoList : " + title, ex);
			}
		}		
		
		#endregion

		#region IImageUpdated implementation
						
		public void UpdatedImage (long id, long userid, SizeDB sizeDB)
		{
			var profileImage = ImageStore.GetLocalFullPicture (id, userid, SizeDB.SizeProfil);
			if (profileImage != null)
			{
				Graphics.ConfigLayerHighRes(imageLayer);
				imageLayer.Contents = UIImageUtils.resizeImage(profileImage, sizeProfile).CGImage;
				//profilePic.Image = UIImageUtils.resizeImage(profileImage, sizeProfile);
				SetNeedsDisplay();
			}
		}
		
		#endregion
	}
	
	public enum RelationType
	{
		Friends,
		Charmers,
		Admirers,
	}
}

