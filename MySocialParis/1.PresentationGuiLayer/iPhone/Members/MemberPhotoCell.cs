using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;
using RedPlum;

namespace MSP.Client
{	
	public class MemberPhotoCell : UITableViewCell {			
			
		#region Private members
		
		const int userSize = 14;
		const int textSize = 14;
		const int timeSize = 12;
		
		const int PicXPad = 12 / 2;
		const int PicYPad = 12 / 2;
		
		const int TextHeightPadding = 4;
		const int TextWidthPadding = 4;
		const int TextYOffset = userSize + 8;
		const int TimeWidth = 46;
		
		const int TitleHeight = 48 / 2;		
		
		const int PicHeight = 615 / 2;
		public const int MiniMapHeight = 80 / 2;
		public const int MiniMapWidth = 460 / 2;
		
		static UIFont userFont = UIFont.BoldSystemFontOfSize (userSize);
		static UIFont textFont = UIFont.SystemFontOfSize (textSize);				
		
		private MemberPhotoCellView tweetView;
		public static int CellStyle;
		
		#endregion
		
		// Should never happen
		public MemberPhotoCell (IntPtr handle) : base (handle) {
			Console.WriteLine (Environment.StackTrace);
		}		
				
		public class MemberPhotoCellView : TapView, IImageUpdated {
			
			#region Private members
			
			Tweet _Tweet;
			int PicWidth;			
			
			UIButton photoBtn;
			UIImage photoImage;
			
			private Action<int> goToUserPhotos;
			private UIActivityIndicatorView spinner;
			private UIImage mapPlanImage;
			private static UIImage EmptyImage;
			private UIButton keywordView, attentionView, addToEvent;
			private DateTime lastPhotoClicked;
			private int photoClickCnt = 0;
			private static UIImage googleLogo;
			
			#endregion
	
