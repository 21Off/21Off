using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Net;
using System.Drawing;
using MonoTouch.ObjCRuntime;

namespace MSP.Client
{
	public class UILoadingView : UIView
	{
		UILabel loadingMessageLabel;
		string loadingMessage;
		UIImageView overlayBackground;
		UIActivityIndicatorView activityIndicator;

		public UILoadingView (IntPtr handle) : base(handle)
		{
			Initialize ();
		}
 
		[Export("initWithCoder:")]
		public UILoadingView (NSCoder coder) : base(coder)
		{
			Initialize ();
		}
 
 		void Initialize ()
		{
			Initialize("Loading...");	
		}
		
		void Initialize (string message)
		{
			//using (var pool = new NSAutoreleasePool())
			//{
				SetUpLoadingMessageLabel(message);
				SetUpActivityIndicator();
				SetUpOverlayBackground();
			
				this.AddSubview(overlayBackground);
				this.AddSubview(activityIndicator);
				this.AddSubview(loadingMessageLabel);
			//}
		}

		void SetUpOverlayBackground ()
		{
			overlayBackground = new UIImageView(new RectangleF(0f, 0f, 320f, 460f));
			overlayBackground.BackgroundColor = new UIColor(0f, 0f, 0f,0.75f);
		}


		void SetUpActivityIndicator ()
		{
			activityIndicator = new UIActivityIndicatorView(new RectangleF(150f, 220f, 20f, 20f));
			activityIndicator.StartAnimating();          
		}


		void SetUpLoadingMessageLabel (string message)
		{
			// Set up loading message - Positioned Above centre in the middle
			loadingMessageLabel = new UILabel(new RectangleF(53f, 139f, 214f, 62f));
			loadingMessageLabel.BackgroundColor = UIColor.Clear;
			loadingMessageLabel.AdjustsFontSizeToFitWidth = true;
			loadingMessageLabel.TextColor = UIColor.White;
			loadingMessageLabel.TextAlignment = UITextAlignment.Center;
			loadingMessageLabel.Lines = 3;
			loadingMessageLabel.Text = message;
			loadingMessageLabel.Font = UIFont.BoldSystemFontOfSize(16f);
		}

 
		public UILoadingView (string message)
		{
			Initialize(message);
		}
		
		public override void WillRemoveSubview (UIView uiview)
		{
			activityIndicator.StopAnimating();
		}
		
		public void FadeOutAndRemove()
		{
			InvokeOnMainThread( delegate { 
			Util.Log ("Fade out loading screen...");
				UIView.BeginAnimations("FadeOutLoadingView");
				UIView.SetAnimationDuration(0.5f);
				UIView.SetAnimationTransition(UIViewAnimationTransition.None, this, true);
				UIView.SetAnimationDidStopSelector(new Selector("FadeOutLoadingViewDone"));
			    this.Alpha = 0f;
				UIView.CommitAnimations();	
			});
		}
		
		void FadeOutLoadingViewDone()
		{ 
				Util.Log ("RemoveFromSuperview...");
				this.RemoveFromSuperview();
		}

	}
}
