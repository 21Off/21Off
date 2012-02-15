using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TweetStation;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	// 
	// TweetCell used for the timelines.   It is relatlively light, and 
	// does not do highlighting.   This might work for the iPhone, but
	// for the iPad we probably should just use TweetViews that do the
	// highlighting of url-like things
	//	
	public class ActivityCell : UITableViewCell {
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
		static UIColor timeColor = UIColor.FromRGB(95, 95, 95);
		
		TweetCellView tweetView;
		
		static CGPath badgePath = GraphicsUtil.MakeRoundedPath (54, 4);
		
		public static int CellStyle;
		
		// Should never happen
		public ActivityCell (IntPtr handle) : base (handle) {
			Console.WriteLine (Environment.StackTrace);
		}
		
		public class TweetCellView : UIView, IImageUpdated {
			UIActivity activity;
			
			UIButton imageBtn;
			UIButton userBtn;
			
			UIImage tweetImage;
			UIImage userImage;
			
			List<Block> blocks;
			
			Action<int> _goToMembersPhotoAction;
			Action<UIActivity> _goToPhotoDetailsAction;			
			
			public TweetCellView (UIActivity _activity, Action<int> goToMembersPhotoAction, 
			                      Action<UIActivity> goToPhotoDetailsAction) 
				: base ()
			{			
				_goToMembersPhotoAction = goToMembersPhotoAction;
				_goToPhotoDetailsAction = goToPhotoDetailsAction;
				
				blocks = new List<Block>();
				
				userBtn = UIButton.FromType(UIButtonType.Custom);
				userBtn.TouchUpInside += OnUserClicked;
								
				Opaque = true;
				BackgroundColor = UIColor.FromHSBA(0, 0, 0, 0);
								
				float xPic = PicXPad;
				float actX = 320 - 50 - xPic;				
				
				var rect1 = new RectangleF (xPic, PicYPad, PicSize, PicSize);
								
				userBtn.Frame = rect1;
				
				this.AddSubview(userBtn);
				
				var rect2 = new RectangleF (actX, PicYPad, PicSize, PicSize);
				imageBtn = UIButton.FromType(UIButtonType.Custom);
				imageBtn.TouchUpInside += OnImageClicked;					
				imageBtn.Frame = rect2;
				
				if (_activity.Type != ActivityType.UserFollow && _activity.Type != ActivityType.PhotoLiker)
				{
					imageAdded = true;
					this.AddSubview(imageBtn);
				}
				
				Update(_activity);
			}
			
			private void OnUserClicked(object sender, EventArgs e)
			{
				if (_goToMembersPhotoAction != null)
					_goToMembersPhotoAction(activity.User.Id);
			}			
			
			private void OnImageClicked(object sender, EventArgs e)
			{
				if (activity.Image == null)
					return;
				
				if (_goToPhotoDetailsAction != null)
					_goToPhotoDetailsAction(activity);
			}
					
			private bool imageAdded = false;
			public void Update (UIActivity _activity)
			{
				try
				{
					if (_activity != activity)
					{
						this.activity = _activity;
						
						if (activity.Type != ActivityType.UserFollow && activity.Type != ActivityType.PhotoLiker)
						{
							if (!imageAdded)
							{
								InvokeOnMainThread(() => 
			                    { 
									imageBtn.Hidden = false;
									this.AddSubview(imageBtn); 
									this.BringSubviewToFront(imageBtn);
								});
								imageAdded = true;
							}
							
							UIImage img = null;
							if (activity.DbActivity.IdPhoto.HasValue)
							{
								img = ImageStore.RequestFullPicture(activity.DbActivity.IdPhoto.Value, 
									activity.Image.UserId, SizeDB.Size50, this);
							}
							
							tweetImage = img ?? ImageStore.DefaultImage;													
							imageBtn.SetBackgroundImage(tweetImage, UIControlState.Normal);													
						}
						else
						{					
							InvokeOnMainThread(() => 
		                    { 
								imageBtn.Hidden = true;
								this.BringSubviewToFront(imageBtn);
								imageBtn.RemoveFromSuperview();								
							});
							imageAdded = false;							 
						}
						
						UIImage profileImg = null;
												
						if (activity.User.HasPhoto == 1)
						{
							long userId = (long)activity.User.Id;
							profileImg = ImageStore.RequestFullPicture(userId, userId, SizeDB.SizeProfil, this);
						}
						
						userImage = profileImg ?? ImageStore.EmptyProfileImage;
						userBtn.SetBackgroundImage(userImage, UIControlState.Normal);
					
						InvokeOnMainThread(() => SetNeedsDisplay ());
					}
				}
				catch (Exception ex)
				{
					Util.LogException("Update", ex);
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

				// Superview is the container, its superview the uitableviewcell
				bool highlighted = (Superview.Superview as UITableViewCell).Highlighted;				
				LayoutActivity(activity, Bounds.Width, blocks);

				//UIColor.White.SetColor ();
				//context.FillRect (bounds);
				//context.DrawLinearGradient (bottomGradient, new PointF (midx, bounds.Height-17), new PointF (midx, bounds.Height), 0);
				//context.DrawLinearGradient (topGradient, new PointF (midx, 1), new PointF (midx, 3), 0);
								
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
												
				if (activity.Type != ActivityType.UserFollow && activity.Type != ActivityType.PhotoLiker)
				{
					float actX = Bounds.Width - 50 - PicXPad;
					
					// Cute touch
					UIColor.White.SetColor ();
					context.SaveState ();
					context.TranslateCTM (actX - 2, PicYPad - 2);
					context.SetLineWidth (1);
					
					// On device, the shadow is painted in the opposite direction!
					context.SetShadowWithColor (new SizeF (1, 1), 3, UIColor.DarkGray.CGColor);
					context.AddPath (badgePath);
					context.FillPath ();					
					
					context.RestoreState ();				
				}
				
				// Cute touch
				UIColor.White.SetColor ();
				context.SaveState ();
				context.TranslateCTM (PicXPad - 2, PicYPad - 2);
				context.SetLineWidth (1);
				
				// On device, the shadow is painted in the opposite direction!
				context.SetShadowWithColor (new SizeF (1, 1), 3, UIColor.DarkGray.CGColor);
				context.AddPath (badgePath);
				context.FillPath ();
				context.RestoreState ();
			}
			
			
			public static float LayoutActivity(UIActivity activity, float width, List<Block> blocks)
			{
				string userText = activity.User.Name;
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
				float remainingSpace = width - 4 * PicXPad - 2 * PicSize - userTextWidth - TextWidthPadding;
				int line = 0;
					
				float res = 0.0f;
				List<string> splits =  activity.Text.Split(new string[] {" "}, StringSplitOptions.RemoveEmptyEntries).ToList();
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
								int count = dim.Width > remainingSpace ? r : r + 1;
								float pos = 2 * PicXPad + PicSize + (line == 0 ? userTextWidth + TextWidthPadding : 0);
								
								using (var nss2 = new NSString(text))
								{
									dim = nss2.StringSize(textFont);
								}
									
								var placement = new RectangleF (pos, TextHeightPadding * (line + 1) + textSize * line, 
								            dim.Width, textSize);
								
								blocks.Add(new Block()
					            {
									Value = text,
									Bounds = placement,
									Font = textFont,
									LineBreakMode = UILineBreakMode.WordWrap,
									TextColor = UIColor.Black,
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
				
				DateTime tweetCreation = activity.DbActivity.Time;
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
						TextColor = timeColor,
					});
					res += timeSize + TextHeightPadding;
				}
				
				return res;
			}

			public void UpdatedImage (long id, long userid, SizeDB sizeDB)
			{				
				if (activity == null)
					return;
				
				if (sizeDB == SizeDB.Size50)
				{
					if (activity.DbActivity.IdPhoto != id)
						return;
					
					tweetImage = ImageStore.GetLocalFullPicture (id, userid, SizeDB.Size50);
					if (tweetImage != null)
						imageBtn.SetBackgroundImage(tweetImage, UIControlState.Normal);				
				}
				
				if (sizeDB == SizeDB.SizeProfil)
				{
					if (activity.DbActivity.IdOwner != userid)
						return;
					
					userImage = ImageStore.GetLocalFullPicture (id, userid, SizeDB.SizeProfil);
					if (userImage != null)
						userBtn.SetBackgroundImage(userImage, UIControlState.Normal);					
				}
				SetNeedsDisplay ();				
			}
		}
		
		// Create the UIViews that we will use here, layout happens in LayoutSubviews
		public ActivityCell (UITableViewCellStyle style, NSString ident, UIActivity tweet, 
		                     Action<int> goToMembersPhotoAction, Action<UIActivity> goToPhotoDetailsAction)
			: base (style, ident)
		{
			SelectionStyle = UITableViewCellSelectionStyle.None;
			
			tweetView = new TweetCellView (tweet, goToMembersPhotoAction, goToPhotoDetailsAction);
			ContentView.Add (tweetView);
		}

		// 
		// This method is called when the cell is reused to reset
		// all of the cell values
		//
		public void UpdateCell (UIActivity tweet)
		{
			tweetView.Update (tweet);
			SetNeedsDisplay ();
		}

		public static float GetCellHeight (RectangleF bounds, UIActivity activity)
		{			 
			return Math.Max(2 * PicYPad + PicSize, TweetCellView.LayoutActivity(activity, bounds.Width, new List<Block>()));
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			
			tweetView.Frame = ContentView.Bounds;
			tweetView.SetNeedsDisplay ();
			
		}
	}	
}