			public MemberPhotoCellView (Tweet tweet, Action<int> goToMembersPhotoAction) : base ()
			{				
				Tapped += OnTapped;
				TappedBlock += OnTappedBlock;
				TapAndHold += OnTapAndHold;
				TapAndHoldCall += OnTapAndHoldCall;
				
				this.goToUserPhotos = goToMembersPhotoAction;
				
				blocks = new List<Block> ();
				bool isMyImage = (tweet.User.Id == AppDelegateIPhone.AIphone.MainUser.Id);
				if (googleLogo == null)
					googleLogo = UIImage.FromBundle("Images/googleLogo.PNG");
				
				Opaque = true;
				BackgroundColor = UIColor.FromHSBA(0, 0, 0, 0);
												
				PicWidth = 320 - 2 * PicXPad;
				
				photoBtn = UIButton.FromType(UIButtonType.Custom);
				photoBtn.Layer.MasksToBounds = true;
				photoBtn.Layer.CornerRadius = 8;
				photoBtn.Layer.Opaque = true;

				Graphics.ConfigLayerHighRes(photoBtn.Layer);
				photoBtn.Frame = new RectangleF (PicXPad + 1, 2 * PicYPad + TitleHeight + 1, PicWidth - 2, PicHeight - 2);
				photoBtn.TouchUpInside += PhotoClicked;
				
				this.AddSubview(photoBtn);
							
				var img1 = Graphics.GetImgResource("like40");
				var img2 = Graphics.GetImgResource("comment40");
				var img3 = Graphics.GetImgResource("keyword");
				var img4 = Graphics.GetImgResource("alert");
				
				var img5 = Graphics.GetImgResource("alert");
				
				var rect1 = new RectangleF(2 * PicXPad, 5 * PicYPad + PicHeight + TitleHeight, 40, 40);
				var rect2 = new RectangleF(320 - 2 * PicXPad - 26, 5 * PicYPad + PicHeight + TitleHeight, 40, 40);
				var rect3 = new RectangleF(2 * PicXPad, 5 * PicYPad + PicHeight + TitleHeight + 100, 26, 26);
				var rect4 = new RectangleF(320 - 2 * PicXPad - 26, 5 * PicYPad + PicHeight + TitleHeight + 100, 26, 26);
				
				var rect5 = new RectangleF(320 - 2 * PicXPad - 26, PicYPad, 26, 26);
				
				var rectSpinner = new RectangleF((320 - 30)/2, 2 * PicYPad + TitleHeight + (PicHeight - 30) / 2, 30, 30);
			
				var interesantBtnView = UIButton.FromType (UIButtonType.Custom);
				interesantBtnView.BackgroundColor = UIColor.Clear;
				interesantBtnView.Frame = rect1;
				interesantBtnView.SetBackgroundImage(img1, UIControlState.Normal);
															                  					
				var commentairesView = UIButton.FromType (UIButtonType.Custom);
				commentairesView.BackgroundColor = UIColor.Clear;
				commentairesView.Frame = rect2;								
				commentairesView.SetBackgroundImage(img2, UIControlState.Normal);
				
				addToEvent = UIButton.FromType(UIButtonType.Custom);
				addToEvent.BackgroundColor = UIColor.Clear;
				addToEvent.Frame = rect5;
				addToEvent.SetBackgroundImage(img5, UIControlState.Normal);
			
				keywordView = UIButton.FromType (UIButtonType.Custom);
				keywordView.BackgroundColor = UIColor.Clear;
				keywordView.Frame = rect3;
				keywordView.SetBackgroundImage(img3, UIControlState.Normal);
			
				attentionView = UIButton.FromType (UIButtonType.Custom);
				attentionView.BackgroundColor = UIColor.Clear;
				attentionView.Frame = rect4;
				attentionView.SetBackgroundImage(img4, UIControlState.Normal);
				
				AddSubview (interesantBtnView);
				AddSubview (commentairesView);
				if (isMyImage) 
					AddSubview (keywordView);
				
				AddSubview (attentionView);
				AddSubview(addToEvent);
				
				interesantBtnView.TouchUpInside += LikePhoto;
				commentairesView.TouchUpInside += GotoComments;
				if (isMyImage) 
					keywordView.TouchUpInside += SeeKeywords;
				attentionView.TouchUpInside += GotoAttention;	
				addToEvent.TouchDown += HandleAddToEventTouchDown;
				
				if (EmptyImage == null)
					EmptyImage = UIImageUtils.MakeEmpty(new Size(320 - 2 * PicXPad, PicHeight));
				
				spinner = new UIActivityIndicatorView (UIActivityIndicatorViewStyle.Gray){
								Frame = rectSpinner
							};
				
				AddSubview (spinner);
				
				photoBtn.SetBackgroundImage(EmptyImage, UIControlState.Normal);
				photoBtn.ContentMode = UIViewContentMode.ScaleAspectFill;
				
				Update(tweet);
			}

			void HandleAddToEventTouchDown (object sender, EventArgs e)
			{
				
			}
							
			#region Events
			
			private void SeeKeywords(object sender, EventArgs e)
			{
				Action act = ()=>				
				{
					var _MSP = AppDelegateIPhone.tabBarController.SelectedViewController as UINavigationController;
					InvokeOnMainThread(()=>
					{
						var b = new PhotoKeywordsViewController(_MSP.VisibleViewController, this._Tweet.Image.Id, _Tweet.User);					
						_MSP.VisibleViewController.PresentModalViewController(b, true);
					});
				};
				
				AppDelegateIPhone.ShowRealLoading(AppDelegateIPhone.AIphone.MainWnd, "Loading keywords", null, act);				
			}
			
			private void LikePhoto(object sender, EventArgs e)
			{
				GotoPhotoLikers(_Tweet.Image);
			}
			
