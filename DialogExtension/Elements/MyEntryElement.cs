using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MonoTouch.Dialog
{
	/// <summary>
	/// An element that can be used to enter text.
	/// </summary>
	/// <remarks>
	/// This element can be used to enter text both regular and password protected entries. 
	///     
	/// The Text fields in a given section are aligned with each other.
	/// </remarks>
	public class MyEntryElement : Element {
		/// <summary>
		///   The value of the EntryElement
		/// </summary>
		
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
		string val;
		
		/// <summary>
		/// The type of keyboard used for input, you can change
		/// this to use this for numeric input, email addressed,
		/// urls, phones.
		/// </summary>
		public UIKeyboardType KeyboardType = UIKeyboardType.Default;
		
		static NSString ekey = new NSString ("EntryElement");
		bool becomeResponder;
		UITextField entry;
		UITextView textView;
		bool isPassword;
		string placeholder;
		static UIFont font = UIFont.BoldSystemFontOfSize (17);

		public event EventHandler Changed;
		public event EventHandler OnReturn;
		
		protected UIReturnKeyType _returnKeyType = UIReturnKeyType.Done;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="MonoTouch.Dialog.MyEntryElement"/> class.
		/// </summary>
		/// <param name='placeholder'>
		/// Placeholder.
		/// </param>
		/// <param name='value'>
		/// Value.
		/// </param>
		/// <param name='isPassword'>
		/// Is password.
		/// </param>
		/// <param name='returnKeyType'>
		/// Return key type.
		/// </param>
		public MyEntryElement (string placeholder, string value, bool isPassword, UIReturnKeyType returnKeyType) : base (null)
		{
			Value = value;
			
			this.isPassword = isPassword;			
			this.placeholder = placeholder;
			
			_returnKeyType = returnKeyType;
		}
		
		/// <summary>
		/// Constructs an EntryElement with the given caption, placeholder and initial value.
		/// </summary>
		/// <param name="caption">
		/// The caption to use
		/// </param>
		/// <param name="placeholder">
		/// Placeholder to display.
		/// </param>
		/// <param name="value">
		/// Initial value.
		/// </param>
		public MyEntryElement (string placeholder, string value, bool isPassword) :
			this(placeholder,value,isPassword,UIReturnKeyType.Done)
		{
		}
		
		public MyEntryElement(string placeholder, string value) : this(placeholder, value, false)
		{
		}

		public override string Summary ()
		{
			return Value;
		}

		// 
		// Computes the X position for the entry by aligning all the entries in the Section
		//
		SizeF ComputeEntryPosition (UITableView tv, UITableViewCell cell)
		{
			Section s = Parent as Section;
			if (s.EntryAlignment.Width != 0)
				return s.EntryAlignment;
			
			SizeF max = new SizeF (-1, -1);
			foreach (var e in s.Elements){
				var ee = e as EntryElement;
				if (ee == null)
					continue;
				
				var size = tv.StringSize ("ee.Caption", font);
				if (size.Width > max.Width)
					max = size;				
			}
			s.EntryAlignment = new SizeF (25 + Math.Min (max.Width, 160), max.Height);
			return s.EntryAlignment;
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (ekey);
			if (cell == null){
				cell = new UITableViewCell (UITableViewCellStyle.Default, ekey);
				cell.SelectionStyle = UITableViewCellSelectionStyle.None;
			} else 
				RemoveTag (cell, 1);
			
			if (entry == null){
				SizeF size = ComputeEntryPosition (tv, cell);
				size = tv.StringSize ("ee.Caption", font);
				size = new SizeF(10, size.Height);
				var frame = new RectangleF (size.Width, (cell.ContentView.Bounds.Height-size.Height)/2-1, 320-size.Width, size.Height);
				//frame = new RectangleF (size.Width, 0, 320-size.Width, cell.ContentView.Bounds.Height);
				var _entry = new UITextField (frame){
					Tag = 1,
					Placeholder = placeholder ?? "",
					SecureTextEntry = isPassword
				};
				textView = new UITextView(new RectangleF (10, 1, cell.ContentView.Bounds.Width - 10, cell.ContentView.Bounds.Height - 2));
				textView.Hidden = true;
				textView.Editable = false;
				
				_entry.Text = Value ?? "";
				entry = _entry;
				
				entry.AutoresizingMask = UIViewAutoresizing.FlexibleWidth |
					UIViewAutoresizing.FlexibleLeftMargin;
				
				entry.ValueChanged += delegate {
					FetchValue ();
				};
				entry.Ended += delegate {
					FetchValue ();
				};
				entry.ShouldReturn += delegate {
					MyEntryElement focus = null;
					foreach (var e in (Parent as Section).Elements){
						if (e == this)
							focus = this;
						else if (focus != null && e is MyEntryElement){
							focus = e as MyEntryElement;
							break;
						}
					}
					if (focus != this)
						focus.entry.BecomeFirstResponder ();
					else 
						focus.entry.ResignFirstResponder ();
					
					if (OnReturn != null)
						OnReturn(this, new EventArgs());
					
					//textView.Text = entry.Text;
					//textView.Hidden = false;
					//entry.Hidden = true;
					return true;
				};
				entry.Started += delegate {
					MyEntryElement self = null;
					var returnType = _returnKeyType;//UIReturnKeyType.Done;
					
					foreach (var e in (Parent as Section).Elements){
						if (e == this)
							self = this;
						else if (self != null && e is MyEntryElement)
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
			
			cell.ContentView.AddSubview (entry);
			//cell.ContentView.AddSubview (textView);			
			
			return cell;
		}
		
		/// <summary>
		///  Copies the value from the currently entry UIView to the Value property and raises the Changed event if necessary.
		/// </summary>
		public void FetchValue ()
		{
			try
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
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
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
		
		public override bool Matches (string text)
		{
			return (Value != null ? Value.IndexOf (text, StringComparison.CurrentCultureIgnoreCase) != -1: false) || base.Matches (text);
		}
		
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
        }	}	
}
