using MonoTouch.UIKit;

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
