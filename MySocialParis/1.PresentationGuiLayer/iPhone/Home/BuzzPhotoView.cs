using System;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;
using MonoTouch.Dialog.Utilities;

namespace MSP.Client
{
	public class BuzzPhotoView : UIButton, ISizeImageUpdated, IImageUpdated
	{
		#region Fields
						
		public UIImage photoImage;
		private Image _Image;
		private RectangleF imgSize;
		private SizeF photoSize;		
		private static UIImage Value;
		
		#endregion
		
		#region Constructors
		
		[Export("layerClass")]
		public static Class LayerClass ()
		{
			return new Class (typeof(CATiledLayer));
		}

		public BuzzPhotoView (Image image, SizeF size) : base(new RectangleF (new PointF(0, 0), size))
		{
			photoSize = size;
			imgSize = new RectangleF (2 * 6, 2 * 6, 2 * (photoSize.Width - 2 * 6), 2 * (photoSize.Height- 2* 6));
			
			Value = Value ?? UIImageUtils.MakeEmpty(new Size(2 * ((int)photoSize.Width - 2 * 6), 2 * ((int)photoSize.Height) - 2 * 6));
						
			UIImage frame = Graphics.GetImgResource("cadre200");
			UIImage composite = GetCompImage(frame, Value);

			this.SetBackgroundImage (composite, UIControlState.Normal);
			
			Update(image);
		}
		
		#endregion
		
		/*
		public override void Draw (RectangleF rect)
		{
		}
		*/		

		protected override void Dispose (bool disposing)
		{
			if (disposing) {
				if (Value != null) {
					Value.Dispose ();
					Value = null;
				}
				if (photoImage != null)
				{
					photoImage.Dispose();
					photoImage = null;
				}
			}
			base.Dispose (disposing);
		}
		
		public void Update(Image image)
		{
			_Image = image;
			
			if (image != null)
			{
				var url = UrlStore.GetPicUrlFromId (image.Id, image.UserId, SizeDB.Size100);
				photoImage = ImageLoader.DefaultRequestImage(url, this);
			}
			
			if (photoImage != null)
			{
				RefreshImage(photoImage);			
			}
			else
			{
				RefreshImage(Value);
			}
		}
		
		private void RefreshImage(UIImage image)
		{
			if (image != null)
			{
				UIImage frame = Graphics.GetImgResource("cadre200");
				UIImage composite = GetCompImage(frame, image);			
				this.SetBackgroundImage (composite, UIControlState.Normal);
			
				SetNeedsDisplay();
			}
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
		
		#region IImageUpdated implementation		

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

		public void UpdatedImage (Uri uri)
		{
			if (_Image == null)
				return;
			
			var url = UrlStore.GetPicUrlFromId (_Image.Id, _Image.UserId, SizeDB.Size100);
			if (uri.Equals(url))
			{
				photoImage = ImageLoader.DefaultRequestImage(url, this);
				RefreshImage(photoImage);
			}
		}
		#endregion
	}
}
