using System;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public enum SwipeDirection
	{
		Left, Right
	}	
	
	public abstract partial class BaseTimelineViewController : DialogViewController
	{
		public event Action<SwipeDirection> OnGestSwipe;
		
		public static Selector MySelector
		{
			get 
			{
				return new Selector("HandleSwipe");
			}
		}

		/*
		public override void ViewDidLoad ()
		{			
			var swipe = new UISwipeGestureRecognizer();
			swipe.AddTarget(this, MySelector);
			swipe.Direction = UISwipeGestureRecognizerDirection.Left;
			swipe.Delegate = new SwipeRecognizerDelegate();
			
			// and last, add the recognizer to this view to take actions
			this.View.AddGestureRecognizer(swipe);			
		}
		*/
		
		/*
		[Export("HandleSwipe")]
		public void HandleSwipe(UISwipeGestureRecognizer recognizer)
		{
			// get the point of the swipe action
			PointF point = recognizer.LocationInView(this.View);
		 
			// TODO: do something with the swipe
			if (OnGestSwipe != null)
				OnGestSwipe(recognizer.Direction == UISwipeGestureRecognizerDirection.Left ? SwipeDirection.Left : SwipeDirection.Right);
		}
		*/		
		
		public class SwipeRecognizerDelegate : MonoTouch.UIKit.UIGestureRecognizerDelegate
		{
			public override bool ShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
			{
				return true;
			}
		}
	}
	
	public interface IGetHeightForRow
	{
		float GetHeightForRow (UITableView tableView, NSIndexPath indexPath);
	}
	
	public abstract partial class BaseTimelineViewController : DialogViewController, IGetHeightForRow
	{
		public BaseTimelineViewController (bool pushing) : base(null, pushing)
		{
			Autorotate = false;
			
			//
			// After the DialogViewController is created, but before it is displayed
			// Assign to the RefreshRequested event.   The event handler typically
			// will queue a network download, or compute something in some thread
			// when the update is complete, you must call "ReloadComplete" to put
			// the DialogViewController in the regular mode
			//			
			RefreshRequested += delegate { ReloadTimeline (); };
			Style = UITableViewStyle.Plain;
			
			TableView.BackgroundView = new UIImageView(Graphics.GetImgResource("fond"));
		}

		// Reloads data from the server
		public abstract void ReloadTimeline ();
		
		public virtual float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)	
		{
			return 120;
		}		


		public override Source CreateSizingSource (bool unevenRows)
		{
			// we are always uneven for TimelineViewControllers
			return new ScrollTrackingSizingSource (this);
		}

		class ScrollTrackingSizingSource : DialogViewController.SizingSource
		{
			BaseTimelineViewController parent;

			public ScrollTrackingSizingSource (DialogViewController dvc) : base(dvc)
			{
				parent = dvc as BaseTimelineViewController;
			}

			/*
			public override void Scrolled (UIScrollView scrollView)
			{
				try {
					var point = Container.TableView.ContentOffset;
					
				} catch (Exception e) {
					
				}
				
				base.Scrolled (scrollView);
			}
			*/
			
			public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return parent.GetHeightForRow(tableView, indexPath);
			}
		}
	}
}