			private void LikeAction()
			{
				try
				{				
					int imageID = _Tweet.Image.Id;
					int likerID = AppDelegateIPhone.AIphone.MainUser.Id;

					var like = new Like()
					{ 
						ParentId = imageID, 
						UserId = likerID,
						Time = DateTime.UtcNow 
					};
					int likesCount = AppDelegateIPhone.AIphone.LikesServ.LikeImage(like);
					if (_Tweet != null)
						_Tweet.LikesCount = likesCount;
					
					InvokeOnMainThread(SetNeedsDisplay);
				}
				catch (Exception ex)
				{
					Util.LogException("LikeAction", ex);
				}
			}
			
			private void PhotoClicked(object sender, EventArgs e)
			{
				if (_Tweet.User.Id == AppDelegateIPhone.AIphone.MainUser.Id)
					return;
				
				photoClickCnt++;
				
				if (photoClickCnt == 1)
					lastPhotoClicked = DateTime.Now;
				
				if (photoClickCnt == 2)
				{
					DateTime now = DateTime.Now;
					if ((now - lastPhotoClicked) < TimeSpan.FromMilliseconds(500))
						AppDelegateIPhone.ShowRealLoading(photoBtn, "Liked", null, LikeAction);
					
					photoClickCnt = 0;
				}						
			}
			
			private void GotoAttention(object sender, EventArgs e)
			{
				bool isPhotoOwner = _Tweet.Image.UserId == AppDelegateIPhone.AIphone.MainUser.Id;
				
				var actionSheet = new UIActionSheet("Post options")
				{
					Style = UIActionSheetStyle.Default,
				};
				
				if (isPhotoOwner)
				{
					actionSheet.DestructiveButtonIndex = actionSheet.AddButton("Delete");
					actionSheet.AddButton("Share");
					actionSheet.CancelButtonIndex = actionSheet.AddButton("Cancel");
					actionSheet.Clicked += delegate (object s, UIButtonEventArgs args)
					{
						switch (args.ButtonIndex)
						{
							case 0: // Delete
							{
								if (_Tweet.DeleteAction != null)
									_Tweet.DeleteAction(_Tweet);
								break;
							}
							case 1: // Share
							{		
								var _MSP = AppDelegateIPhone.tabBarController.SelectedViewController as UINavigationController;
								var vc = new SocialNetworksShareViewController(_MSP,_Tweet.Image);
								_MSP.PushViewController(vc, true);							
								break;
							}
							case 2: // Cancel
							{
								break;
							}
						}
					};
				}
				else
				{
					actionSheet.DestructiveButtonIndex = actionSheet.AddButton("Report content");
					actionSheet.AddButton("Share");
					actionSheet.CancelButtonIndex = actionSheet.AddButton("Cancel");
					actionSheet.Clicked += delegate (object s, UIButtonEventArgs args)
					{
						switch (args.ButtonIndex)
						{
							case 0: // Report content
							{
								var reporter = new ContentReporter("", "Why ?", "Cancel", "Report content");
								reporter.SelectedImage = _Tweet.Image;
								reporter.Show();
								break;
							}
							case 1: // Share
							{		
								var _MSP = AppDelegateIPhone.tabBarController.SelectedViewController as UINavigationController;
								var vc = new SocialNetworksShareViewController(_MSP,_Tweet.Image);
								_MSP.PushViewController(vc, true);							
								break;
							}
							case 2: // Cancel
							{
								break;
							}
						}
					};
				}
				
				actionSheet.ShowFromTabBar (AppDelegateIPhone.tabBarController.TabBar);
			}			
			
			private void GotoComments(object sender, EventArgs e)
			{
				Action act = ()=>
				{
					var _MSP = AppDelegateIPhone.aroundNavigationController;
					InvokeOnMainThread(()=>
					{
						var b = new PhotoCommentsViewController(_MSP.VisibleViewController, this._Tweet.Image.Id, _Tweet.User);					
						_MSP.VisibleViewController.PresentModalViewController(b, true);
					});
				};
				
				AppDelegateIPhone.ShowRealLoading(AppDelegateIPhone.AIphone.MainWnd, "Loading comments", null, act);
			}
				
