using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;
using MonoTouch.UIKit;

namespace MSP.Client
{
	//
	// A static class that will reuse the UIImagePickerController
	// as iPhoneOS has a crash if multiple UIImagePickerController are created
	//   http://stackoverflow.com/questions/487173
	// (Follow the links)
	//	
	public static class Camera
	{
		static UIImagePickerController picker;	
		static Action<UIImage> _callbackImg;
		static Action _oncancel;
		
		static void Init ()
		{
			if (picker != null)
				return;
			
			picker = new UIImagePickerController ();
			picker.Delegate = new CameraDelegate ();
		}
		
		class CameraDelegate : UIImagePickerControllerDelegate {
			public override void FinishedPickingImage (UIImagePickerController picker, UIImage image, NSDictionary editingInfo)
			{
				var cb = _callbackImg;
				_oncancel = null;
				_callbackImg = null;
				
				picker.DismissModalViewControllerAnimated(true);
				cb(image);
			}
			public override void Canceled (UIImagePickerController picker)
			{
				var cb = _oncancel;
				_oncancel = null;
				_callbackImg = null;
				
				picker.DismissModalViewControllerAnimated(true);
				cb();				
			}
		}
		
		public static void TakePicture (UIViewController parent, Action<UIImage> callback, Action onCancel)
		{
			Init ();
			picker.SourceType = UIImagePickerControllerSourceType.Camera;
			_callbackImg = callback;
			_oncancel = onCancel;
			if (parent != null)
				parent.PresentModalViewController (picker, true);
		}
		
		public static void SelectPicture (UIViewController parent, Action<UIImage> callback, Action onCancel)
		{
			Init ();
			picker.SourceType = UIImagePickerControllerSourceType.PhotoLibrary;
			_callbackImg = callback;
			_oncancel = onCancel;
			if (parent != null)
				parent.PresentModalViewController (picker, true);
		}
	}	
}
