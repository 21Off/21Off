using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public class PhotoCellView : UIView
	{
		private List<BuzzPhoto> photos;

		public PhotoCellView (List<ImageInfo> fileNames, int rowIndex, Action<BuzzPhoto> onPhotoClicked) : base()
		{
			photos = new List<BuzzPhoto> ();
			
			int space = 5;
			int width = (320 - (fileNames.Count + 1) * space) / fileNames.Count;
			
			int i = 0;
			foreach (ImageInfo imageInfo in fileNames) {
				i++;
					
				Image image = imageInfo.Img;
				string title = image == null ? "" : (image.Name ?? "No comment");
				var buzzPhoto = new BuzzPhoto (image, title, new SizeF (width, 120), onPhotoClicked);
				
				buzzPhoto.Frame = new RectangleF ((i - 1) * (width + space) + space, 5, width, 120);
				photos.Add(buzzPhoto);
				
				this.AddSubview (buzzPhoto);
			}
			
			Opaque = false;
			//BackgroundColor = UIColor.Red;
			//BackgroundColor = UIColor.FromPatternImage (Graphics.GetImgResource ("fond"));
		}

		public void Update (List<ImageInfo> fileNames, int rowIndex)
		{
			try
			{
				for (int i = 0; i < photos.Count; i++)
				{					
					var imageInfo = fileNames[i];
					string title = imageInfo.Img == null ? "" : (imageInfo.Img.Name ?? "No comment");					
					
					BuzzPhoto buzzPhoto = photos[i];
					buzzPhoto.Update(title, imageInfo.Img);
				}
			}
			catch (Exception ex)
			{
				Util.LogException("PhotoCellView Update", ex);
			}
		}
	}
}
