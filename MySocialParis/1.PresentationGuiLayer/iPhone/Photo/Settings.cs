using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System;
using Twitter;

namespace MSP.Client
{
	// 
	// The view controller for our settings, it overrides the source class so we can
	// make it editable
	//
	public class Settings : DialogViewController {
		DialogViewController parent;
		
		// 
		// This source overrides the EditingStyleForRow to enable editing
		// of the table.   The editing is triggered with a button on the navigation bar
		//
		class MySource : Source {
			DialogViewController parent;
			
			public MySource (DialogViewController parent) : base (parent)
			{
				this.parent = parent;
			}
			
			public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return UITableViewCellEditingStyle.None;
			}
			
			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, 
			                                         NSIndexPath indexPath)
			{
			}
		}
		
		public override Source CreateSizingSource (bool unevenRows)
		{
			return new MySource (this);
		}
		
		public override void ViewWillAppear (bool animated)
		{

		}
			
		public CheckboxElement facebook, twitter;
		private MSPNavigationController msp;
		
		/*
		public override void ViewWillDisappear (bool animated)
		{
			base.ViewWillDisappear (animated);
			
			Util.Defaults.SetInt (facebook.Value ? 0 : 1, "facebook");			
			Util.Defaults.SetInt (twitter.Value ? 0 : 1, "twitter");
			Util.Defaults.Synchronize ();
		}
		*/
		
		public TwitterApplication twitterApp;
		public FaceBook.FaceBookApplication facebookApp;
		
		public Settings () : base (UITableViewStyle.Grouped, null)
		{
			msp = AppDelegateIPhone.tabBarController.SelectedViewController as MSPNavigationController;
			msp.OnViewAppeared += HandleMspOnViewAppeared;
			
			twitterApp = new Twitter.TwitterApplication(msp);
			facebookApp = new FaceBook.FaceBookApplication(msp);
			
			bool twitterLoggedIn = twitterApp.LoggedIn();
			bool facebookLoggedIn = facebookApp.LoggedIn();			
			
			Util.Defaults.SetInt (twitterLoggedIn ? 1 : 0, "twitterOption");
			Util.Defaults.SetInt (facebookLoggedIn ? 1 : 0, "facebookOption");
			
			Root = new RootElement (Locale.GetText ("Settings"))
			{
				new Section (Locale.GetText ("share your post on:")){
					(facebook = new CheckboxElement (Locale.GetText ("Facebook"), facebookLoggedIn)),
					(twitter = new CheckboxElement(Locale.GetText ("Twitter"),  twitterLoggedIn)),
				}							
			};
			
			facebook.ValueChanged += (sender, e) => 
			{
				if (!facebookApp.LoggedIn())
				{
					//facebookApp.Login();
					var _MSP = AppDelegateIPhone.tabBarController.SelectedViewController as UINavigationController;
					var vc = new SocialNetworksParentViewController(_MSP);
					_MSP.PushViewController(vc, true);
					return;
				}
				
				facebook.SetValue(facebook.Value);
				
				Util.Defaults.SetInt (facebook.Value ? 1 : 2, "facebookOption");
				Util.Defaults.Synchronize ();
			};
			twitter.ValueChanged += (sender, e) => 
			{
				if (!twitterApp.LoggedIn())
				{					
					//twitterApp.Login();
					var _MSP = AppDelegateIPhone.tabBarController.SelectedViewController as UINavigationController;
					var vc = new SocialNetworksParentViewController(_MSP);
					_MSP.PushViewController(vc, true);
					return;
				}
				
				twitter.SetValue(twitter.Value);
				
				Util.Defaults.SetInt (twitter.Value ? 1 : 2, "twitterOption");
				Util.Defaults.Synchronize ();
			};
		}

		void HandleMspOnViewAppeared (object sender, EventArgs e)
		{
			if (Util.Defaults.IntForKey ("twitterOption") != 2)
			{
				twitter.SetValue(twitterApp.LoggedIn());
				Util.Defaults.SetInt (twitter.Value ? 0 : 1, "twitter");
				Util.Defaults.SetInt (twitter.Value ? 1 : 0, "twitterOption");
			}
			
			if (Util.Defaults.IntForKey ("facebookOption") != 2)
			{
				facebook.SetValue(facebookApp.LoggedIn());
				Util.Defaults.SetInt (facebook.Value ? 0 : 1, "facebook");
				Util.Defaults.SetInt (facebook.Value ? 1 : 0, "facebookOption");
			}
			
			Util.Defaults.Synchronize ();
		}
	}
}

