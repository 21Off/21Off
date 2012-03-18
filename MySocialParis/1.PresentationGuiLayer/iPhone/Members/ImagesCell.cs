using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;

namespace MSP.Client
{
	public class ImagesCell : UITableViewCell {
		const int userSize = 14;
		const int textSize = 14;
		const int timeSize = 12;
		
		const int PicSize = 50;
		static float PicXPad = 10;
		static float PicYPad = 5;
		
		static UIFont userFont = UIFont.BoldSystemFontOfSize (userSize);
		
		ImagesCellView tweetView;
		
		static CGPath badgePath = GraphicsUtil.MakeRoundedPath (54, 4);		
		
		// Should never happen
		public ImagesCell (IntPtr handle) : base (handle) {
			Util.Log (Environment.StackTrace);
		}
		
		public class ImagesCellView : UIView, ISizeImageUpdated {
			ImagesCellInfo imagesCellInfo;
			ImagesCellInfo copy;
			
			List<UIButton> buttons;
			List<UIImage> images;			
			List<Block> blocks;					
			
			public ImagesCellView (ImagesCellInfo imagesCellInfo) 
				: base ()
			{
				blocks = new List<Block>();				
				buttons = new List<UIButton>();
				images = new List<UIImage>();
				
				Opaque = true;
				BackgroundColor = UIColor.FromHSBA(0, 0, 0, 0);
				
				copy = imagesCellInfo;
			}
			
			private bool initDone;
			
			private void InitImages()
			{	
				PicXPad = (this.Frame.Width - copy.Images.Count * PicSize) / (copy.Images.Count + 1);
				PicYPad = (this.Frame.Height - PicSize) / 2;
				for (int i = 0; i < copy.Images.Count; i++)
				{													
					var rect1 = new RectangleF ((i + 1) * PicXPad + i * PicSize, PicYPad, PicSize, PicSize);
					var userBtn = UIButton.FromType(UIButtonType.Custom);
					userBtn.TouchUpInside += OnImageClicked;
					userBtn.Frame = rect1;
					buttons.Add(userBtn);
					images.Add(null);
					
					if (copy.Images[i].Img != null)
					{
						this.AddSubview(userBtn);
						visibleButtons.Add(userBtn);
					}
				}
				
				Update(copy);				
			}
			
			private List<UIButton> visibleButtons = new List<UIButton>();
			
			private void CheckImages()
			{
				for (int i = 0; i < copy.Images.Count; i++)
				{			
					if (copy.Images[i].Img == null)
					{
						if (visibleButtons.Contains(buttons[i]))
						{
							this.WillRemoveSubview(buttons[i]);
							visibleButtons.Remove(buttons[i]);
						}
					}
					else
					{
						if (!visibleButtons.Contains(buttons[i]))
						{
							this.AddSubview(buttons[i]);
							visibleButtons.Add(buttons[i]);
						}
					}
				}
			}
			
			private void OnImageClicked(object sender, EventArgs e)
			{
				Image img = null;
				for (int i = 0; i < buttons.Count; i++)
				{
					if (buttons[i] == (UIButton)sender)
					{
					 	img  = copy.Images[i].Img;
						break;
					}
				}
				if (img == null)
					return;
				
				
				var _MSP = AppDelegateIPhone.tabBarController.SelectedViewController as MSPNavigationController;
				var searchModal = _MSP.ModalViewController;
				//_MSP.DismissModalViewControllerAnimated(false);
				
				Action act = ()=>
				{			
					try
					{
						int askerId = AppDelegateIPhone.AIphone.MainUser.Id;
						FullUserResponse fullUser = AppDelegateIPhone.AIphone.UsersServ.GetFullUserById(img.UserId, askerId);
						if (fullUser == null)
							return;
						
						Action backAct = () =>
						{
							//_MSP.PresentModalViewController(searchModal, true);
						};
						
						BeginInvokeOnMainThread(()=>
						{
							var a = new PhotoDetailsViewController(_MSP, fullUser, img, false, backAct);
							_MSP.PushViewController(a, true);							
							//_MSP.PresentModalViewController(a, true);
						});
					}
					catch (Exception ex)
					{
						Util.LogException("OnPhotoClicked", ex);
					}
				};
				AppDelegateIPhone.ShowRealLoading(null, "Loading photo details", null, act);
			}
			
