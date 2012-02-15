using System;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.IO;

namespace MonoTouch.Dialog
{			
	public class PhotoElement : Element {
		public UIImage Value;
		static RectangleF rect = new RectangleF (0, 0, dimx, dimy);
		static NSString ikey = new NSString ("PhotoElement");
		UIImage scaled;
		UIPopoverController popover;
		
		// Apple leaks this one, so share across all.
		static UIImagePickerController picker;
		
		// Height for rows
		const int dimx = 48;
		const int dimy = 43;
		
		// radius for rounding
		const int rad = 10;
		
		static UIImage MakeEmpty ()
		{
			using (var cs = CGColorSpace.CreateDeviceRGB ()){
				using (var bit = new CGBitmapContext (IntPtr.Zero, dimx, dimy, 8, 0, cs, CGImageAlphaInfo.PremultipliedFirst)){
					bit.SetStrokeColor (1, 0, 0, 0.5f);
					bit.FillRect (new RectangleF (0, 0, dimx, dimy));
					
					return UIImage.FromImage (bit.ToImage ());
				}
			}
		}
		
		UIImage Scale (UIImage source)
		{
			return source.Scale(new SizeF (dimx, dimy));
		}
		
		public PhotoElement (UIImage image) : base ("")
		{
			if (image == null){
				Value = MakeEmpty ();
				scaled = Value;
			} else {
				Value = image;			
				scaled = Scale (Value);
			}
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (ikey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, ikey);				
			}
			
			if (scaled == null)
				return cell;
			
			Section psection = Parent as Section;
			bool roundTop = psection.Elements [0] == this;
			bool roundBottom = psection.Elements [psection.Elements.Count-1] == this;
			
			using (var cs = CGColorSpace.CreateDeviceRGB ()){
				using (var bit = new CGBitmapContext (IntPtr.Zero, dimx, dimy, 8, 0, cs, CGImageAlphaInfo.PremultipliedFirst)){
					// Clipping path for the image, different on top, middle and bottom.
					if (roundBottom){
						bit.AddArc (rad, rad, rad, (float) Math.PI, (float) (3*Math.PI/2), false);
					} else {
						bit.MoveTo (0, rad);
						bit.AddLineToPoint (0, 0);
					}
					bit.AddLineToPoint (dimx, 0);
					bit.AddLineToPoint (dimx, dimy);
					
					if (roundTop){
						bit.AddArc (rad, dimy-rad, rad, (float) (Math.PI/2), (float) Math.PI, false);
						bit.AddLineToPoint (0, rad);
					} else {
						bit.AddLineToPoint (0, dimy);
					}
					bit.Clip ();
					bit.DrawImage (rect, scaled.CGImage);
															
					cell.ImageView.Image = UIImage.FromImage (bit.ToImage ());
					cell.TextLabel.Text = "image profile";
				}
			}
			return cell;
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				if (scaled != null){
					scaled.Dispose ();
					Value.Dispose ();
					scaled = null;
					Value = null;
				}
			}
			base.Dispose (disposing);
		}

		class MyDelegate : UIImagePickerControllerDelegate {
			PhotoElement container;
			UITableView table;
			NSIndexPath path;
			
			public MyDelegate (PhotoElement container, UITableView table, NSIndexPath path)
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
		
		void Picked (UIImage image)
		{
			Value = image;
			
			var err = new NSError();
			
			/*
			string BaseDir = Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), "..");
			string tmpDir = Path.Combine (BaseDir, "Tmp/");			
			var jpgstr = Path.Combine(tmpDir, string.Format("{0}.jpg", "profile"));
			image.AsJPEG((float)0.7).Save(jpgstr, true, out err);			
			*/
			
			scaled = Scale (image);
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
		public event Action OnEntered;
		
		UIViewController currentController;
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{				
			if (picker == null)
				picker = new UIImagePickerController ();
			picker.Delegate = new MyDelegate (this, tableView, path);
			
			switch (UIDevice.CurrentDevice.UserInterfaceIdiom){
			case UIUserInterfaceIdiom.Pad:
				RectangleF useRect;
				popover = new UIPopoverController (picker);
				var cell = tableView.CellAt (path);
				if (cell == null)
					useRect = rect;
				else
					rect = cell.Frame;
				popover.PresentFromRect (rect, dvc.View, UIPopoverArrowDirection.Any, true);
				break;
				
			default:
			case UIUserInterfaceIdiom.Phone:
				dvc.ActivateController (picker);
				break;
			}
			currentController = dvc;
			
			if (OnEntered != null)
				OnEntered();
		}
	}	
}
