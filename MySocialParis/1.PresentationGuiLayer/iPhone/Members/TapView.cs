using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public class TapView : UIView
	{
		public List<Block> blocks;
		Block highlighted = null;
		
		bool blockingTouchEvents;
		NSTimer holdTimer;					
		
		// Tapped events
		public delegate void TappedCallEvent(object callObject);
		public delegate void TappedEvent (string value);
		
		public event Action<Block> TappedBlock;
		public event TappedEvent Tapped;
		public event TappedEvent TapAndHold;
		
		public event TappedCallEvent TappedCall;
		public event TappedCallEvent TapAndHoldCall;		
		
		private UIFont userFont;
		
		public TapView(RectangleF rect) : base(rect)
		{
			blocks = new List<Block>();
		}		
		
		public TapView()
		{
			blocks = new List<Block>();
		}
		
		public TapView(UIFont userFont) : this()
		{
			this.userFont = userFont;
		}
		
		public override void TouchesBegan (NSSet touches, UIEvent evt)
		{
			blockingTouchEvents = false;
			Track ((touches.AnyObject as UITouch).LocationInView (this));
			
			// Start tracking tap and hold
			if (highlighted != null && (highlighted.Font == userFont || userFont == null)){
				holdTimer = NSTimer.CreateScheduledTimer (TimeSpan.FromSeconds (1), delegate {
					blockingTouchEvents = true;
					
					Console.WriteLine("Tap and hold "+ highlighted.Value);
					if (TapAndHold != null)
						TapAndHold (highlighted.Value);					
					if (highlighted.CallObject != null && TapAndHoldCall != null)
						TapAndHoldCall (highlighted.CallObject);
				});
			}
		}

		void CancelHoldTimer ()
		{
			if (holdTimer == null)
				return;
			holdTimer.Invalidate ();
			holdTimer = null;
		}
		
		void Track (PointF pos)
		{
			foreach (var block in blocks){
				if (!block.Bounds.Contains (pos))
					continue;

				highlighted = block;
				SetNeedsDisplay ();
			}
		}
		
		public override void TouchesEnded (NSSet touches, UIEvent evt)
		{
			CancelHoldTimer ();
			if (blockingTouchEvents)
				return;
			
			if (highlighted != null && (highlighted.Font == userFont || userFont == null)){
				Console.WriteLine("Touches ended " + highlighted.Value);
				if (Tapped != null)
					Tapped (highlighted.Value);
				if (TappedBlock != null)
					TappedBlock(highlighted);				
				if (highlighted != null && TappedCall != null)
					TappedCall (highlighted.CallObject);				
			}
			
			highlighted = null;
			SetNeedsDisplay ();
		}
		
		public override void TouchesCancelled (NSSet touches, UIEvent evt)
		{
			CancelHoldTimer ();
			highlighted = null;
			SetNeedsDisplay ();
		}

		public override void TouchesMoved (NSSet touches, UIEvent evt)
		{
			CancelHoldTimer ();
			Track ((touches.AnyObject as UITouch).LocationInView (this));
		}			
	}	
}
