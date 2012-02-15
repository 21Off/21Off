using System.Drawing;
using MonoTouch.UIKit;
using MSP.Client;

namespace MSP.Client
{
	public class RotatingTabBar : UITabBarController
	{
		UIView indicator;
		int selected;

		public RotatingTabBar () : base()
		{
		}

		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return false;
			
	//		return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}


		void UpdatePosition (bool animate)
		{
			var vc = ViewControllers;
			var w = View.Bounds.Width / vc.Length;
			var x = w * selected;
			
			if (animate) {
				UIView.BeginAnimations (null);
				UIView.SetAnimationCurve (UIViewAnimationCurve.EaseInOut);
			}
			
			indicator.Frame = new RectangleF (x + ((w - 10) / 2), View.Bounds.Height - TabBar.Bounds.Height - 4, 10, 6);
			indicator.Alpha = 1.0f;
			
			if (animate)
				UIView.CommitAnimations ();
		}

		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
			
			UpdatePosition (false);
		}

		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			
			if (indicator == null) {
				indicator = new TriangleView (UIColor.FromRGB (0.0f, 0.0f, 0.0f), UIColor.Black);
				View.AddSubview (indicator);
				ViewControllerSelected += OnSelected;
				UpdatePosition (false);
			}
		}
		
		public void SelectTab(int index)
		{
			selected = index;
			UpdatePosition (false);
		}

		public void OnSelected (object sender, UITabBarSelectionEventArgs a)
		{
			var vc = ViewControllers;
			
			for (int i = 0; i < vc.Length; i++) {
				if (vc[i] == a.ViewController) {
					selected = i;
					UpdatePosition (true);
					return;
				}
			}
		}
		
		
		public override void ViewDidLoad ()
		{
/*
			 * base.ViewDidLoad ();
			this.TabBar.BackgroundColor = UIColor.White;
						
			var v = new UIView(new RectangleF(0, 0, 480, 49));
			var i = UIImage.FromFile("Images/Ver4/bkg-white.png");
			var c = UIColor.FromPatternImage(i);
			v.BackgroundColor = c;
			this.TabBar.InsertSubview(v, 0);
*/			
/*
  Can't access to cocoa API
*/			 
/*			for(int index1=0; index1< this.TabBar.Subviews.Length; index1++)
    		{
				if (this.TabBar.Subviews[index1].GetType() == typeof(UITabBarButton))
				{
					for(int index2=0; index2< this.TabBar.Subviews[index1].Subviews.Length; index2++)
					{
						if (this.TabBar.Subviews[index1].Subviews[index2].GetType() == typeof(UITabBarButtonLabel))
						{
							var button = UITabBarButtonLabel(this.TabBar.Subviews[index1].Subviews[index2]);
							
						}
					}
				}
		    }

*/
		}
	}
}