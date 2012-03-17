using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using System;

namespace MSP.Client
{
	public class ImagesElement : OwnerDrawnElement
	{
		private List<ImageInfo> _images;
		private int cellIndex;
		
		public ImagesElement(List<ImageInfo> images, int rowIndex) : base(UITableViewCellStyle.Default, "ImagesElement")
		{
			_images = images;
			cellIndex = rowIndex;
		}
		
		public override void Draw (RectangleF bounds, CGContext context, UIView view)
		{
			//UIColor.White.SetFill ();
			//context.FillRect (bounds);
			try
			{
				UIView searchView = view.ViewWithTag(1);				
				if (searchView == null)
				{
					var photoCellView = new PhotoCellView(_images, cellIndex, null);										
					
					photoCellView.Tag = 1;
					view.Add(photoCellView);
				}
				else
				{
					var photoCellView = (PhotoCellView)searchView;					
					photoCellView.Update(_images, cellIndex);
					
					photoCellView.DrawBorder(UIColor.Green);
				}
			}
			catch (Exception ex)
			{
				Util.LogException("Draw ImagesElement", ex);
			}
		}
		
		public override float Height (RectangleF bounds)
		{			
			return 120.0f;
		}
	}
}
