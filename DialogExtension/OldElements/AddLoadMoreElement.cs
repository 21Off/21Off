using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MonoTouch.Dialog
{
	public class AddLoadMoreElement : Element, IElementSizing
	{
		static NSString key = new NSString ("LoadMoreElement");
		public UIColor TextColor { get; set; }
		public UIColor BackgroundColor { get; set; }
		public event Action<AddLoadMoreElement> Tapped = null;
		public UIFont Font;
		public float? Height;
		UITextAlignment alignment = UITextAlignment.Center;
		bool animating;
		
		UITextField entry;
		
		string placeholder;
		
		public AddLoadMoreElement (string _placeholder, Action<AddLoadMoreElement> tapped) : base ("")
		{
			placeholder = _placeholder;
			
			Tapped += tapped;
			Font = font;
							
		}
		
		static UIFont font = UIFont.BoldSystemFontOfSize (17);
		
		public string Value { 
			get {
				return val;
			}
			set {
				val = value;
				if (entry != null)
					entry.Text = value;
			}
		}
				
		
		public event EventHandler Changed;		
		string val;
		bool becomeResponder;
		
		/// <summary>
		/// Makes this cell the first responder (get the focus)
		/// </summary>
		/// <param name="animated">
		/// Whether scrolling to the location of this cell should be animated
		/// </param>
		public void BecomeFirstResponder (bool animated)
		{
			becomeResponder = true;
			var tv = GetContainerTableView ();
			if (tv == null)
				return;
			tv.ScrollToRow (IndexPath, UITableViewScrollPosition.Middle, animated);
			if (entry != null){
				entry.BecomeFirstResponder ();
				becomeResponder = false;
			}
		}

		public void ResignFirstResponder (bool animated)
		{
			becomeResponder = false;
			var tv = GetContainerTableView ();
			if (tv == null)
				return;
			tv.ScrollToRow (IndexPath, UITableViewScrollPosition.Middle, animated);
			if (entry != null)
				entry.ResignFirstResponder ();
        }
		
		public UIKeyboardType KeyboardType = UIKeyboardType.Default;
		
		private void CreateEntry(UITableView tv, UITableViewCell cell)
		{
			if (entry == null){
				var tableView = this.GetContainerTableView();
				tableView.Frame = new RectangleF(0, 39, 320, 480);
				
				var _entry = new UITextField ()
				{ 
					Tag = 3, Placeholder = placeholder ?? "",
					BackgroundColor = UIColor.Clear,			
					TextColor = TextColor ?? UIColor.Black,
					Font = Font ?? UIFont.BoldSystemFontOfSize (16),								
					Text = Value ?? "",
				};
				
				entry = _entry;
				
				entry.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleLeftMargin;
				
				entry.ValueChanged += delegate {
					FetchValue ();
				};
				entry.Ended += delegate {
					FetchValue ();
				};
				entry.ShouldReturn += delegate {
					AddLoadMoreElement focus = null;
					foreach (var e in (Parent as Section).Elements){
						if (e == this)
							focus = this;
						else if	(focus != null && e is AddLoadMoreElement){
							focus = e as AddLoadMoreElement;
							break;
						}
					}
					if (focus != null && focus.entry != null)
						if (focus != this)
							focus.entry.BecomeFirstResponder ();
						else 
							focus.entry.ResignFirstResponder ();
					
					if (Tapped != null)
					{
						Animating = true;
						Tapped(this);
					}
					
					return true;
				};
				entry.Started += delegate {
					AddLoadMoreElement self = null;
					var returnType = UIReturnKeyType.Send;
					
					foreach (var e in (Parent as Section).Elements){
						if (e == this)
							self = this;
						else if (self != null && e is AddLoadMoreElement)
							returnType = UIReturnKeyType.Next;
					}
					entry.ReturnKeyType = returnType;
				};
			}
			if (becomeResponder){
				entry.BecomeFirstResponder ();
				becomeResponder = false;
			}
			entry.KeyboardType = KeyboardType;			
		}	
				
		public override UITableViewCell GetCell (UITableView tv)
		{						
			var cell = tv.DequeueReusableCell (key);			
			UIActivityIndicatorView activityIndicator;
			
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, key);
				
				CreateEntry(tv, cell);
				
				activityIndicator = new UIActivityIndicatorView () {
					ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray,
					Tag = 1
				};
				cell.ContentView.AddSubview (entry);
				cell.ContentView.AddSubview (activityIndicator);
			} else {
				
				cell.Editing = false;				
				activityIndicator = cell.ContentView.ViewWithTag (1) as UIActivityIndicatorView;
				entry = cell.ContentView.ViewWithTag (3) as UITextField;
			}
			if (Animating){
				activityIndicator.Hidden = false;
				activityIndicator.StartAnimating ();
			} else {
				activityIndicator.Hidden = true;
				activityIndicator.StopAnimating ();
			}

			cell.ContentView.BackgroundColor = BackgroundColor ?? UIColor.Clear;
			
			Layout (cell, activityIndicator);
			return cell;
		}
		
		public bool Animating {
			get {
				return animating;
			}
			set {
				if (animating == value)
					return;
				animating = value;
				var cell = GetActiveCell ();
				if (cell == null)
					return;
				
				var activityIndicator = cell.ContentView.ViewWithTag (1) as UIActivityIndicatorView;
				if (value){
					activityIndicator.Hidden = false;
					activityIndicator.StartAnimating ();
				} else {
					activityIndicator.StopAnimating ();
					activityIndicator.Hidden = true;
				}
				Layout (cell, activityIndicator);
			}
		}
		
		/// <summary>
		///  Copies the value from the currently entry UIView to the Value property and raises the Changed event if necessary.
		/// </summary>
		public void FetchValue ()
		{
			if (entry == null)
				return;

			var newValue = entry.Text;
			var diff = newValue != Value;
			Value = newValue;
			
			if (diff){
				if (Changed != null)
					Changed (this, EventArgs.Empty);
			}
		}		
				
		protected override void Dispose (bool disposing)
		{
			if (disposing){
				if (entry != null){
					entry.Dispose ();
					entry = null;
				}
			}
		}		
		
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			tableView.DeselectRow (path, true);		
			
			if (Animating)
				return;
			
			if (Tapped != null){
				Animating = true;
				Tapped (this);
			}
		}
		
		SizeF GetTextSize (string text)
		{
			return new NSString (text).StringSize (Font, UIScreen.MainScreen.Bounds.Width, UILineBreakMode.TailTruncation);
		}
		
		const int pad = 10;
		const int isize = 20;
		
		public float GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			return Height ?? GetTextSize (placeholder).Height + 2*pad;
		}
		
		void Layout (UITableViewCell cell, UIActivityIndicatorView activityIndicator)
		{
			var sbounds = cell.ContentView.Bounds;

			var size = GetTextSize (placeholder);
			var size2 = GetTextSize(placeholder);
			
			if (!activityIndicator.Hidden)
				activityIndicator.Frame = new RectangleF (320 - 50, pad, isize, isize);
						
			entry.Frame = new RectangleF(pad, pad, 320 - 50 - pad, size.Height);
			//desc_entry.Frame = new RectangleF((320 - size2.Width)/2, pad, size2.Width, size.Height);
		}
		
		public UITextAlignment Alignment { 
			get { return alignment; } 
			set { alignment = value; }
		}
		public UITableViewCellAccessory Accessory { get; set; }
	}	
}
