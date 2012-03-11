using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreLocation;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	class pickerDelegate : UIImagePickerControllerDelegate
	{
		private VCViewController _navigationController;
		private UINavigationController _shareNavCont;
		private Image prevImage;
		
		public pickerDelegate(VCViewController msp, UINavigationController shareNavCont) : base()
		{
			_navigationController = msp;
			_shareNavCont = shareNavCont;
		}
		
		public pickerDelegate(VCViewController msp, UINavigationController shareNavCont, Image prevImage)
			:this(msp, shareNavCont)
		{
			this.prevImage = prevImage;
		}
		
		public override void Canceled (UIImagePickerController picker)
		{	
			UIApplication.SharedApplication.SetStatusBarHidden(false, false);
			
			var imagePicker = (VCViewController)_navigationController;
			if (imagePicker.IsCameraAvailable && imagePicker.SourceType == UIImagePickerControllerSourceType.PhotoLibrary)
			{
				imagePicker.SourceType = UIImagePickerControllerSourceType.Camera;
				imagePicker.btnBib.Hidden = false;
				return;
			}
			
			AppDelegateIPhone.tabBarController.DismissModalViewControllerAnimated(true);			
			
			var aaa = _shareNavCont.TabBarController;
			aaa.SelectedViewController = aaa.ViewControllers[0];
			
			var rotatingTb = (RotatingTabBar)AppDelegateIPhone.tabBarController;
			rotatingTb.SelectTab(0);
		}		
		
		public override void FinishedPickingImage (UIImagePickerController picker, UIImage image, NSDictionary editingInfo)
		{	
			UIApplication.SharedApplication.SetStatusBarHidden(false, false);
			
			var imagePicker = (VCViewController)_navigationController;
			if (imagePicker.IsCameraAvailable)
				imagePicker.btnBib.Hidden = true;
						
			imagePicker.DismissModalViewControllerAnimated(true);
			
			if (imagePicker.IsCameraAvailable)
			{
				image.SaveToPhotosAlbum (delegate {
					// ignore errors
					});
			}
			
			if (prevImage != null)
			{
				var photoLocation = new CLLocation(prevImage.Latitude, prevImage.Longitude);				
				var photoPost = new PhotoPostViewController(_shareNavCont, image, photoLocation, null);
				_shareNavCont.PushViewController(photoPost, true);			
			}
			else
			{
				var photoLocation = new PhotoLocationViewController(_shareNavCont, image);
				_shareNavCont.PushViewController(photoLocation, true);
			}
		}	
	}
	
}