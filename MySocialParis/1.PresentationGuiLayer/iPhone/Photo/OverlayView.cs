using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace MSP.Client
{
	public class OverlayView :  UIView
	{
		private VCViewController _ImagePicker;
		
		public OverlayView(VCViewController imagePicker, RectangleF frame) : base(frame)
		{		 				
			_ImagePicker = imagePicker;
						
			this.Opaque = false;
			this.BackgroundColor= UIColor.Clear;
			
			/*
			UIImage img = UIImage.FromFile("Images/carre.png");
			UIImageView imgView = new UIImageView(img);
			imgView.Frame = new RectangleF((320 - 305) / 2, (480 - 305) / 2, 305, 305);
			
			this.AddSubview(imgView);
			*/
			
			UIButton btn = UIButton.FromType(UIButtonType.Custom);
			btn.SetBackgroundImage(UIImage.FromFile("Images/Share/icone_photo.png"), UIControlState.Normal);
			btn.Frame = new RectangleF((320 - 100) / 2 , 480 - 40, 100, 40f);
			btn.TouchUpInside += HandleBtnTouchUpInside;
			
			this.AddSubview(btn);
			
			UIButton btnBib = UIButton.FromType(UIButtonType.Custom);
			btnBib.SetBackgroundImage(Graphics.GetImgResource("ok"), UIControlState.Normal);
			btnBib.Frame = new RectangleF(10, 480 - 26 - 7, 26, 26);
			this.AddSubview(btnBib);
			btnBib.TouchUpInside += HandleBtnBibTouchUpInside;
						
			UIButton backBtn = UIButton.FromType(UIButtonType.Custom);
			backBtn.Frame = new RectangleF(320 - 10 - 26, 480 - 26 - 7, 26, 26);
			backBtn.SetBackgroundImage(Graphics.GetImgResource("back"), UIControlState.Normal);
			backBtn.TouchUpInside += HandleBtnBackTouchUpInside;
			
			this.AddSubview(backBtn);
		}

		void HandleBtnBibTouchUpInside (object sender, EventArgs e)
		{
			_ImagePicker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
			_ImagePicker.WantsFullScreenLayout = true;
		}

		void HandleBtnTouchUpInside (object sender, EventArgs e)
		{
			//_ImagePicker.TakePicture();
		}

		void HandleBtnBackTouchUpInside (object sender, EventArgs e)
		{
			_ImagePicker.DismissModalViewControllerAnimated(true);	
		}
	}
}
