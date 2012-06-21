//
// This cell does not perform cell recycling, do not use as
// sample code for new elements. 
//
using System;
using System.Drawing;
using System.Threading;
using MonoTouch.CoreFoundation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;



namespace MonoTouch.Dialog
{
	public class CustomLoadMoreElement : Element, IElementSizing
	{
		static NSString key = new NSString ("LoadMoreElement");
		public UIColor BackgroundColor { get; set; }
		public event Action<CustomLoadMoreElement> Tapped = null;
		public float? Height;
		bool animating;
		
		private UIImage _Image;
		
		public CustomLoadMoreElement () : base ("")
		{
		}
		
		public CustomLoadMoreElement (Action<CustomLoadMoreElement> tapped) : base("")
		{
			Tapped += tapped;
		}		
		
		public UIImage Image
		{
			set
			{
				_Image = value;
			}
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			var cell = tv.DequeueReusableCell (key);
			UIActivityIndicatorView activityIndicator;
			ImageButton imageButton;
			
			if (cell == null)
			{
				cell = new UITableViewCell (UITableViewCellStyle.Default, key);
			
				activityIndicator = new UIActivityIndicatorView () 
				{
					ActivityIndicatorViewStyle = UIActivityIndicatorViewStyle.Gray,
					Tag = 1
				};				
								
				imageButton = new ImageButton(new RectangleF(0, 0, 0, 0))
				{
					ContentMode = UIViewContentMode.Center,
					Tag = 3
				};
				imageButton.TouchUpInside+= HandleGlassButtonTouchUpInside;
				
				cell.ContentView.AddSubview(imageButton);
				cell.ContentView.AddSubview (activityIndicator);
			} 
			else 
			{
				activityIndicator = cell.ContentView.ViewWithTag (1) as UIActivityIndicatorView;
				imageButton = cell.ContentView.ViewWithTag(3) as ImageButton;
			}
			
			if (Animating)
			{
				activityIndicator.Hidden = false;
				activityIndicator.StartAnimating ();
			} 
			else 
			{
				activityIndicator.Hidden = true;
				activityIndicator.StopAnimating ();
			}
			
			if (BackgroundColor != null)
			{
				cell.ContentView.BackgroundColor = BackgroundColor ?? UIColor.Clear;
			} 
			else 
			{
				cell.ContentView.BackgroundColor = null;
			}
			
			imageButton.BackgroundColor = UIColor.Clear;

			Layout (cell, activityIndicator, imageButton);
			return cell;
		}

		void HandleGlassButtonTouchUpInside (object sender, EventArgs e)
		{
			if (Animating)
				return;
			
			if (Tapped != null){
				Animating = true;
				Tapped (this);
			}
		}
		
		public bool Animating 
		{
			get 
			{
				return animating;
			}
			
			set 
			{
				if (animating == value)
					return;
				
				animating = value;
				var cell = GetActiveCell ();
				if (cell == null)
					return;
				
				var activityIndicator = cell.ContentView.ViewWithTag (1) as UIActivityIndicatorView;
				var imageButton = cell.ContentView.ViewWithTag(3) as ImageButton;
				
				if (value)
				{
					imageButton.Hidden = true;
					activityIndicator.Hidden = false;
					activityIndicator.StartAnimating ();
				} 
				else 
				{
					activityIndicator.StopAnimating ();
					activityIndicator.Hidden = true;
					imageButton.Hidden = false;
				}
				
				Layout (cell, activityIndicator, imageButton);
			}
		}
				
		public override void Selected (DialogViewController dvc, UITableView tableView, NSIndexPath path)
		{
			tableView.DeselectRow (path, true);
			
			if (Animating)
				return;
			
			if (Tapped != null)
			{
				Animating = true;
				Tapped (this);
			}
		}	
		
		public float GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			return Height.Value;
		}
		
		void Layout (UITableViewCell cell, UIActivityIndicatorView activityIndicator, ImageButton button)
		{			
			var sbounds = cell.ContentView.Bounds;
			
			if (!activityIndicator.Hidden)
			{
				activityIndicator.Frame = new RectangleF ((sbounds.Width - _Image.Size.Width) / 2 ,10, 
					_Image.Size.Width, _Image.Size.Height);
			}
			else
			{
				button.BackgroundColor = UIColor.Clear;
				button.SetTitle ("", UIControlState.Normal);
				button.NormalColor = UIColor.Clear;
				button.DrawImage = true;
				
				button.Frame = new RectangleF ((sbounds.Width - _Image.Size.Width) / 2 ,10, 
					_Image.Size.Width, _Image.Size.Height);
				button.SetBackgroundImage(_Image, UIControlState.Normal);
			}
		}		
		
		public UITableViewCellAccessory Accessory { get; set; }
	}
}

