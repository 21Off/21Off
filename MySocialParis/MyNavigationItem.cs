using MonoTouch.UIKit;

namespace MSP.Client
{
	public class MyNavigationItem : UINavigationItem
	{
		private UIBarButtonItem a;
		private UIBarButtonItem b;
		
		public MyNavigationItem(string title) : base(title)
		{
			a = new UIBarButtonItem("back", UIBarButtonItemStyle.Done, null);
			b  = new UIBarButtonItem("back", UIBarButtonItemStyle.Done, null);
			
			this.SetLeftBarButtonItem(a, true);		
		}
	}
}
