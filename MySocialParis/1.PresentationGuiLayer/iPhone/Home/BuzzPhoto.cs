using System;
using System.Drawing;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public class BuzzPhoto : UIView
	{
		private BuzzPhotoView buzz;
		private UILabel text;
		public Image Photo {get;set;}
		
		public BuzzPhoto (Image image, string title, SizeF size, Action<BuzzPhoto> onPhotoClicked)
		{
			Photo = image;
						
			buzz = new BuzzPhotoView (image, new SizeF (100, 100));
			buzz.TouchUpInside += delegate(object sender, EventArgs e) {
				if (onPhotoClicked != null)
					onPhotoClicked(this);
			};			
			
			text = new UILabel (new RectangleF (6, 103, size.Width, 13))
			{ 
				Text = title,
				TextColor = UIColor.FromRGBA(0.5f,0.5f,0.5f,1.0f), 
				Font = UIFont.FromName ("HelveticaNeue-Bold", 12),
				BackgroundColor = UIColor.FromHSBA (0, 0, 0, 0),
			};
			
			this.AddSubview (buzz);
			this.AddSubview (text);
		}
		
		public void Update (string title, Image photo)
		{
			Photo = photo;
			
			text.Text = title;			
			buzz.Update(photo);
		}	
	}
}

