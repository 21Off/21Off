using System;
using System.Drawing;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MonoTouch.Dialog
{
	public class AddLoadMoreWithImageElement : Element, IElementSizing
	{
		static NSString key = new NSString ("AddLoadMoreWithImageElement");
		public UIColor TextColor { get; set; }
		public UIColor BackgroundColor { get; set; }
		public event Action<AddLoadMoreWithImageElement> Tapped = null;
		public UIFont Font;
		public float? Height;
		UITextAlignment alignment = UITextAlignment.Center;
		bool animating;
		
		UITextField entry;
		
		string placeholder;
		
		UIImage activityImage;
		
		const int PicSize = 40;		
		
		public AddLoadMoreWithImageElement (string _placeholder, UIImage image, Action<AddLoadMoreWithImageElement> tapped) : base ("")
		{
			activityImage = image;
			
			BackgroundColor = UIColor.Clear;
			
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
				
				var _entry = new UITextField (){ Tag = 3, Placeholder = placeholder ?? "" };
				
				_entry.BackgroundColor = UIColor.Clear;				
				_entry.TextColor = TextColor ?? UIColor.Black;
				_entry.Font = Font ?? UIFont.BoldSystemFontOfSize (16);
								
				_entry.Text = Value ?? "";
				entry = _entry;
				
				entry.AutoresizingMask = UIViewAutoresizing.FlexibleWidth | UIViewAutoresizing.FlexibleLeftMargin;
				
				entry.ValueChanged += delegate {
					FetchValue ();
				};
				entry.Ended += delegate {
					FetchValue ();
				};
				entry.ShouldReturn += delegate {
					AddLoadMoreWithImageElement focus = null;
					foreach (var e in (Parent as Section).Elements){
						if (e == this)
							focus = this;
						else if	(focus != null && e is AddLoadMoreWithImageElement){
							focus = e as AddLoadMoreWithImageElement;
							break;
						}
					}
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
					AddLoadMoreWithImageElement self = null;
					var returnType = UIReturnKeyType.Done;
					
					foreach (var e in (Parent as Section).Elements){
						if (e == this)
							self = this;
						else if (self != null && e is AddLoadMoreWithImageElement)
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
		
		static CGPath badgePath = GraphicsUtil.MakeRoundedPath (42, 4);
		static CGPath commentPath = GraphicsUtil.MakeRoundedRectPath(new RectangleF(0, 0, 310, 56), 4);		
		
		public class LoadMoreWithImageCellView : UIView {
						
			public LoadMoreWithImageCellView () : base ()
			{			
				Opaque = true;
				BackgroundColor = UIColor.FromHSBA(0, 0, 0, 0);
			}
			
			public override void Draw (RectangleF rect)
			{
				try {
					RealDraw (rect);
				} catch (Exception e) {;
				}
			}
			
			void RealDraw (RectangleF rect)
			{			
				var context = UIGraphics.GetCurrentContext ();
									
				// Cute touch
				UIColor.FromRGB(239, 239, 239).SetColor ();
				context.SaveState ();
				context.TranslateCTM (5, (60 - 56) / 2);
				context.SetLineWidth (1);
				
				// On device, the shadow is painted in the opposite direction!
				context.SetShadowWithColor (new SizeF (1, 1), 3, UIColor.DarkGray.CGColor);
				context.AddPath (commentPath);
				context.FillPath ();
				context.RestoreState ();
			}
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{						
			var cell = tv.DequeueReusableCell (key);			
			UIActivityIndicatorView activityIndicator;
			LoadMoreWithImageCellView loadMoreCellVIew;
			
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, key);
				
				CreateEntry(tv, cell);
				
				activityIndicator = new UIActivityIndicatorView () {
					ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray,
					Tag = 1
				};
				
				
				loadMoreCellVIew = new LoadMoreWithImageCellView();
				loadMoreCellVIew.Frame = new RectangleF(0, 0, 320, 60);
				
				cell.ContentView.AddSubview(loadMoreCellVIew);
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
			
			if (!activityIndicator.Hidden)
				activityIndicator.Frame = new RectangleF (320 - 50, 1 + (56 - size.Height) / 2, isize, isize);
						
			entry.Frame = new RectangleF(pad, 1 + (56 - size.Height) / 2 , 320 - 50 - pad, size.Height);
		}
		
		public UITextAlignment Alignment { 
			get { return alignment; } 
			set { alignment = value; }
		}
		public UITableViewCellAccessory Accessory { get; set; }
	}	
}
