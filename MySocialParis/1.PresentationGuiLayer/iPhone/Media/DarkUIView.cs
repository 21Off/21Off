
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MonoTouch.CoreAnimation;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;

namespace MSP.Client
{
	public class DarkUIView : UIView
	{
		public DarkUIView()
		{
			Layer.BackgroundColor = UIColor.Black.CGColor;
			Layer.ShadowColor = UIColor.LightGray.CGColor;
			Layer.ShadowRadius = 1.0f;
			Layer.ShadowOffset = new SizeF(0, -1);
			Layer.ShadowOpacity = 0.8f;				
		}
		
		public override void Draw (RectangleF rect)
		{
			if (OnDraw != null)
			{
				CGContext context = UIGraphics.GetCurrentContext();
				OnDraw(rect, context, this);
			}
			else
				base.Draw(rect);
		}
		
		public void Update()
		{
			SetNeedsDisplay();
		
		}			
		
		public event Action<RectangleF, CGContext, UIView> OnDraw;
	}	
}
