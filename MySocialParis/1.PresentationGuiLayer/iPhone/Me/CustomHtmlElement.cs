
using System;
using System.Drawing;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.CoreGraphics;
using System.Threading;

namespace MSP.Client
{
	public class CustomHtmlElement : CHtmlElement
	{
		private UINavigationController nav;
		
		public CustomHtmlElement(string caption, string url, UINavigationController nav) : base(caption, url)
		{
			this.nav = nav;
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			tableView.DeselectRow (path, true);
			
			//base.Selected (dvc, tableView, path);
			
			var evc = new EmptyViewController(()=>
			{
				nav.PopViewControllerAnimated(true);
			}, Caption);			
			
			var vc = new WebViewController (this) {
				Autorotate = dvc.Autorotate
			};
			var web = new UIWebView (UIScreen.MainScreen.ApplicationFrame){
				BackgroundColor = UIColor.White,
				ScalesPageToFit = true,
				AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleHeight
			};
			web.LoadStarted += delegate {
				NetworkActivity = true;
				var indicator = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.White);
				vc.NavigationItem.RightBarButtonItem = new UIBarButtonItem(indicator);
				indicator.StartAnimating();
			};
			web.LoadFinished += delegate {
				NetworkActivity = false;
				vc.NavigationItem.RightBarButtonItem = null;
			};
			web.LoadError += (webview, args) => {
				NetworkActivity = false;
				vc.NavigationItem.RightBarButtonItem = null;
				if (web != null)
					web.LoadHtmlString (String.Format ("<html><center><font size=+5 color='red'>An error occurred:<br>{0}</font></center></html>", args.Error.LocalizedDescription), null);
			};
			vc.NavigationItem.Title = Caption;
			vc.View.AddSubview (web);
			//vc.View.Frame = new RectangleF(0, 41 , 320, 480 - 41);
			
			evc.Add(vc.View);
			nav.PushViewController(evc, true);
			
			web.LoadRequest (NSUrlRequest.FromUrl (new NSUrl (Url)));
		}
		
		// We use this class to dispose the web control when it is not
		// in use, as it could be a bit of a pig, and we do not want to
		// wait for the GC to kick-in.
		private class WebViewController : UIViewController {
			CHtmlElement container;
			
			public WebViewController (CHtmlElement container) : base ()
			{
				this.container = container;
			}
			
			public override void ViewWillDisappear (bool animated)
			{
				base.ViewWillDisappear (animated);
				NetworkActivity = false;
				if (container.web == null)
					return;

				container.web.StopLoading ();
				container.web.Dispose ();
				container.web = null;
			}

			public bool Autorotate { get; set; }
			
			public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
			{
				return Autorotate;
			}
		}		
	}
}
