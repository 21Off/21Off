using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public class My : UINavigationBarDelegate
	{
		public My():base()
		{
		}
		
		public override void DidPopItem (UINavigationBar navigationBar, UINavigationItem item)
		{
			// TODO: Implement - see: http://go-mono.com/docs/index.aspx?link=T%3aMonoTouch.Foundation.ModelAttribute
		}
		
		public override void DidPushItem (UINavigationBar navigationBar, UINavigationItem item)
		{
			// TODO: Implement - see: http://go-mono.com/docs/index.aspx?link=T%3aMonoTouch.Foundation.ModelAttribute
		}
		
		public override bool ShouldPopItem (UINavigationBar navigationBar, UINavigationItem item)
		{
			// TODO: Implement - see: http://go-mono.com/docs/index.aspx?link=T%3aMonoTouch.Foundation.ModelAttribute
			
			return true;
		}
		
		public override bool ShouldPushItem (UINavigationBar navigationBar, UINavigationItem item)
		{
			// TODO: Implement - see: http://go-mono.com/docs/index.aspx?link=T%3aMonoTouch.Foundation.ModelAttribute
			return true;
		} 
	}

	public partial class MeNavController : UINavigationController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public MeNavController (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public MeNavController (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public MeNavController () : base("MeNavController", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
			//this.NavigationBar.Delegate	= new My();
			cm = new CustomNavigationBar();		
		}		
		
		private CustomNavigationBar cm;
		
		
		public override UINavigationBar NavigationBar {
			get {
				return cm;
			}
		}
		
		#endregion
	}
}

