using System;
using System.Drawing;
using MonoTouch.CoreAnimation;
using MonoTouch.Dialog.Utilities;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;

namespace MSP.Client
{
	public class BuzzPhotoView : UIButton, ISizeImageUpdated, IImageUpdated
	{
		#region Fields
						
		public UIImage photoImage;
		private Image _Image;
		private RectangleF imgSize;
		private SizeF photoSize;		
		private static UIImage CompositeValue;
		private static UIImage AlbumFond;
		
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
			
			if (CompositeValue == null)
			{
				var emptyImg = UIImageUtils.MakeEmpty(new Size(2 * ((int)photoSize.Width - 2 * 6), 2 * ((int)photoSize.Height) - 2 * 6));									
				
				UIImage frame = Graphics.GetImgResource("cadre200");
				UIImage frameAlbum = Graphics.GetImgResource("cadre200_album");
				
				CompositeValue = GetCompImage(frame, emptyImg);				
				AlbumFond = GetCompImage(frameAlbum, emptyImg);
			}			
			
			this.SetBackgroundImage (IsAlbum(image) ? AlbumFond : CompositeValue, UIControlState.Normal);
			
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
				/*
				if (CompositeValue != null) {
					CompositeValue.Dispose ();
					CompositeValue = null;
				}
				*/
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
			
				RefreshImage(photoImage);
			}
			else
				RefreshImage(null);
		}
		
		private void RefreshImage(UIImage image)
		{
			bool isAlbum = IsAlbum(_Image);
			
			if (image != null)
			{				
				UIImage frame = Graphics.GetImgResource(isAlbum ? "cadre200_album" : "cadre200");
				UIImage composite = GetCompImage(frame, image);			
				this.SetBackgroundImage (composite, UIControlState.Normal);			
			}
			else
				this.SetBackgroundImage (isAlbum ? AlbumFond : CompositeValue, UIControlState.Normal);
			
			//this.Enabled = _Image != null || isAlbum;
			SetNeedsDisplay();
		}		
		
		private bool IsAlbum(Image image)
		{
			return image == null ? false : (image.IdAlbum > 0);
		}
		
		private UIImage GetCompImage(UIImage frame, UIImage image)
		{
			UIImage composite = UIImageUtils.overlayImageWithPolaroid
			(
				image, frame, imgSize,
				new RectangleF (0, 0, 2 * photoSize.Width, 2 * photoSize.Height),
				new SizeF (2 * photoSize.Width, 2 * photoSize.Height)
			);
			
			return composite;
		}
		
		#region IImageUpdated implementation		
		
		[Obsolete()]
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
