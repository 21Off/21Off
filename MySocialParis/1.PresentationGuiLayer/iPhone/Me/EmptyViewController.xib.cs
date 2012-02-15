using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;

namespace MSP.Client
{
	public partial class EmptyViewController : UIViewController
	{
		#region Constructors
		
		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code
		
		public EmptyViewController (IntPtr handle) : base (handle)
		{
			
		}
		
		[Export ("initWithCoder:")]
		public EmptyViewController (NSCoder coder) : base (coder)
		{
			
		}
		
		public EmptyViewController (Action onBack, string title) : base ("EmptyViewController", null)
		{
			this.OnBack = onBack;
			this._title = title;
		}
		
		#endregion
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			this.backBtn.TouchDown += (sender, e) => 
			{
				if (OnBack != null) 
					OnBack();
			};
			this.titleLbl.Text = _title;
			
			UIUtils.SetTitle(_title, null, titleLbl, null);
		}
		
		private string _title;
		private Action OnBack; 
	}
}

