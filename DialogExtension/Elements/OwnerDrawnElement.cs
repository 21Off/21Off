using System;
using System.Drawing;
using MonoTouch.UIKit;
using MonoTouch.CoreFoundation;
using MonoTouch.CoreGraphics;
using MonoTouch.Foundation;

namespace MonoTouch.Dialog
{
	
	public abstract class CustomOwnerDrawnElement : Element, IElementSizing
	{		
		public string CellReuseIdentifier
		{
			get;set;	
		}
		
		public UITableViewCellStyle Style
		{
			get;set;	
		}
		
		public UITableViewCellSelectionStyle SelectionStyle 
		{
			get;set;
		}
		
		public UITableViewCellAccessory Accessory 
		{
			get;set;
		}		
		
		public CustomOwnerDrawnElement (UITableViewCellStyle style, string cellIdentifier) : base(null)
		{
			this.CellReuseIdentifier = cellIdentifier;
			this.Style = style;
		}
		
		public CustomOwnerDrawnElement (UITableViewCellStyle style, string cellIdentifier, 
                      UITableViewCellSelectionStyle selectionStyle, UITableViewCellAccessory accesory)
			: this(style, cellIdentifier)
		{
			this.SelectionStyle = selectionStyle;
			this.Accessory = accesory;			
		}
		
		public float GetHeight (UITableView tableView, NSIndexPath indexPath)
		{
			return Height(tableView.Bounds);
		}
		
		public override UITableViewCell GetCell (UITableView tv)
		{
			CustomOwnerDrawnCell cell = tv.DequeueReusableCell(this.CellReuseIdentifier) as CustomOwnerDrawnCell;
			
			if (cell == null)
			{
				cell = new CustomOwnerDrawnCell(this, this.Style, this.CellReuseIdentifier)
				{
					SelectionStyle = SelectionStyle,
					Accessory = Accessory,
				};
			}
			else
			{
				cell.Element = this;
			}
			
			cell.Update();
			return cell;
		}	
		
		public abstract void Draw(RectangleF bounds, CGContext context, UIView view);
		public virtual void DrawImageView(UIImageView view)
		{}
		
		public abstract float Height(RectangleF bounds);
		
		class CustomOwnerDrawnCell : UITableViewCell
		{
			OwnerDrawnCellView view;
			
			public CustomOwnerDrawnCell(CustomOwnerDrawnElement element, UITableViewCellStyle style, string cellReuseIdentifier) : base(style, cellReuseIdentifier)
			{
				Element = element;
			}
			
			public CustomOwnerDrawnElement Element
			{
				get {
					return view.Element;
				}
				set {
					if (view == null)
					{
						view = new OwnerDrawnCellView (value, this);
						ContentView.Add (view);
					}
					else
					{
						view.Element = value;
					}
				}
			}							

			public void Update()
			{
				SetNeedsDisplay();
				view.SetNeedsDisplay();
			}		
	
			public override void LayoutSubviews()
			{
				base.LayoutSubviews();

				view.Frame = ContentView.Bounds;
			}			
		}
		
		class OwnerDrawnCellView : UIView
		{				
			CustomOwnerDrawnElement element;
			CustomOwnerDrawnCell cell;
			
			public OwnerDrawnCellView(CustomOwnerDrawnElement element, CustomOwnerDrawnCell cell)
			{
				this.element = element;
				this.cell = cell;
			}
			
			public CustomOwnerDrawnElement Element
			{
				get { return element; }
				set { element = value; }
			}
			
			public void Update()
			{
				SetNeedsDisplay();			
			}
			
			public override void Draw (RectangleF rect)
			{
				CGContext context = UIGraphics.GetCurrentContext();
				element.Draw(rect, context, this);
				element.DrawImageView(cell.ImageView);
			}
		}
	}
}