			private void GotoMap()
			{
				var _MSP = AppDelegateIPhone.aroundNavigationController;							
				var b = new PhotoMapViewController(_MSP.VisibleViewController, this._Tweet.Image);
				b.View.Frame = UIScreen.MainScreen.Bounds;
				
				_MSP.VisibleViewController.PresentModalViewController(b, true);
			}
			
			private void GotoPhotoLikers(Image image)
			{
				Action act = ()=>
				{
					var _MSP = AppDelegateIPhone.aroundNavigationController;
					InvokeOnMainThread(()=>
					{
						var b = new PhotoLikersViewController(_MSP.VisibleViewController, this._Tweet);
						_MSP.VisibleViewController.PresentModalViewController(b, true);
					});
				};
				
				AppDelegateIPhone.ShowRealLoading(AppDelegateIPhone.AIphone.MainWnd, "Showing photo likers", null, act);				
			}
			
			private void GotoList(string title)
			{
				Action act = ()=>
				{
					var images = new List<Image>();
					var keywResp = AppDelegateIPhone.AIphone.KeywServ.GetSimilarImages(title, DateTime.MaxValue.Ticks);
					var similarImages = keywResp.Images;
					
					foreach (SimilarImage similarImage in similarImages)
					{
						images.Add(similarImage.Image);
					}									
					
					InvokeOnMainThread(()=>
	                {
						var _MSP = AppDelegateIPhone.tabBarController.SelectedViewController as MSPNavigationController;
						
						var m = new SearchByKeywordViewController(_MSP, images, title, false);
						_MSP.PushViewController(m, true);
					});
					
				};
				AppDelegateIPhone.ShowRealLoading(null, "Searching images", null, act);				
			}
			
			private void OnTapped(string value)
			{				
				if (value == seeAllComments)
					GotoComments(null, new EventArgs());
				
				if (value == "map")
					GotoMap();
			}
			
			private void OnTappedBlock(Block block)
			{
				/*
				if (block.Tag == "likesCount")
				{
					GotoPhotoLikers(block.CallObject as Image);	
				}
				*/
				if (block.Tag == "keyword")
				{					
					GotoList((string)block.CallObject);
				}
				
				if (block.CallObject is User)
				{
					if (goToUserPhotos != null)
						goToUserPhotos((block.CallObject as User).Id);
				}				
			}
			
			private void OnTapAndHold(string value)
			{
				
			}
			
			private void OnTapAndHoldCall(object value)
			{
							
			}		
			
			#endregion
			
			#region Updates
			
			public void Update (Tweet tweet)
			{					
				this._Tweet = tweet;					
				
				mapPlanImage = ImageStore.RequestFullPicture(tweet.Image.Id, tweet.Image.UserId, SizeDB.SizeMiniMap, this);
				if (mapPlanImage != null)
					SetMiniMap(mapPlanImage);
				
				photoImage = ImageStore.RequestFullPicture(tweet.Image.Id, tweet.Image.UserId, SizeDB.SizeFull, this);
				
				if (photoImage == null)
				{					
					photoImage = EmptyImage;
					spinner.StartAnimating ();
				}
				else
					spinner.StopAnimating();
				
				photoBtn.SetBackgroundImage(photoImage, UIControlState.Normal);
				SetNeedsDisplay ();
			}
			
			private void SetMiniMap(UIImage mapImage)
			{
				var planSize = new SizeF (MiniMapWidth, MiniMapHeight);
				
				mapPlanImage = UIImageUtils.overlayImage(mapImage, 
                 	  Graphics.GetImgResource("position"),
                      new RectangleF(new PointF(0, 0), planSize),
                      new RectangleF(new PointF((planSize.Width - 34)/2, (planSize.Height - 34)/2),
                      new SizeF(34, 34)), planSize);
			}
			
