using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MSP.Client
{
	[MonoTouch.Foundation.Register("MSPNavigationController")]
	public class MSPNavigationController : UINavigationController
	{
		private AppDelegateIPhone _AppDel;		
		public AppDelegateIPhone AppDel {
			get {
				return this._AppDel;
			}
		}

		public MSPNavigationController (AppDelegateIPhone appDel)
		{			
			//NavigationBar.TintColor = UIColor.Red;
			NavigationBarHidden = true;
			
			
			_AppDel = appDel;
		}
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			
			if (OnViewAppeared != null)
				OnViewAppeared(this, EventArgs.Empty);
		}
		
		public event EventHandler OnViewAppeared;
	}		
}

