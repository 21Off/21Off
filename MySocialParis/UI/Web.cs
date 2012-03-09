//
// Copyright 2010 Miguel de Icaza
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
//
using System;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using System.Drawing;

namespace TweetStation
{
	public class WebViewController : UIViewController {
		static WebViewController Main = new WebViewController ();
		
		UIToolbar topBar;
		UIToolbar toolbar;
		UIBarButtonItem backButton, forwardButton, stopButton, refreshButton;
		UILabel title;
		protected UIWebView WebView;
		
		DialogViewController _parent;
		
		public void SetParent(DialogViewController parent)
		{
			_parent = parent;
		}				

		protected WebViewController ()
		{
			var fixedSpace = new UIBarButtonItem (UIBarButtonSystemItem.FixedSpace, null) {
				Width = 26
			};
			var flexibleSpace = new UIBarButtonItem (UIBarButtonSystemItem.FlexibleSpace, null);

			toolbar = new UIToolbar ();
			topBar = new UIToolbar ();
			
			title = new UILabel (new RectangleF (10, 0, 80, 30)){
				BackgroundColor = UIColor.Clear,
				AdjustsFontSizeToFitWidth = true,
				Font = UIFont.BoldSystemFontOfSize (22),
				MinimumFontSize = 14,
				TextColor = UIColor.White,
				ShadowColor = UIColor.FromRGB (64, 74, 87),
				ShadowOffset = new SizeF (0, -1)
			};
			
			topBar.Items = new UIBarButtonItem []  {
				new UIBarButtonItem (title),
				flexibleSpace,
				new UIBarButtonItem (Locale.GetText ("Close"), UIBarButtonItemStyle.Bordered, (o, e) => { DismissModalViewControllerAnimated (true);} )
			};
			
			backButton = new UIBarButtonItem (UIImage.FromBundle ("Images/back.png"), UIBarButtonItemStyle.Plain, (o, e) => { WebView.GoBack (); });
			forwardButton = new UIBarButtonItem (UIImage.FromBundle ("Images/forward.png"), UIBarButtonItemStyle.Plain, (o, e) => { WebView.GoForward (); });
			refreshButton = new UIBarButtonItem (UIBarButtonSystemItem.Refresh, (o, e) => { WebView.Reload (); });
			stopButton = new UIBarButtonItem (UIBarButtonSystemItem.Stop, (o, e) => { WebView.StopLoading (); });
			
			toolbar.Items = new UIBarButtonItem [] { backButton, fixedSpace, forwardButton, flexibleSpace, stopButton, fixedSpace, refreshButton };

			View.AddSubview (topBar);
			View.AddSubview (toolbar);
		}

		void UpdateNavButtons ()
		{
			if (WebView == null)
				return;
			
			backButton.Enabled = WebView.CanGoBack;
			forwardButton.Enabled = WebView.CanGoForward;
		}
		
		protected virtual string UpdateTitle ()
		{
			return WebView.EvaluateJavascript ("document.title");			
		}
		
		public void SetupWeb (string initialTitle)
		{
			WebView = new UIWebView (){
				ScalesPageToFit = true,
				MultipleTouchEnabled = true,
				AutoresizingMask = UIViewAutoresizing.FlexibleHeight|UIViewAutoresizing.FlexibleWidth,
			};
			WebView.LoadStarted += delegate { 
				stopButton.Enabled = true;
				refreshButton.Enabled = false;
				UpdateNavButtons ();
				
				Util.PushNetworkActive (); 
			};
			WebView.LoadFinished += delegate {
				stopButton.Enabled = false;
				refreshButton.Enabled = true;
				Util.PopNetworkActive (); 
				UpdateNavButtons ();
				
				title.Text = UpdateTitle ();
			};
			
			title.Text = initialTitle;
			View.AddSubview (WebView);
			backButton.Enabled = false;
			forwardButton.Enabled = false;
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			return toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown;
		}
		
		public override void ViewDidDisappear (bool animated)
		{
			base.ViewDidDisappear (animated);
			if (WebView != null){
				WebView.RemoveFromSuperview ();
				WebView.Dispose ();
				WebView = null;
			}
			
			if (_parent != null)
				_parent.View.Frame = new RectangleF(0, 45, 320, 460 - 45);
		}

		void LayoutViews ()
		{
			var sbounds = View.Bounds;
			int top = (InterfaceOrientation == UIInterfaceOrientation.Portrait) ? 0 : -44;
			
			topBar.Frame = new RectangleF (0, top, sbounds.Width, 44);
			toolbar.Frame =  new RectangleF (0, sbounds.Height-44, sbounds.Width, 44);
			WebView.Frame = new RectangleF (0, top+44, sbounds.Width, sbounds.Height-88-top);
			
			title.Frame = new RectangleF (10, 0, sbounds.Width-80, 38);
		}
		
		public override void ViewWillAppear (bool animated)
		{
			base.ViewWillAppear (animated);
			LayoutViews ();
		}

		public override void DidRotate (UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate (fromInterfaceOrientation);
			LayoutViews ();
		}
		
		public static string EncodeIdna (string host)
		{
			foreach (char c in host){
				if (c > (char) 0x7f){
					var segments = host.Split ('.');
					var encoded = new string [segments.Length];
					int i = 0;
					
					foreach (var s in segments)
						encoded [i++] = Mono.Util.Punycode.Encode (s.ToLower ());
					
					return "xn--" + string.Join (".", encoded);
				}
			}
			return host;
		}
		
		public static void OpenUrl (DialogViewController parent, string url)
		{
			UIView.BeginAnimations ("foo");
			Main.HidesBottomBarWhenPushed = true;
			Main.SetupWeb (url);
			Main.SetParent(parent);
			
			if (url.StartsWith ("http://")){
				string host;
				int last = url.IndexOf ('/', 7);
				if (last == -1)
					host = url.Substring (7);
				else 
					host = url.Substring (7, last-7);
				url = "http://" + EncodeIdna (host) + (last == -1 ? "" : url.Substring (last));
			}
			Main.WebView.LoadRequest (new NSUrlRequest (new NSUrl (url)));
			
			parent.PresentModalViewController (Main, true);

			UIView.CommitAnimations ();
		}
		
		public static void OpenHtmlString (DialogViewController parent, string htmlString, NSUrl baseUrl)
		{
			UIView.BeginAnimations ("foo");
			Main.HidesBottomBarWhenPushed = true;
			Main.SetupWeb ("");
			
			Main.WebView.LoadHtmlString (htmlString, baseUrl);
			parent.ActivateController (Main);
			UIView.CommitAnimations ();
		}
		
		public static void OpenHtmlString (DialogViewController parent, string htmlString)
		{
			OpenHtmlString (parent, htmlString, new NSUrl (Util.BaseDir, true));
		}
	}
}

