using System;
using MonoTouch.Dialog;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;
using System.Drawing;
using MonoTouch.Foundation;

namespace MSP.Client
{
	/// <summary>
	/// This is an example of implementing the OwnerDrawnElement abstract class.
	/// It makes it very simple to create an element that you draw using CoreGraphics
	/// </summary>
	public class SampleOwnerDrawnElement : OwnerDrawnElement
	{
		CGGradient gradient;
		private UIFont subjectFont = UIFont.SystemFontOfSize(10.0f);
		private UIFont fromFont = UIFont.BoldSystemFontOfSize(14.0f);
		private UIFont dateFont = UIFont.BoldSystemFontOfSize(14.0f);
		

		public SampleOwnerDrawnElement (string text, DateTime sent, string from) : base(UITableViewCellStyle.Default, "sampleOwnerDrawnElement")
		{
			this.Subject = text;
			this.From = from;
			this.Sent = FormatDate(sent);
			CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB();
			gradient = new CGGradient(
			    colorSpace,
			    new float[] { 0.95f, 0.95f, 0.95f, 1, 
							  0.85f, 0.85f, 0.85f, 1},
				new float[] { 0, 1 } );
		}
		
		public string Subject
		{
			get; set; 
		}
		
		public string From
		{
			get; set; 
		}

		public string Sent
		{
			get; set; 
		}
		
		
		public string FormatDate (DateTime date)
		{
	
			if (DateTime.Today == date.Date) {
				return date.ToString ("hh:mm");
			} else if ((DateTime.Today - date.Date).TotalDays < 7) {
				return date.ToString ("ddd hh:mm");
			} else
			{
				return date.ToShortDateString ();			
			}
		}
		
		public override void Draw (RectangleF bounds, CGContext context, UIView view)
		{
			UIColor.White.SetFill ();
			context.FillRect (bounds);
			
			context.DrawLinearGradient (gradient, new PointF (bounds.Left, bounds.Top), new PointF (bounds.Left, bounds.Bottom), CGGradientDrawingOptions.DrawsAfterEndLocation);
			
			UIColor.Black.SetColor ();
			view.DrawString(this.From, new RectangleF(10, 5, bounds.Width/2, 10 ), fromFont, UILineBreakMode.TailTruncation);
			
			UIColor.Brown.SetColor ();
			view.DrawString(this.Sent, new RectangleF(bounds.Width/2, 5, (bounds.Width/2) - 10, 10 ), dateFont, UILineBreakMode.TailTruncation, UITextAlignment.Right);
			
			UIColor.DarkGray.SetColor();
			view.DrawString(this.Subject, new RectangleF(10, 30, bounds.Width - 20, TextHeight(bounds) ), subjectFont, UILineBreakMode.WordWrap);
		}
		
		public override float Height (RectangleF bounds)
		{
			var height = 40.0f + TextHeight (bounds);
			return height;
		}	
		
		private float TextHeight (RectangleF bounds)
		{
			SizeF size;
			using (var str = new NSString (this.Subject))
			{
				size = str.StringSize (subjectFont, new SizeF (bounds.Width - 20, 1000), UILineBreakMode.WordWrap);
			}			
			return size.Height;
		}
		
		public override string ToString ()
		{
			return string.Format (Subject);
		}
	}
}

