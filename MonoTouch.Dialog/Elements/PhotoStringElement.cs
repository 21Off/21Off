
using System;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
namespace MonoTouch.Dialog
{
	public class PhotoStringElement : StringElement {
		static NSString skey = new NSString ("PhotoStringElement");
		// Apple leaks this one, so share across all.
		static UIImagePickerController picker;		
		UIImage image;
		public UITableViewCellAccessory Accessory { get; set; }
		
		public PhotoStringElement (string caption, UIImage image) : base (caption)
		{
			this.image = image;
			this.Accessory = UITableViewCellAccessory.None;
		}

		public PhotoStringElement (string caption, string value, UIImage image) : base (caption, value)
		{
			this.image = image;
			this.Accessory = UITableViewCellAccessory.None;
		}
		
		public PhotoStringElement (string caption,  NSAction tapped, UIImage image) : base (caption, tapped)
		{
			this.image = image;
			this.Accessory = UITableViewCellAccessory.None;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (skey);
			if (cell == null){
				cell = new UITableViewCell (Value == null ? UITableViewCellStyle.Default : UITableViewCellStyle.Subtitle, skey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			}
			
			cell.Accessory = Accessory;
			cell.TextLabel.Text = Caption;
			cell.TextLabel.TextAlignment = Alignment;
			
			cell.ImageView.Image = image;
			
			// The check is needed because the cell might have been recycled.
			if (cell.DetailTextLabel != null)
				cell.DetailTextLabel.Text = Value == null ? "" : Value;
			
			return cell;
		}
		
		class MyDelegate : UIImagePickerControllerDelegate {
			PhotoStringElement container;
			UITableView table;
			NSIndexPath path;
			
			public MyDelegate (PhotoStringElement container, UITableView table, NSIndexPath path)
			{
				this.container = container;
				this.table = table;
				this.path = path;
			}
			public override void Canceled (UIImagePickerController picker)
			{
				// TODO: Implement - see: http://go-mono.com/docs/index.aspx?link=T%3aMonoTouch.Foundation.ModelAttribute
				container.Cancelled();
				table.ReloadRows (new NSIndexPath [] { path }, UITableViewRowAnimation.None);				
			}
			public override void FinishedPickingImage (UIImagePickerController picker, UIImage image, NSDictionary editingInfo)
			{
				container.Picked (image);
				table.ReloadRows (new NSIndexPath [] { path }, UITableViewRowAnimation.None);
			}
		}
		
		UIViewController currentController;		
		void Picked (UIImage image)
		{
			this.image = image;
			//scaled = Scale (image);
			currentController.DismissModalViewControllerAnimated (true);
			
			if (OnExited != null)
				OnExited();
		}
		
		void Cancelled()
		{
			currentController.DismissModalViewControllerAnimated (true);
			
			if (OnExited != null)
				OnExited();
		}
		
		public event Action OnExited;
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			if (picker == null)
				picker = new UIImagePickerController ();
			
			picker.Delegate = new MyDelegate (this, tableView, path);
			
			dvc.ActivateController (picker);
			currentController = dvc;
		}		
	}
}