			#region IImageUpdated implementation
			
			public void UpdatedImage (long id, long userid, SizeDB sizeDB)
			{
				if (_Tweet == null)
					return;
				
				if (sizeDB == SizeDB.SizeMiniMap)
				{
					if (_Tweet.Image.Id != id)
						return;
					
					mapPlanImage = ImageStore.GetLocalFullPicture(id, userid, SizeDB.SizeMiniMap);
					
					if (mapPlanImage != null)
						SetMiniMap(mapPlanImage);
				}
				else
				{			
					if (_Tweet.Image.Id != id)
						return;
					
					if (_Tweet.User.Id != userid)
						return;					
					
					photoImage = ImageStore.GetLocalFullPicture(id, userid, SizeDB.SizeFull);
					if (photoImage != null)
					{
						photoBtn.SetBackgroundImage(photoImage, UIControlState.Normal);
						spinner.StopAnimating();						
					}
				}
				
				SetNeedsDisplay();
			}
			
			#endregion
			
			#endregion
			
			#region Draw Methods
			
			int state;
			public override void Draw (RectangleF rect)
			{
				try {
					state = 0;
					RealDraw (rect);
				} catch (Exception e) {
					Util.LogException ("State: " + state, e);
				}
			}		
			
			private CGPath titlePath;
			private CGPath badgePath;
			private CGPath minimapPath;
			private CGPath imagePath;
				
			private void RealDraw (RectangleF rect)
			{
				if (_Tweet == null)
					return;
				
				blocks.Clear();
				
				var bounds = Bounds;				
				
				var rectMiniMap = new RectangleF(0, 0, MiniMapWidth, MiniMapHeight);
				float cellHeight = 0;
				float lineY = Layout(bounds, _Tweet, blocks, out cellHeight);
				
				titlePath = titlePath ?? GraphicsUtil.MakeRoundedRectPath(new RectangleF(PicXPad, 0, PicWidth, TitleHeight), 4);
				badgePath = GraphicsUtil.MakeRoundedRectPath(new RectangleF(PicXPad, 0, PicWidth, cellHeight), 4);
				minimapPath = minimapPath ?? GraphicsUtil.MakeRoundedRectPath(rectMiniMap, 4);
				imagePath = imagePath ?? GraphicsUtil.MakeRoundedRectPath(new RectangleF(0, 0, PicWidth, PicHeight), 8);
				var context = UIGraphics.GetCurrentContext ();
		
				/****	Title	****/			
				UIColor.FromRGB(239, 239, 239).SetColor ();
				context.SaveState ();
				context.TranslateCTM (0, PicYPad);
				context.SetLineWidth (1);
				
				// On device, the shadow is painted in the opposite direction!
				context.SetShadowWithColor (new SizeF (1, 1), 3, UIColor.DarkGray.CGColor);
				context.AddPath (titlePath);				
				context.FillPath ();
				context.RestoreState ();
				/*****************************************************************************/
				
				/****	Commentaires	****/				
				UIColor.FromRGB(239, 239, 239).SetColor ();
				context.SaveState ();
				context.TranslateCTM (0, 3 * PicYPad + PicHeight + TitleHeight);
				context.SetLineWidth (1);
				
				// On device, the shadow is painted in the opposite direction!
				context.SetShadowWithColor (new SizeF (1, 1), 3, UIColor.DarkGray.CGColor);
				context.AddPath (badgePath);
				context.FillPath ();
				context.RestoreState ();
				/*****************************************************************************/
								
				/****	Image	****/
				UIColor.FromRGB(239, 239, 239).SetColor ();
				context.SaveState ();
				context.TranslateCTM (PicXPad, 2 * PicYPad + TitleHeight);
				context.SetLineWidth (1);
								
				// Device and Sim interpret the Y for the shadow differently.
				context.SetShadowWithColor (new SizeF (1, 1), 3, UIColor.DarkGray.CGColor);
				context.AddPath (imagePath);
				context.FillPath();
				context.RestoreState ();
				/*****************************************************************************/				
				
				/****	Minimap	****/	
				context.SaveState ();
				context.TranslateCTM ((bounds.Width - MiniMapWidth)/2, 4 * PicYPad + PicHeight + TitleHeight);
				context.AddPath (minimapPath);
				UIColor.LightGray.SetColor ();
	
				context.SetLineWidth (1);
				// Device and Sim interpret the Y for the shadow differently.
				context.SetShadowWithColor (new SizeF (0, -1), 1, UIColor.DarkGray.CGColor);
				context.StrokePath ();
				
				// Clip the image to the path and paint it
				if (mapPlanImage != null){
					context.AddPath (minimapPath);
					context.Clip ();					
					mapPlanImage.Draw (new RectangleF (0, 0, MiniMapWidth, MiniMapHeight));
					var block = new Block()
					{
						Value = "map",
						Bounds = new RectangleF((bounds.Width - MiniMapWidth)/2, 4 * PicYPad + PicHeight + TitleHeight, 
						                        MiniMapWidth, MiniMapHeight),
						Type = BlockType.Image,
					};
					blocks.Add(block);					
	            }
				if (googleLogo != null)
					googleLogo.Draw(new RectangleF(2, MiniMapHeight - 2 - 23 / 2, 69 / 2, 23 / 2)); 
				
				context.RestoreState ();
				/*****************************************************************************/																				
								
				DrawIcons(lineY, bounds);
				
				foreach (Block block in blocks)
				{
					if (block.Type == BlockType.Text)
					{
						block.TextColor.SetColor();
						if (block.LineBreakMode.HasValue)
							DrawString (block.Value, block.Bounds, block.Font, block.LineBreakMode.Value, UITextAlignment.Left);
						else
							DrawString (block.Value, block.Bounds, block.Font);
					}
				}
			}
				
