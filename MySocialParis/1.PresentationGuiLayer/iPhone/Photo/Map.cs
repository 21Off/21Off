using System;
using System.Drawing;
using System.IO;
using MonoTouch.CoreLocation;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.MapKit;
using MonoTouch.ObjCRuntime;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public class Map : MKMapView
	{						
		public Map (IntPtr handle) : base(handle)
		{
			
		}

		[Export("initWithCoder:")]
		public Map (NSCoder coder) : base(coder)
		{
			
		}
		
		public Map()
		{
			InitGestureRecog();
		}
		
		public static Selector MySelector
		{
			get 
			{
				return new Selector("HandleSwipe");
			}
		}
		
		public void InitGestureRecog ()
		{								
			var gestureRecognizer = new UITapGestureRecognizer();
			gestureRecognizer.CancelsTouchesInView = false;
			gestureRecognizer.AddTarget(this, MySelector);
			
			gestureRecognizer.Delegate = new SwipeRecognizerDelegate();
			
			// and last, add the recognizer to this view to take actions
			this.AddGestureRecognizer(gestureRecognizer);			
		}		
		
		public event Action<PointF> OnTapped;
		
		[Export("HandleSwipe")]
		public void HandleSwipe(UITapGestureRecognizer recognizer)
		{
			// get the point of the swipe action
			PointF point = recognizer.LocationInView(this);
		 
			// TODO: do something with the swipe
			if (OnTapped != null)
				OnTapped(point);
		}
		
		public class SwipeRecognizerDelegate : MonoTouch.UIKit.UIGestureRecognizerDelegate
		{
			public override bool ShouldReceiveTouch(UIGestureRecognizer recognizer, UITouch touch)
			{
				return true;
			}
		}		
	}
}
