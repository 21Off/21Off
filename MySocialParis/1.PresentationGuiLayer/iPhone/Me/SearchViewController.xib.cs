using System;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;
using System.Collections.Generic;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public partial class SearchViewController : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public SearchViewController (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public SearchViewController (NSCoder coder) : base(coder)
		{

		}
		
		private UINavigationController _MSP;
		public SearchViewController (UINavigationController msp) : base("SearchViewController", null)
		{
			_MSP = msp;
		}
		
		private SearchUser SearchByUser;
		private SearchKeyword SearchByKeyword;
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			var dv = new SearchUser();
			dv.View.Frame = new RectangleF(0, 41, 320, 480 - 41);
			dv.TableView.BackgroundView = new UIImageView(Graphics.GetImgResource("fond"));
			SearchByUser = dv;
						
			var dv2 = new SearchKeyword();
			dv2.View.Frame = new RectangleF(0, 41, 320, 480 - 41);
			dv2.TableView.BackgroundView = new UIImageView(Graphics.GetImgResource("fond"));
			SearchByKeyword = dv2;
			
			usersBtn.SetBackgroundImage(Graphics.GetImgResource("usersbuttonD"), UIControlState.Selected);
			usersBtn.SetBackgroundImage(Graphics.GetImgResource("usersbuttonD"), UIControlState.Highlighted);
			usersBtn.Selected = false;
			
			keywordsBtn.SetBackgroundImage(Graphics.GetImgResource("keywordsbuttonD"), UIControlState.Selected);			
			keywordsBtn.SetBackgroundImage(Graphics.GetImgResource("keywordsbuttonD"), UIControlState.Highlighted);			
			keywordsBtn.Selected = true;
			Flip2(0);
			
			mapBtn.TouchDown += HandleMapBtnTouchDown;
			usersBtn.TouchDown += (sender, e) => Flip2(1);
			keywordsBtn.TouchDown += (sender, e) => Flip2(0);
			
			/*
			segmentedControl.SelectedSegment = 1;
			segmentedControl.ValueChanged += delegate {
					
				Flip2(segmentedControl.SelectedSegment);
			};
			*/
			
			this.View.AddSubview(SearchByKeyword.View);
			this.View.AddSubview(SearchByUser.View);
			SearchByKeyword.View.Hidden = true;
			
			var view = new UIView(new RectangleF(0, 40 - 2, 320, 1));
			view.Layer.BackgroundColor = UIColor.LightGray.CGColor;
			this.View.AddSubview(view);			
			
			Initialize();
		}

		void HandleMapBtnTouchDown (object sender, EventArgs e)
		{
			List<Image> images = SearchByKeyword.FoundImages;						
						
			var hInfos = new HeaderInfos()
			{ 
				SubTitle = SearchByKeyword.SearchedText, 
				Title = "keywords" 
			};
			
			var b = new PhotoMapViewController(this, images, hInfos);
			b.View.Frame = UIScreen.MainScreen.Bounds;
			
			this.PresentModalViewController(b, true);
		}
		
		private void Flip2(int i)
		{
			usersBtn.SetBackgroundImage(i == 1
				? Graphics.GetImgResource("usersbuttonD")
				: Graphics.GetImgResource("usersbuttonU"), UIControlState.Normal);
			
			keywordsBtn.SetBackgroundImage(i == 0 
				? Graphics.GetImgResource("keywordsbuttonD")
				: Graphics.GetImgResource("keywordsbuttonU"), UIControlState.Normal);
			
			View.SetNeedsDisplay();
			
			SearchByKeyword.View.Hidden = i != 0;
			SearchByUser.View.Hidden = i != 1;
			
			mapBtn.Hidden = i == 1;
		}
		
		private void Flip(int i)
		{
			usersBtn.SetBackgroundImage(i == 1
				? Graphics.GetImgResource("usersbuttonD")
				: Graphics.GetImgResource("usersbuttonU"), UIControlState.Normal);
			
			keywordsBtn.SetBackgroundImage(i == 0 
				? Graphics.GetImgResource("keywordsbuttonD")
				: Graphics.GetImgResource("keywordsbuttonU"), UIControlState.Normal);
			
			UIView.BeginAnimations("Flipper");
			UIView.SetAnimationDuration(1.25);
			UIView.SetAnimationCurve(UIViewAnimationCurve.EaseInOut);
			if (i == 0)
			{
			   Console.WriteLine("to map");
			   UIView.SetAnimationTransition 
			        (UIViewAnimationTransition.FlipFromRight, this.View, true);
			   SearchByKeyword.ViewWillAppear(true);
			   SearchByUser.ViewWillDisappear(true);
			 
			   SearchByKeyword.View.RemoveFromSuperview();
			   this.View.AddSubview(SearchByUser.View);
			
			   SearchByUser.ViewDidDisappear(true);
			   SearchByKeyword.ViewDidAppear(true);
			}
			else
			{
			   Console.WriteLine("to list");
			   UIView.SetAnimationTransition 
			         (UIViewAnimationTransition.FlipFromLeft, this.View, true);
			   SearchByUser.ViewWillAppear(true);
			   SearchByKeyword.ViewWillDisappear(true);
			 
			   SearchByUser.View.RemoveFromSuperview();
			   this.View.AddSubview(SearchByKeyword.View);
			 
			   SearchByKeyword.ViewDidDisappear(true);
			   SearchByUser.ViewDidAppear(true);
			}
			UIView.CommitAnimations();			
		}
		
		void Initialize ()
		{
			backBtn.SetImage(Graphics.GetImgResource("back"), UIControlState.Normal);			
			this.backBtn.TouchDown += HandleBackBtnhandleTouchDown;
		}

		void HandleBackBtnhandleTouchDown (object sender, EventArgs e)
		{
			_MSP.PopViewControllerAnimated(false);
			//_MSP.DismissModalViewControllerAnimated(true);
		}
		
		#endregion
	}
}

