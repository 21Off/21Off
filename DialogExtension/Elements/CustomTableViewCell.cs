using System;
using System.Drawing;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Linq;

namespace MonoTouch.Dialog
{
	public class CustomTableViewCell : UITableViewCell
	{
		public CustomTableViewCell(UITableViewCellStyle style, string reuseIdentifier) : base (style, reuseIdentifier)
		{
		}
		
		public override void LayoutSubviews ()
		{
			base.LayoutSubviews ();
			
			var imageViewFrame = this.ImageView.Frame;
			RectangleF detailTextFrame = TextLabel.Frame;
			
			float med = detailTextFrame.Left - imageViewFrame.Right;
			
			TextLabel.Frame = new RectangleF(new PointF(imageViewFrame.Left, 0), detailTextFrame.Size);
			ImageView.Frame =  new RectangleF(new PointF(detailTextFrame.Width + med, imageViewFrame.Top), imageViewFrame.Size);
		}	
	}
}
