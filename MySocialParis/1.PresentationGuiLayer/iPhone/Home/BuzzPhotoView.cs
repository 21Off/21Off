using System;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;

namespace MSP.Client
{

	public class BuzzPhotoView : UIButton, IImageUpdated
	{
		[Export("layerClass")]
		public static Class LayerClass ()
		{
			return new Class (typeof(CATiledLayer));
		}

		public BuzzPhotoView (Image image, SizeF size) : base(new RectangleF (new PointF(0, 0), size))
		{
/*			Layer.MasksToBounds = true;
			Layer.CornerRadius = 8;
			Layer.Opaque = true;
			Layer.ContentsScale = UIScreen.MainScreen.Scale;
			
			Layer.BorderColor = UIColor.White.CGColor;
			Layer.BorderWidth = 2F;
			//Layer.BackgroundColor = UIColor.FromRGBA(1.0f, 1.0f, 1.0f, 1.0F).CGColor;
			Layer.ShadowColor = UIColor.Black.CGColor;
			Layer.ShadowOpacity = 0.5f;
			Layer.ShadowRadius = 0.5f;
			Layer.ShadowOffset = new SizeF(-1.0f, 1.0f);
						 */
			//Graphics.ConfigLayerHighRes(Layer);
			
			photoSize = size;
			imgSize = new RectangleF (2 * 6, 2 * 6, 2 * (photoSize.Width - 2 * 6), 2 * (photoSize.Height- 2* 6));
			
			Value = Value ?? UIImageUtils.MakeEmpty(new Size(2 * ((int)photoSize.Width - 2 * 6), 2 * ((int)photoSize.Height) - 2 * 6));
						
			UIImage frame = Graphics.GetImgResource("cadre200");
			UIImage composite = GetCompImage(frame, Value);

			this.SetBackgroundImage (composite, UIControlState.Normal);
			_Image = image;
		}

		public override void Draw (RectangleF rect)
		{			
			//base.Draw (rect);
			if (!picLoaded) {
				Update(_Image);
				picLoaded = true;
			}
		}
				
		private Image _Image;
		private RectangleF imgSize;
		private SizeF photoSize;		
		public static UIImage Value;
		public UIImage photoImage;
		
		private bool picLoaded = false;
		
		public void Update(Image image)
		{
			if (_Image != image || !picLoaded)
			{
				_Image = image;
				
				photoImage = null;
				
				if (image == null)
				{
					InvokeOnMainThread (() =>
					{
						RefreshImage(Value);
					});
				}
				else
				{
					photoImage = ImageStore.RequestFullPicture(image.Id, image.UserId, SizeDB.Size100, this);					
					if (photoImage != null)
					{
						InvokeOnMainThread (() => { RefreshImage(photoImage); });
					}
					else
					{
						//Occurs when a new photo appers in the list
						InvokeOnMainThread (() => { RefreshImage(Value); });
					}
				}
			}
			else
			{
				if (image == null)
				{
					InvokeOnMainThread (() => { RefreshImage(Value); });
				}
				else
				{
					if (photoImage != null)
					{
						InvokeOnMainThread (() => { RefreshImage(photoImage); });
					}
				}
			}
		}
		
		private void RefreshImage(UIImage image)
		{
			if (image != null)
			{
				//UIView.BeginAnimations("imageThumbnailTransitionIn");
			    //UIView.SetAnimationDuration(0.5f);
//				this.SetBackgroundImage (image, UIControlState.Normal);			
			    //UIView.CommitAnimations();
		
				UIImage frame = Graphics.GetImgResource("cadre200");
				UIImage composite = GetCompImage(frame, image);			
				this.SetBackgroundImage (composite, UIControlState.Normal);	
			}
			
			SetNeedsDisplay();
		}
		
		private UIImage GetCompImage(UIImage frame, UIImage image)
		{
			UIImage composite = UIImageUtils.overlayImageWithPolaroid
			(
				image,
				frame,
				imgSize,
				new RectangleF (0, 0, 2 * photoSize.Width, 2 * photoSize.Height),
				new SizeF (2 * photoSize.Width, 2 * photoSize.Height)
			);
			
			return composite;
		}

		public void UpdatedImage (long id, long userId, SizeDB sizeDB)
		{		
			if (_Image == null || _Image.Id != id)
				return;

			try 
			{
				UIImage resImg = ImageStore.GetLocalFullPicture(id, userId, SizeDB.Size100);
				
				if (resImg != null)
				{
					photoImage = resImg;
					RefreshImage(resImg);
				}
				
				SetNeedsDisplay ();
			}
			catch (Exception ex)
			{
				Util.LogException("UpdatedImage", ex);
			}
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (Value != null) {
					Value.Dispose ();
					Value = null;
				}
			}
			base.Dispose (disposing);
		}
	}
}
