using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public class PhotoCellView : UIView
	{
		int _RowIndex= -1;

		List<BuzzPhoto> photos;

		public PhotoCellView (List<ImageInfo> fileNames, int rowIndex, Action<BuzzPhoto> onPhotoClicked) : base()
		{				
			_RowIndex = rowIndex;
			photos = new List<BuzzPhoto> ();
			
			int i = 0;
			foreach (ImageInfo imageInfo in fileNames) {
				i++;
					
				Image image = imageInfo.Img;
				string title = image == null ? "" : (image.Name ?? "No comment");
				var buzzPhoto = new BuzzPhoto (image, title, new SizeF (100, 120), onPhotoClicked);
				
				buzzPhoto.Frame = new RectangleF ((i - 1) * (100 + 5) + 5, 5, 100, 120);
				photos.Add(buzzPhoto);
				
				this.AddSubview (buzzPhoto);
			}
			
			Opaque = false;
			BackgroundColor = UIColor.FromRGBA (0, 0, 0, 0);
			BackgroundColor = UIColor.FromPatternImage (Graphics.GetImgResource ("fond"));
		}

		public void Update (List<ImageInfo> fileNames, int rowIndex)
		{
			try
			{
				bool changed = false;
				if (fileNames.Count != photos.Count)
					changed = true;
				
				for (int i = 0; i < photos.Count; i++)
				{
					BuzzPhoto buzzPhoto = photos[i];//	drawed
					var imageInfo = fileNames[i];//	to be drawn
					
					if (imageInfo.Img == null && buzzPhoto.Photo != null)
						changed = true;
					
					if (imageInfo.Img != null && buzzPhoto.Photo == null)
						changed = true;
					
					if (imageInfo.Img != null && buzzPhoto.Photo != null)
					{
						changed = imageInfo.Img.Id != buzzPhoto.Photo.Id;
					}
				}
				
				if (_RowIndex != rowIndex || changed)
				{	
					_RowIndex = rowIndex;
					for (int i = 0; i < photos.Count; i++)
					{					
						var imageInfo = fileNames[i];
						string title = imageInfo.Img == null ? "" : (imageInfo.Img.Name ?? "No comment");					
						
						BuzzPhoto buzzPhoto = photos[i];
						buzzPhoto.Update(title, imageInfo.Img);
					}				
				}
			}
			catch (Exception ex)
			{
				Util.LogException("PhotoCellView Update", ex);
			}
		}
	}
}
