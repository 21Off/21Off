using System;
using MonoTouch.UIKit;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	public abstract class BoolElement : Element {
		bool val;
		public bool Value {
			get {
				return val;
			}
			set {
				bool emit = val != value;
				val = value;
				if (emit && ValueChanged != null)
					ValueChanged (this, EventArgs.Empty);
			}
		}
		public event EventHandler ValueChanged;
		
		public BoolElement (string caption, bool value) : base (caption)
		{
			val = value;
		}
		
		public virtual void SetValue(bool value)
		{
			val = value;
		}
		
		public override string Summary ()
		{
			return val ? "On" : "Off";
		}		
	}	
	
	/// <summary>
	/// Used to display switch on the screen.
	/// </summary>
	public class CustomBooleanElement : BoolElement {
		static NSString bkey = new NSString ("CustomBooleanElement");
		UISwitch sw;
		UIFont font;
		
		public CustomBooleanElement(string caption, bool value, UIFont font) : base(caption, value)
		{
			this.font = font;
		}
		
		public CustomBooleanElement (string caption, bool value) : base (caption, value)
		{  }
		
		public CustomBooleanElement (string caption, bool value, string key) : base (caption, value)
		{  }
		
		public override void SetValue (bool value)
		{
			base.SetValue (value);
			
			sw.On = value;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			if (sw == null){
				sw = new UISwitch (){
					BackgroundColor = UIColor.Clear,
					Tag = 1,
					On = Value
				};
				sw.AddTarget (delegate {
					Value = sw.On;
				}, UIControlEvent.ValueChanged);
			} else
				sw.On = Value;
			
			var cell = tv.DequeueReusableCell (bkey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, bkey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			} else
				RemoveTag (cell, 1);
		
			if (font != null) 
				cell.TextLabel.Font = font;
			cell.TextLabel.Text = Caption;
			cell.AccessoryView = sw;

			return cell;
		}
		
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				if (sw != null){
					sw.Dispose ();
					sw = null;
				}
			}
		}
	}	
	
	public class CustomCheckboxElement : StringElement {
		public new bool Value;
		public string Group;
		
		public CustomCheckboxElement (string caption) : base (caption) {}
		public CustomCheckboxElement (string caption, bool value) : base (caption)
		{
			Value = value;
		}
		
		public CustomCheckboxElement (string caption, bool value, string group) : this (caption, value)
		{
			Group = group;
		}
		
		UITableViewCell ConfigCell (UITableViewCell cell)
		{
			cell.Accessory = Value ? UITableViewCellAccessory.Checkmark : UITableViewCellAccessory.None;
			return cell;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			return  ConfigCell (base.GetCell (tv));
		}
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			Value = !Value;
			
			if (ValueChanged != null)
				ValueChanged(this, EventArgs.Empty);
			else
			{				
				var cell = tableView.CellAt (path);
				ConfigCell (cell);
				base.Selected (dvc, tableView, path);
			}

		}
		
		public void SetValue(bool value)
		{
			Value = value;
			ConfigCell(this.GetActiveCell());
		}
		
		public event EventHandler ValueChanged;
	}
	

	/// <summary>
	///  Used to display a cell that will launch a web browser when selected.
	/// </summary>
	public class CHtmlElement : Element {
		NSUrl nsUrl;
		static NSString hkey = new NSString ("CHtmlElement");
		public UIWebView web;
		
		public CHtmlElement (string caption, string url) : base (caption)
		{
			Url = url;
		}
		
		public CHtmlElement (string caption, NSUrl url) : base (caption)
		{
			nsUrl = url;
		}
		
		public string Url {
			get {
				return nsUrl.ToString ();
			}
			set {
				nsUrl = new NSUrl (value);
			}
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (hkey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, hkey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.Blue;
			}
			cell.Accessory = UITableViewCellAccessory.DisclosureIndicator;
			
			cell.TextLabel.Text = Caption;
			return cell;
		}

		public static bool NetworkActivity {
			set {
				UIApplication.SharedApplication.NetworkActivityIndicatorVisible = value;
			}
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
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			var vc = new WebViewController (this) {
				Autorotate = dvc.Autorotate
			};
			web = new UIWebView (UIScreen.MainScreen.ApplicationFrame){
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
			
			OnActivateController(dvc, vc);
			web.LoadRequest (NSUrlRequest.FromUrl (nsUrl));
		}
		
		
		public virtual void OnActivateController(DialogViewController dvc, UIViewController vc)
		{
			dvc.ActivateController (vc);
		}
	}	
}