			public const string seeAllComments = "->see all comments";
				
			public static float Layout(RectangleF bounds, Tweet tweet, List<Block> blocks, out float cellHeight)
			{				
				float lineY = TitleHeight + PicHeight + 3 * PicYPad;
				string imageName = tweet.Image.Name ?? "";
				using (var nss = new NSString (imageName))
				{
					var dim = nss.StringSize (userFont, bounds.Width - 4 * PicXPad, UILineBreakMode.TailTruncation);
					var placement = new RectangleF (2 * PicXPad, PicYPad + (TitleHeight - dim.Height) / 2, dim.Width, dim.Height);
					blocks.Add(new Block()
		            {
						Value = imageName, 
						Bounds = placement,
						Font = userFont, 
						LineBreakMode = UILineBreakMode.TailTruncation,						
						TextColor = UIColor.DarkGray,
					});					
				}			
				
				float paragraph = 20;
				cellHeight =  PicYPad + MiniMapHeight + paragraph;
				lineY += cellHeight;
				if (tweet.Comments.Count >= 5)
				{
					using (var nss = new NSString(seeAllComments))
					{
						var placement = new RectangleF (2 * PicXPad, TitleHeight + PicHeight + 4 * PicYPad + MiniMapHeight + 20, 
						                                                  bounds.Width, userSize);
						blocks.Add(new Block()
			            {
							Value = seeAllComments,
							Bounds = placement,
							Font = userFont, 
							TextColor = UIColor.FromRGB(33, 73, 153),
						});
					}
					cellHeight += userSize;
					lineY += userSize;
				}				
				
				float commentsHeight = DrawComments(lineY, bounds, tweet, blocks);
				cellHeight += commentsHeight;
				lineY += commentsHeight;
				
				cellHeight += paragraph;
				lineY += paragraph;
				float keywordsHeight = DrawKeywords(lineY, tweet, blocks);
				cellHeight += Math.Max(keywordsHeight, 26) + PicYPad;
				lineY += Math.Max(keywordsHeight, 26) + PicYPad;
				
				return lineY + 2;
			}
			
