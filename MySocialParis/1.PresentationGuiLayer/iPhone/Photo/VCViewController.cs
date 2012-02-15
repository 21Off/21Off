using System;
using System.Drawing;
using MonoTouch.UIKit;

namespace MSP.Client
{
    public class VCViewController : UIImagePickerController
    {
		public UIButton btnBib;
		public bool IsCameraAvailable;
		private static UIImage libraryPhoto;
		
        #region Constructor
		
        public VCViewController()
            : base()
        {
            initialize();
        }
				
        void initialize()
        {
			WantsFullScreenLayout = true;			
			NavigationBarHidden = true;
						
            if (IsSourceTypeAvailable(UIImagePickerControllerSourceType.Camera))
			{		
				IsCameraAvailable = true;
                SourceType = UIImagePickerControllerSourceType.Camera;
	            ShowsCameraControls = true;
				
				//CameraViewTransform = CGAffineTransform.MakeScale(1f, 400.0f / 320.0f);
				//var rectangleF = new RectangleF(0, 0, 320, this.View.Frame.Height);
				//CameraOverlayView = new OverlayView(this, rectangleF);
				
				//120x62
				if (libraryPhoto == null)
					libraryPhoto = Graphics.GetImgResource("album");							
				
				btnBib = UIButton.FromType(UIButtonType.RoundedRect);
				btnBib.SetImage(libraryPhoto, UIControlState.Normal);
				
				btnBib.Frame = new RectangleF((320 - libraryPhoto.CGImage.Width / 2) / 2, 21 / 2, 
                      libraryPhoto.CGImage.Width / 2, libraryPhoto.CGImage.Height / 2);
				
				btnBib.Opaque = false;
				btnBib.Alpha = 0.5f;
				
				btnBib.TouchUpInside += HandleBtnBibTouchUpInside;
				//Add(btnBib);
				
				this.CameraOverlayView.AddSubview(btnBib);
			}
            else
                SourceType = UIImagePickerControllerSourceType.PhotoLibrary;

			AllowsEditing = true;
			Title  = "share";			
        }

        void HandleBtnBibTouchUpInside (object sender, EventArgs e)
        {
			btnBib.Hidden = true;
			SourceType = UIImagePickerControllerSourceType.PhotoLibrary;			
        }
		
        #endregion
    }
}
