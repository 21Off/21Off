
using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.Dialog;
using System.Drawing;

namespace MSP.Client
{
	public partial class MeViewCont : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public MeViewCont (IntPtr handle) : base(handle)
		{
			Initialize ();
		}

		[Export("initWithCoder:")]
		public MeViewCont (NSCoder coder) : base(coder)
		{
			Initialize ();
		}

		public MeViewCont () : base("MeViewCont", null)
		{
			Initialize ();
		}

		void Initialize ()
		{
			this.root = CreateRoot ();		
		}
		
		#endregion
		
		private RootElement root;
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();						
			
			var dv = new DialogViewController (root, true);
			
			// When the view goes out of screen, we fetch the data.
			dv.ViewDissapearing += delegate {

			};
			
			dv.View.Frame = new System.Drawing.RectangleF(0, 60, 320, 480 - 60);
			
			this.View.AddSubview(dv.View);				
		}		
		
		RootElement CreateRoot ()
		{
			return new RootElement ("Settings") {
				new Section (){
					new StringElement ("mes photos"
					                 /*, el => 
	                 	{				
							int userId = AppDelegateIPhone.AIphone.MainUser.Id;
							return new MembersPhotoViewControler(AppDelegateIPhone.meNavigationController, userId);
						}
						*/, ()=>
	                   {
							Action act = ()=>
							{
								int userId = AppDelegateIPhone.AIphone.MainUser.Id;
								var m = new MembersPhotoViewControler(AppDelegateIPhone.meNavigationController, userId, false);
								//this.PresentModalViewController(m, true);
								AppDelegateIPhone.meNavigationController.PushViewController(m, false);
							
								var view = new UIView();
								var btn = UIButton.FromType(UIButtonType.Custom);
								btn.Frame = new RectangleF(0, -13, 26, 26);
								btn.SetBackgroundImage(Graphics.GetImgResource("back"), UIControlState.Normal);								
								view.Add(btn);
								
							};
							AppDelegateIPhone.ShowRealLoading(View, "Loading photos", null, act);
						}
					),
					
					new StringElement ("mon profile"),					
					new StringElement("mes autres reseaux"),
				}
			};
		}
	}
}

