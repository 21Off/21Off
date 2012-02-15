using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using MSP.Client.DataContracts;
using System.Drawing;

namespace MSP.Client
{
	public partial class SearchByKeywordViewController : UIViewController
	{
		#region Constructors
		
		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code
		
		public SearchByKeywordViewController (IntPtr handle) : base (handle)
		{
			
		}
		
		[Export ("initWithCoder:")]
		public SearchByKeywordViewController (NSCoder coder) : base (coder)
		{
			
		}
		
		private bool isModal;
		private MSPNavigationController msp;
		private List<Image> images;
		private string keyword;
		
		public SearchByKeywordViewController (MSPNavigationController msp, List<Image> images, string keyword, bool isModal)
			: base ("SearchByKeywordViewController", null)
		{
			this.isModal = isModal;
			this.msp = msp;
			this.images = images;
			this.keyword = keyword;
		}
		
		#endregion
		
		public override void ViewDidLoad ()
		{
			backBtn.TouchUpInside += HandleBackBtnTouchUpInside;
			mapBtn.TouchDown += HandleMapBtnTouchDown;
			
			base.ViewDidLoad ();
			
			Initialize();
			
			UIViewExtensions.SetTitleText("", keyword, null, subTitle);
		}

		void HandleMapBtnTouchDown (object sender, EventArgs e)
		{
			var _MSP = AppDelegateIPhone.aroundNavigationController;
			var headerInfos = new HeaderInfos()
			{
				SubTitle = keyword,
				Title = "Keywords",
			};
			
			var b = new PhotoMapViewController(_MSP.VisibleViewController, images, headerInfos);
			b.View.Frame = UIScreen.MainScreen.Bounds;
			
			_MSP.VisibleViewController.PresentModalViewController(b, true);
		}

		void HandleBackBtnTouchUpInside (object sender, EventArgs e)
		{
			if (isModal)
				this.DismissModalViewControllerAnimated(true);
			else
				msp.PopViewControllerAnimated(true);
		}
		
		void Initialize ()
		{
			var root = new RootElement("search") {
				new Section() 
			};
			
			int rowCount = 4;
			for (int i = 0; i < images.Count; i += rowCount)
			{
				var imagesInfos = new List<ImageInfo>();
				for (int j = 0; j < rowCount; j++)
				{
					var imgInfo = new ImageInfo()
					{
						Img = (i + j < images.Count) ? images[i + j] : null,							
					};
					imagesInfos.Add(imgInfo);
				}
				root[0].Add(new Images2Element(imagesInfos, i));
			}
			
			var mediaView = new DialogViewController(root, false);
			mediaView.TableView.BackgroundView = new UIImageView(UIImage.FromBundle("Images/Ver4/fond"));	
			mediaView.View.Frame = new RectangleF(0, 45, 320, this.View.Frame.Height - 45);
			mediaView.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			mediaView.Style = UITableViewStyle.Plain;
								
			Add(mediaView.View);
			
			var view = new UIView(new RectangleF(0, 40 , 320, 1));
			view.Layer.BackgroundColor = UIColor.LightGray.CGColor;
			this.View.AddSubview(view);
		}
	}
}