			private void DrawIcons(float lineY, RectangleF bounds)
			{
				var rect1 = new RectangleF(2 * PicXPad, 5 * PicYPad + PicHeight + TitleHeight + 26 + PicYPad, 26, userSize);
				var rect2 = new RectangleF(320 - 2 * PicXPad - 26, 5 * PicYPad + PicHeight + TitleHeight + 26 + PicYPad, 26, userSize);
				var rect3 = new RectangleF(2 * PicXPad, lineY - PicYPad - 26, 26, 26);
				var rect4 = new RectangleF(320 - 2 * PicXPad - 26, lineY - PicYPad - 26, 26, 26);
				
//				UIColor color = UIColor.FromRGB(33, 73, 153);
				UIColor color = UIColor.FromRGB(150, 150, 150);
				color.SetColor();
												
				string s1 = _Tweet.LikesCount.ToString();
				
				using (var nss = new NSString (s1))
				{
					var dim = nss.StringSize (userFont);
					rect1.X = 2 * PicXPad + (26 - dim.Width)/2;
					rect1.Width = dim.Width;
					
					var placement = rect1;					
					DrawString (s1, rect1, userFont);
					
					blocks.Add(new Block()
		            {
						Value = s1, 
						Bounds = placement,
						Font = userFont,
						TextColor = color,						
						CallObject = _Tweet.Image,
						Tag = "likesCount",
					});					
				}
				string s2 = _Tweet.Comments.Count.ToString();
				using (var nss = new NSString (s2))
				{
					var dim = nss.StringSize (userFont);
					rect2.X = 320 - 2 * PicXPad - 26 + (26 - dim.Width)/2;
					rect2.Width = dim.Width;
					DrawString (s2, rect2, userFont);
				}
				
				keywordView.Frame = rect3;
			    attentionView.Frame = rect4;				
			}
			
			private static float DrawComments(float lineY, RectangleF bounds, Tweet tweet, List<Block> blocks)
			{
				float commentsHeight = 0;
				int i = 0;
				
				//lineY += PicYPad;
				//commentsHeight += PicYPad;
				
				foreach (Comment comment in tweet.Comments)
				{
					// Show only the first 5 comments
					if (i >= 5)
						break;
					
					User user = tweet.CommentsUsers[i];
					
					//lineY += PicYPad;
					//commentsHeight += PicYPad;
					
					var userDim = new SizeF(0, 0);
					using (var nss = new NSString (user.Name))
					{
						userDim = nss.StringSize (userFont);
						var placement = new RectangleF (2* PicXPad, lineY, bounds.Width, userSize);
						blocks.Add(new Block()
			            {
							Value = user.Name, 
							Bounds = placement,
							Font = userFont,
							TextColor = UIColor.FromRGB(33, 73, 153),
							CallObject = user,					
						});						
					}
					
					float userTextWidth = userDim.Width;
					float remainingSpace = 320 - 4 * PicXPad - PicXPad - userDim.Width;				
					
					List<string> splits = comment.Name.Split(new string[] {" "}, 
						StringSplitOptions.RemoveEmptyEntries).ToList();
					
					float res = CommentCell.LayoutList (splits, 320, blocks, userTextWidth, remainingSpace,
						TextWidthPadding, lineY - PicYPad + 2, 0, textSize);
					
					lineY += res;
					commentsHeight += res;
					/*
					using (var nss = new NSString (comment.Name))
					{						
						var commentWitdh = 320 - 4 * PicXPad - PicXPad - userDim.Width;
						var dim = nss.StringSize (textFont, new SizeF(commentWitdh, 100), UILineBreakMode.WordWrap);
						
						var placement = new RectangleF (2 * PicXPad + userDim.Width + PicXPad, lineY, commentWitdh, dim.Height);						
						blocks.Add(new Block()
			            {
							Value = comment.Name,
							Bounds = placement,
							Font = textFont,
							LineBreakMode = UILineBreakMode.WordWrap,
							TextColor = UIColor.DarkGray,
						});
						
						lineY += dim.Height;
						commentsHeight += dim.Height;
					}
					*/
					i++;
				}
				return commentsHeight;
			}
			
