using System;
using System.Drawing;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public partial class MembersPhotoViewControler : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public MembersPhotoViewControler (IntPtr handle) : base(handle)
		{
			
		}

		[Export("initWithCoder:")]
		public MembersPhotoViewControler (NSCoder coder) : base(coder)
		{
			
		}

		public MembersPhotoViewControler (UINavigationController activityView, int userID, bool isModal) 
			: base("MembersPhotoViewControler", null)
		{
			_ActivityView = activityView;
			_UserID = userID;
			_IsModal = isModal;
		}
		
		#endregion
		
		private UINavigationController _ActivityView;
		private int _UserID;
		private bool _IsModal;

		void Initialize ()
		{		
			try
			{
				var rootElement = new RootElement ("timelineTitle") { UnevenRows = true };
				var section = new Section();
				rootElement.Add(section);				
				
				int ht = 80 + 10;
				var photoPanelRect = new RectangleF (0, 0, this.View.Frame.Width, ht);
				var membersPhotoPanel = new MembersPhotoPanelView (this, _IsModal, photoPanelRect, _UserID);			
				membersPhotoPanel.prevController = _ActivityView;
				membersPhotoPanel.MemberRoot = rootElement;							
				
				var mediaView = new MembersView(false, _UserID);
				mediaView.Root = rootElement;
				mediaView.View.Frame = new RectangleF(0, ht, UIScreen.MainScreen.Bounds.Width, this.View.Frame.Height - ht);
							
				this.View.AddSubview (membersPhotoPanel);
				this.View.AddSubview(mediaView.View);
				
				var view = new UIView(new RectangleF(0, _IsModal ? 90 : 80 , 320, 1));
				view.Layer.BackgroundColor = UIColor.LightGray.CGColor;
				this.View.AddSubview(view);				
			}
			catch (Exception ex)
			{
				Util.LogException("MembersPhotoViewController Initialize", ex);
			}
		}

		public override void ViewDidLoad ()
		{			
			base.ViewDidLoad ();
			
			Initialize ();
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			
			if (ViewAppearing != null)
				ViewAppearing(this, EventArgs.Empty);
		}
		
		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			
			if (ViewDisappearing != null)
				ViewDisappearing(this, EventArgs.Empty);
		}
		
		public event EventHandler ViewDisappearing;
		public event EventHandler ViewAppearing;
	}
}