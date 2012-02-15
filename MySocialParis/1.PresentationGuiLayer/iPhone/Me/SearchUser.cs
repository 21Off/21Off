using System;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Drawing;

namespace MSP.Client
{			
	public class SearchUser : SearchDialog {
		
		public SearchUser()
		{
			TableView.BackgroundView = new UIImageView(UIImage.FromBundle("Images/Ver4/fond"));			
		}
		
		public override SearchMirrorElement MakeMirror ()
		{
			return new SearchMirrorElement (Locale.GetText ("Search for user `{0}'"));
		}
		
		public void Reset()
		{
			if (Root.Count > 0)
				Root[0].Clear();
			
			FinishSearch();	
			
			//SearchMirror = MakeMirror ();
			var entries = new Section () {
				//SearchMirror
			};
						
			PopulateSearch (entries);
			
			Root = new RootElement (Locale.GetText ("Search")){
				entries,
			};
			
			ReloadData();
			//StartSearch ();
			//PerformFilter ("");
		}
		
		public override void SearchButtonClicked (string text)
		{	
			if (string.IsNullOrWhiteSpace(text))
				return;
			
			if (Root.Count > 0)
				Root[0].Clear();
			
			FinishSearch();	
			
			var lw = new LoadingView();
			
			lw.Show("Searching for " + text);
			
			NSTimer.CreateScheduledTimer (TimeSpan.FromSeconds (0.2), delegate {
				
				SearchMirror.Text = text;
				var entries = new Section () {
				};
				
				var users = AppDelegateIPhone.AIphone.UsersServ.GetAllUsersByName(SearchMirror.Text);
				if (users != null)				
				{
					foreach (var user in users)
					{		
						entries.Add (new UserElementII(user));
					}
				}
				
				Root = new RootElement (Locale.GetText ("Search")){
					entries,
				};
				
				ReloadData();
				lw.Hide();
				
				if (users.Count == 0)
					AppDelegateIPhone.ShowMessage(View, "No match found", null, 2);
			});
		}
				
		public override void PopulateSearch (Section entries)
		{
			return;
			var users = AppDelegateIPhone.AIphone.UsersServ.GetAllUsersByName(SearchMirror.Text);
			if (users == null)
				return;
			
			foreach (var user in users)
			{		
				entries.Add (new UserElementII(user));
			}
		}
		
		public override void Selected (NSIndexPath indexPath)
		{
			var uinav = (UINavigationController) AppDelegateIPhone.tabBarController.SelectedViewController;
			//GetItemText (indexPath);
			
			var element = Root [0][indexPath.Row] as UserElementII;
			if (element == null)
				return;
		
			Action act = () =>
			{
				InvokeOnMainThread(()=>
				{
					var membersPhotoView = new MembersPhotoViewControler (uinav, element.User.Id, false);
										
					uinav.PushViewController(membersPhotoView, true);
					//ActivateController (membersPhotoView);
				});
			};
			AppDelegateIPhone.ShowRealLoading(null, "Loading user photos", null, act);
		}
	}
}
