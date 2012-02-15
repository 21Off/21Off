using System;
using System.Drawing;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public partial class PhotoDetailsViewController : UIViewController
	{
		private FullUserResponse _ImageOwner;
		private Image _Image;
		private UINavigationController _MSP;
		private bool _IsModal;
		private Action backAction;
		
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public PhotoDetailsViewController (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public PhotoDetailsViewController (NSCoder coder) : base(coder)
		{
		}
		
		public PhotoDetailsViewController (UINavigationController msp, FullUserResponse owner, Image image, 
			bool isModal, Action backAction)
		{
			this.backAction = backAction;
			_MSP = msp;
			_Image = image;
			_ImageOwner = owner;
			_IsModal = isModal;
			
			Initialize();
		}
		
		public PhotoDetailsViewController (UINavigationController msp, FullUserResponse owner, Image image, bool isModal)
			: base("PhotoDetailsViewController", null)
		{
			_MSP = msp;
			_Image = image;
			_ImageOwner = owner;
			_IsModal = isModal;
			
			Initialize();
		}
		
		#endregion	
		
		void Initialize ()
		{
			var membersPhotoPanel = new PhotoDetailsPanelView (new RectangleF (0, 1, this.View.Frame.Width, 44), 
			                                                   _ImageOwner, _Image, _IsModal, backAction);
			membersPhotoPanel.prevController = _MSP;
			membersPhotoPanel.PictureTapped += GotoMemberPhotosArea;
			
			var rootElement = new RootElement ("timelineTitle") { UnevenRows = true };
			var section = new Section();
			rootElement.Add(section);
			
			var mediaView = new PhotoDetailsView(true, _Image, _ImageOwner.User);
			mediaView.Root = rootElement;
			mediaView.View.Frame = new RectangleF(0, 45, 320, this.View.Frame.Height - 45);
						
			this.View.AddSubview (membersPhotoPanel);
			this.View.AddSubview(mediaView.View);			
		}
		
		public void GotoMemberPhotosArea()
		{
			Action act = ()=>
			{
				InvokeOnMainThread(()=>
                {
					
					var m = new MembersPhotoViewControler(_MSP, _ImageOwner.User.Id, _IsModal);
					if (_IsModal)				
						this.PresentModalViewController(m, true);
					else
						_MSP.PushViewController(m, true);
				});
				
			};
			AppDelegateIPhone.ShowRealLoading(View, "Loading photos", null, act);
		}		
	}
}

