using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.CoreFoundation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	public class CustomImageStringElement : ImageStringElement
	{
		private UIImage _image;
		
		public CustomImageStringElement (string caption, NSAction tapped, UIImage image) : base (caption, tapped, image)
		{
			_image = image;
		}		
		
		public override UITableViewCell GetCell (UITableView tv)
		{			
			var uITableViewCell = (CustomTableViewCell)tv.DequeueReusableCell (this.CellKey);
			if (uITableViewCell == null)
			{
				uITableViewCell = new CustomTableViewCell ((this.Value != null) ? UITableViewCellStyle.Subtitle : UITableViewCellStyle.Default, this.CellKey);
				uITableViewCell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			}
			uITableViewCell.Accessory = this.Accessory;
			uITableViewCell.TextLabel.Text = this.Caption;
			uITableViewCell.TextLabel.TextAlignment = this.Alignment;
			uITableViewCell.ImageView.Image = this._image;
			if (uITableViewCell.DetailTextLabel != null)
			{
				uITableViewCell.DetailTextLabel.Text = ((this.Value != null) ? this.Value : string.Empty);
			}
			return uITableViewCell;
		}
	}	
	
}