			public void Update (ImagesCellInfo _imagesCellInfo)
			{
				try
				{
					if (_imagesCellInfo != this.imagesCellInfo)
					{
						this.imagesCellInfo = _imagesCellInfo;
						
						int i = 0;
						foreach (ImageInfo imgInfo in imagesCellInfo.Images)
						{
							UIImage img = null;
							
							if (imgInfo.Img != null)
								img = ImageStore.RequestFullPicture(imgInfo.Img.Id, imgInfo.Img.UserId, SizeDB.Size50, this);
							else
								continue;
							
							images[i] = img ?? ImageStore.DefaultImage;													
							buttons[i].SetBackgroundImage(images[i], UIControlState.Normal);													
							i++;
						}
					
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
				
				//LayoutActivity(activity, Bounds.Width, blocks);
								
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
				
				if (!initDone)
				{
					InitImages();
					initDone = true;
				}				
				
				int i = 0;
				//	Add cute touch for each image
				foreach (ImageInfo imgInfo in imagesCellInfo.Images)
				{
					if (imgInfo.Img == null)
						break;
					
					var p = new PointF ((i + 1) * PicXPad + i * PicSize - 2, PicYPad - 2);
					
					// Cute touch
					UIColor.White.SetColor ();
					context.SaveState ();
					context.TranslateCTM (p.X, p.Y);
					context.SetLineWidth (1);
					
					// On device, the shadow is painted in the opposite direction!
					context.SetShadowWithColor (new SizeF (1, 1), 3, UIColor.DarkGray.CGColor);
					context.AddPath (badgePath);
					context.FillPath ();
					context.RestoreState ();
					
					i++;
				}				
			}
						
			public static float LayoutActivity(UIActivity activity, float width, List<Block> blocks)
			{
				string userText = activity.User.Name;
				SizeF userTextDim = SizeF.Empty;
				using (NSString nss = new NSString(userText))
				{
					userTextDim = nss.StringSize(userFont);
					
					var placement = new RectangleF (PicXPad + (PicSize - PicXPad) / 2, PicSize, userTextDim.Width, userSize);
					blocks.Add(new Block()
		            {
						Value = userText,
						Bounds = placement,
						Font = userFont,
						LineBreakMode = UILineBreakMode.WordWrap,
						TextColor = UIColor.Black,
					});
				}				
					
				float res = 0.0f;				
				
				return res;
			}

			public void UpdatedImage (long id, long userid, SizeDB sizeDB)
			{				
				if (imagesCellInfo == null)
					return;
				
				if (sizeDB == SizeDB.Size50)
				{
					int i = 0;
					foreach (ImageInfo imgInfo in imagesCellInfo.Images)
					{
						if (imgInfo.Img != null && imgInfo.Img.Id == id)
						{
							images[i] = ImageStore.GetLocalFullPicture (id, userid, SizeDB.Size50);
							if (images[i] != null)
								buttons[i].SetBackgroundImage(images[i], UIControlState.Normal);
							
							break;
						}
						i++;
					}
				}
				
				SetNeedsDisplay ();				
			}
		}
		
		// Create the UIViews that we will use here, layout happens in LayoutSubviews
		public ImagesCell (UITableViewCellStyle style, NSString ident, ImagesCellInfo imagesCellInfo)
			: base (style, ident)
		{
			SelectionStyle = UITableViewCellSelectionStyle.None;			
			
			tweetView = new ImagesCellView (imagesCellInfo);
			ContentView.Add (tweetView);
		}

		// 
		// This method is called when the cell is reused to reset
		// all of the cell values
		//
		public void UpdateCell (ImagesCellInfo imagesCellInfo)
		{
			tweetView.Update (imagesCellInfo);
			SetNeedsDisplay ();
		}

		public static float GetCellHeight (RectangleF bounds, UIActivity activity)
		{			 
			return 120;
		}

		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			
			tweetView.Frame = ContentView.Bounds;
			tweetView.SetNeedsDisplay ();
			
		}
	}
	
	public class ImagesCellInfo 
	{
		public List<ImageInfo> Images {get;set;}
		public int RowIndex {get;set;}
	}
}
