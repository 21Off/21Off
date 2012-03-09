using System;
using System.Linq;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TweetStation;
using MSP.Client.DataContracts;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MSP.Client
{
	public class CommentCell : UITableViewCell {
		const int userSize = 14;
		const int textSize = 14;
		const int timeSize = 12;
		
		const int PicSize = 50;
		const int PicXPad = 10;
		const int PicYPad = 5;
		
		const int PicAreaWidth = 2 * PicXPad + PicSize;
		
		const int TextHeightPadding = 4;
		const int TextWidthPadding = 4;
		const int TextYOffset = userSize + 8;
		const int MinHeight = PicSize + 2 * PicYPad;
		const int TimeWidth = 46;
		
		static UIFont userFont = UIFont.BoldSystemFontOfSize (userSize);
		static UIFont textFont = UIFont.SystemFontOfSize (textSize);
		static UIFont timeFont = UIFont.SystemFontOfSize (timeSize);
		
		CommentCellView tweetView;
		
		private UIComment _Comment;
		public UIComment Comment {get { return _Comment;}}
		
		static CGPath badgePath = GraphicsUtil.MakeRoundedPath (54, 4);
		static CGPath commentPath;
		
		public static int CellStyle;
		
		// Should never happen
		public CommentCell (IntPtr handle) : base (handle) {
			Console.WriteLine (Environment.StackTrace);
		}
		
		public class CommentCellView : UIView, IImageUpdated {
			UIComment _comment;
			
			List<Block> blocks;
			
			UIButton activityBtn;
			UIImage activityImage;
			
			public CommentCellView (UIComment comment, Action<int> goToMembersPhotoAction) : base ()
			{							
				blocks = new List<Block>();
				activityBtn = UIButton.FromType(UIButtonType.Custom);
				
				activityBtn.TouchUpInside += (s, e)=>
				{
					if (goToMembersPhotoAction != null)
						goToMembersPhotoAction(_comment.PhotoOwner.Id);
				};
				
				Opaque = true;
				BackgroundColor = UIColor.FromHSBA(0, 0, 0, 0);

				activityBtn.Frame = new RectangleF (10, 1 + (56 - PicSize) / 2, PicSize, PicSize);
				
				this.AddSubview(activityBtn);
				activityBtn.TouchUpInside += HandleActivityBtnTouchUpInside;
				
				Update(comment);
			}

			void HandleActivityBtnTouchUpInside (object sender, EventArgs e)
			{
				var uinav = (UINavigationController) AppDelegateIPhone.tabBarController.SelectedViewController;
				Action act = () =>
				{
					InvokeOnMainThread(()=>
					{
						var membersPhotoView = new MembersPhotoViewControler (uinav, _comment.CommentOwner.Id, false);
											
						uinav.PushViewController(membersPhotoView, true);
						//ActivateController (membersPhotoView);
					});
				};
				AppDelegateIPhone.ShowRealLoading(null, "Loading user photos", null, act);				
			}
			
			private int currentUserId = -1;
			public void Update (UIComment comment)
			{
				if (this._comment != comment)
				{
					this._comment = comment;
					try
					{
						var user = _comment.CommentOwner ?? AppDelegateIPhone.AIphone.UsersServ.GetUserById(_comment.Comment.UserId);
						_comment.CommentOwner = user;
						
						if (currentUserId != user.Id)
						{	
							currentUserId = user.Id;
							
							activityImage = TweetStation.ImageStore.DefaultImage;
							activityBtn.SetBackgroundImage(activityImage, UIControlState.Normal);
							
							NSTimer.CreateScheduledTimer (1, delegate {
								
								this.UpdateProfileImage(user);
							});
						}
					
					}
					catch (Exception ex)
					{
						this._comment = null;
						Util.LogException("UpdateComment", ex);
					}
					InvokeOnMainThread(() => SetNeedsDisplay ());
				}
			}
			
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
			
			void RealDraw (RectangleF rect)
			{			
				blocks.Clear();
				var context = UIGraphics.GetCurrentContext ();
					
				var height = LayoutComment(_comment, Bounds.Width, blocks);
				height = Math.Max(60, height);
				commentPath = GraphicsUtil.MakeRoundedRectPath(new RectangleF(0, 0, 310, height), 4);
				
				// Cute touch				
				UIColor.FromRGB(239, 239, 239).SetColor ();
				context.SaveState ();
				context.TranslateCTM (5, (60 - 56) / 2);
				context.SetLineWidth (1);
				
				// On device, the shadow is painted in the opposite direction!
				context.SetShadowWithColor (new SizeF (1, 1), 3, UIColor.DarkGray.CGColor);
				context.AddPath (commentPath);
				context.FillPath ();
				context.RestoreState ();
								
				// Cute touch
				UIColor.Black.SetColor ();
				context.SaveState ();
				context.TranslateCTM (9, (56 - PicSize) / 2);
				context.SetLineWidth (1);
				
				// On device, the shadow is painted in the opposite direction!
				context.SetShadowWithColor (new SizeF (1, 1), 3, UIColor.LightGray.CGColor);
				context.AddPath (badgePath);
				context.FillPath ();					
								
				context.RestoreState ();
				
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
			
			public static float LayoutComment(UIComment activity, float width, List<Block> blocks)
			{
				string userText = activity.CommentOwner == null ? "NoName" : activity.CommentOwner.Name;
				SizeF userTextDim = SizeF.Empty;
				using (NSString nss = new NSString(userText))
				{
					userTextDim = nss.StringSize(userFont);
					
					var placement = new RectangleF (PicSize + 2 * PicXPad, TextHeightPadding, userTextDim.Width, userSize);
					blocks.Add(new Block()
		            {
						Value = userText,
						Bounds = placement,
						Font = userFont,
						LineBreakMode = UILineBreakMode.WordWrap,
						TextColor = UIColor.Black,
					});
				}				
				
				float userTextWidth = userTextDim.Width;
				float remainingSpace = width - 2 * PicXPad - PicSize - userTextWidth - TextWidthPadding;				
				
				List<string> splits =  activity.Comment.Name.Split(new string[] {" "}, 
					StringSplitOptions.RemoveEmptyEntries).ToList();
				
				float res = LayoutList (splits, width, blocks, userTextWidth, remainingSpace, 
					TextHeightPadding, 0, PicSize, textSize);
				
				DateTime tweetCreation = activity.Comment.Time;
				var ts = DateTime.UtcNow - tweetCreation;				
				
				string time = string.Format("{0} ago", Util.FormatTime (ts));
				using (NSString nss = new NSString(time))
				{
					var dim  = nss.StringSize(timeFont);
					
					var placement = new RectangleF (PicSize + 2 * PicXPad, res, dim.Width, timeSize);
					blocks.Add(new Block()
		            {
						Value = time,
						Bounds = placement,
						Font = timeFont,
						LineBreakMode = UILineBreakMode.WordWrap,
						TextColor = UIColor.FromRGB (95, 95, 95),
					});
					res += timeSize + TextHeightPadding;
				}				
				
				return res;
			}
			
			void UpdateProfileImage(User user)
			{
				if (_comment == null || currentUserId != user.Id)
					return;
				
				UIImage profileImage = null;
				if (user.HasPhoto == 1)
				{
					profileImage = ImageStore.RequestFullPicture(user.Id, user.Id, SizeDB.SizeProfil, this);
				}
				
				profileImage = profileImage ?? ImageStore.EmptyProfileImage;			
				profileImage = UIImageUtils.resizeImage(profileImage, new SizeF(PicSize, PicSize));
				var sharpImage = GraphicsII.RemoveSharpEdges(profileImage);
				
				activityBtn.SetBackgroundImage(sharpImage, UIControlState.Normal);
				this.BeginInvokeOnMainThread(SetNeedsDisplay);
			}
		

			#region IImageUpdated implementation
			public void UpdatedImage (long id, long userid, SizeDB sizeDB)
			{
				if (currentUserId != userid)
					return;
				
				UIImage profileImage = ImageStore.GetLocalFullPicture(id, userid, sizeDB);
				if (profileImage != null)
				{
					profileImage = UIImageUtils.resizeImage(profileImage, new SizeF(PicSize, PicSize));
					var sharpImage = GraphicsII.RemoveSharpEdges(profileImage);
					
					activityBtn.SetBackgroundImage(sharpImage, UIControlState.Normal);
					SetNeedsDisplay ();						
				}
			}
			#endregion
}
		
		// Create the UIViews that we will use here, layout happens in LayoutSubviews
		public CommentCell (UITableViewCellStyle style, NSString ident, UIComment comment, 
		                     Action<int> goToMembersPhotoAction)
			: base (style, ident)
		{
			SelectionStyle = UITableViewCellSelectionStyle.None;
			_Comment = comment;
			
			tweetView = new CommentCellView (comment, goToMembersPhotoAction);
			UpdateCell (comment);
			ContentView.Add (tweetView);
		}

		// 
		// This method is called when the cell is reused to reset
		// all of the cell values
		//
		public void UpdateCell (UIComment tweet)
		{
			tweetView.Update (tweet);
			SetNeedsDisplay ();
		}

		public static float GetCellHeight (RectangleF bounds, UIComment comment)
		{
			return Math.Max(60, CommentCellView.LayoutComment(comment, bounds.Width, new List<Block>()));
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			
			tweetView.Frame = ContentView.Bounds;
			tweetView.SetNeedsDisplay ();			
		}
		
		public static float LayoutList (List<string> splits, float width, List<Block> blocks, 
			float userTextWidth, float remainingSpace, float textHeightPadding, float lineY, 
			float PicSize, float textSize)
		{
			float res = 0.0f;
			int line = 0;					

			while (true)
			{
				string tempUserText = "";
				for (int r = 0; r < splits.Count; r++)
				{
					string oldTemp = tempUserText;
					if (r < splits.Count - 1)													   
						tempUserText += splits[r] + " ";
					else
						tempUserText += splits[r];
					
					using (var nss = new NSString (tempUserText)){
						var dim = nss.StringSize (textFont);
						if (dim.Width > remainingSpace || r == splits.Count - 1)
						{											
							string text = dim.Width > remainingSpace ? oldTemp : tempUserText;
							if (text == "") 
							{
								splits.Clear();
								break;
							}
								
							int count = dim.Width > remainingSpace ? r : r + 1;							
							float pos = 2 * PicXPad + PicSize + (line == 0 ? userTextWidth + TextWidthPadding : 0);
							
							using (var nss2 = new NSString(text))
							{
								dim = nss2.StringSize(textFont);
							}
														
							var placement = new RectangleF (pos, lineY + textHeightPadding * (line + 1) + textSize * line, 
							            dim.Width, textSize);
							
	  						var regx = new Regex("http://([\\w+?\\.\\w+])+([a-zA-Z0-9\\~\\!\\@\\#\\$\\%\\^\\&amp;\\*\\(\\)_\\-\\=\\+\\\\\\/\\?\\.\\:\\;\\'\\,]*)?", 
					                     RegexOptions.IgnoreCase); 
    
		    				Match match = regx.Match(text);
							
							blocks.Add(new Block()
				            {
								Value = match.Success ? match.Value : text,
								Bounds = placement,
								Font = textFont,
								LineBreakMode = UILineBreakMode.WordWrap,
								TextColor = UIColor.Black,
								Type = match.Success ? BlockType.Url : BlockType.Text,
							});							
							
							remainingSpace = width - 4 * PicXPad - 2 * PicSize;
							line++;
							splits.RemoveRange(0, count);
							break;
						}
					}
				}
				if (splits.Count == 0)
				{
					res = line * (TextHeightPadding + textSize);
					break;
				}
			}
			
			return res;
		}		
		
	}	
}
