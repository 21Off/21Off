using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Drawing;

namespace MSP.Client
{
	public partial class RelationsViewController : UIViewController
	{
		#region Constructors
		
		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code
		
		public RelationsViewController (IntPtr handle) : base (handle)
		{

		}
		
		[Export ("initWithCoder:")]
		public RelationsViewController (NSCoder coder) : base (coder)
		{
		}
		
		public RelationsViewController (UINavigationController nav, string title, string subTitle, RootElement root) 
			: base ("RelationsViewController", null)
		{
			this.root = root;
			this.nav = nav;
			titleText = title;
			subTitleText = subTitle;
			
			/*
			foreach (var el in root[0])
			{
				if (el is UserElementII)
				{
					var uel = (UserElementII)el;
					uel.OnEntered += Entered;					
				}
			}
			*/
		}
		
		#endregion
		
		private RootElement root;
		private UINavigationController nav;
		private string titleText;
		private string subTitleText;
		
		void Entered()
		{
			lineview.Hidden = true;
		}
		
		void Initialize ()
		{
			backBtn.TouchDown+= HandleBackBtnTouchDown;
			
			titleBtn.Text = titleText;
			subTitleBtn.Text = subTitleText;
		}
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			Initialize ();
			
			var rv = new RelationsView(root, false);
			rv.View.Frame = new RectangleF(0, 40, 320, 480 - 40);
			
			UIViewExtensions.SetTitleText(titleText, subTitleText, titleBtn, subTitleBtn);					
			
			this.View.Add(rv.View);
			
			lineview = new UIView(new RectangleF(0, 35 , 320, 1));
			lineview.Layer.BackgroundColor = UIColor.LightGray.CGColor;
			this.View.AddSubview(lineview);
		}
		
		private UIView lineview;

		void HandleBackBtnTouchDown (object sender, EventArgs e)
		{
			nav.PopViewControllerAnimated(true);
		}		
	}
}

