
using System;
using System.Collections.Generic;
using System.Drawing;
using MonoTouch.CoreLocation;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
namespace MSP.Client
{
		public class HorizontalScrollView : UIScrollView
		{
			public HorizontalScrollView(TimelineViewController timeline)
			{
				AutoresizingMask = UIViewAutoresizing.FlexibleHeight | UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleTopMargin;
				AutosizesSubviews = true;
				AlwaysBounceVertical = false;
				DirectionalLockEnabled = true;
					
				ShowsVerticalScrollIndicator = false;
				ShowsHorizontalScrollIndicator = false;				
				
				ScrollEnabled = true;
				PagingEnabled = true;
				Bounces = true;		
				DelaysContentTouches = false;				
			}
		}	
}