			private static float DrawKeywords(float lineY, Tweet tweet, List<Block> blocks)
			{
				float keywordsHeight = 0;
				
				float dispoWidth = 320 - 6 * PicXPad - 2 * 26;
				float dispoWidthLine = dispoWidth;
				
				var keywords = new List<string>();
				foreach (Keyword keyword in tweet.Keywords)
				{
					keywords.Add(keyword.Name);
				}
				
				int line = 0;
				for (int i = 0; i < keywords.Count; i++)
				{
					string s = "/ " + keywords[i] + " ";
					
					using (var nss = new NSString (s))
					{						
						var dim = nss.StringSize (userFont, new SizeF(300, userSize));
						float keyX = 0;
						if (dim.Width > dispoWidth)
						{
							//TODO CHECK/FINISH THIS
							string[] splits = s.Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries);
							string newWord = "";
							
							int index = -1;
							int j = 0;
							while (j < splits.Length)
							{
								newWord += splits[j] + " ";
								/*
								if (j <= splits.Count - 1)
									newWord += " ";
								*/	
								
								using (var nw = new NSString(newWord))
								{
									var dimNewWord = nw.StringSize(userFont, new SizeF(dispoWidth, userSize));
									if (dimNewWord.Width > dispoWidth)
									{
										if (index == -1)
											index = 0;
										
										keywords.Insert(index, newWord);
										newWord = "";
										index++;
									}
								}
								j++;
							}
							if (index != -1)
								continue;
						}
						
						if (dim.Width <= dispoWidthLine)
						{
						 	keyX = dispoWidth - dispoWidthLine;
							dispoWidthLine -= dim.Width;
						}
						else
						{
							keywordsHeight += userSize + PicYPad;
							lineY += userSize + PicYPad;
							dispoWidthLine = dispoWidth;
							dispoWidthLine -= dim.Width;
							line++;
						}
						
						var placement = new RectangleF (3 * PicXPad + 26 + keyX, lineY, dim.Width, userSize);
						blocks.Add(new Block()
			            {
							Value = s, 
							Bounds = placement,
							Font = userFont,
							TextColor = UIColor.FromRGB(33, 73, 153),
							CallObject = keywords[i],
							Tag = "keyword",
						});
					}
				}
				
				if (line >= 1)
					keywordsHeight += userSize + PicYPad;
				
				return keywordsHeight;
			}

			#endregion
		}
		
		// Create the UIViews that we will use here, layout happens in LayoutSubviews
		public MemberPhotoCell (UITableViewCellStyle style, NSString ident, Tweet tweet, Action<int> goToMembersPhotoAction) 
			: base (style, ident)
		{
			SelectionStyle = UITableViewCellSelectionStyle.Blue;
			
			tweetView = new MemberPhotoCellView (tweet, goToMembersPhotoAction);
			ContentView.Add (tweetView);			
		}

		// 
		// This method is called when the cell is reused to reset
		// all of the cell values
		public void UpdateCell (Tweet tweet)
		{
			tweetView.Update (tweet);
			SetNeedsDisplay ();
		}		
		
		public static float GetCellHeight (RectangleF bounds, Tweet tweet)
		{
			float cellHeight = 0;
			return MemberPhotoCellView.Layout(bounds, tweet, new List<Block>(), out cellHeight);
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			
			tweetView.Frame = ContentView.Bounds;
			tweetView.SetNeedsDisplay ();
		}
	}		
}

