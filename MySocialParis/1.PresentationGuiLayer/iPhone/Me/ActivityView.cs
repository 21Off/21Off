using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;

namespace MSP.Client
{
	public class ActivityView : BaseTimelineViewController
	{
		public MSPNavigationController MSPNavigationController {get;set;}
		
		public ActivityView (bool pushing) : base(pushing)
		{
			this.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			this.TableView.BackgroundColor = new UIColor (0, 0, 0, 0);
			this.TableView.AllowsSelection = false;
			
			ReloadTimeline();
		}
		
		public override void ReloadTimeline ()
		{			
			ThreadPool.QueueUserWorkItem(o => DownloadTweets ());
		}
		
		void GoToMembersPhotoView (int userId)
		{
			Action act = ()=>
			{
				InvokeOnMainThread(()=>
				{
					var a = new MembersPhotoViewControler(MSPNavigationController, userId, false);
					MSPNavigationController.PushViewController(a, false);
					//MSPNavigationController.PresentModalViewController (a, true);
				});
			};			
			AppDelegateIPhone.ShowRealLoading(View, "Loading photos", null, act);				
		}
		
		void GoToPhotoDetailsView (UIActivity activity)
		{			
			Image image = activity.Image;
			int imgUserId = activity.Image.UserId;			
			int askerId = AppDelegateIPhone.AIphone.MainUser.Id;
			
			FullUserResponse user =  imgUserId == askerId 
				? new FullUserResponse()
				{ 
					User = AppDelegateIPhone.AIphone.MainUser 
				}
				: AppDelegateIPhone.AIphone.UsersServ.GetFullUserById(imgUserId, askerId);
			
			Action act = ()=>
			{
				try
				{
					this.InvokeOnMainThread(()=>
					{
						var a = new PhotoDetailsViewController(MSPNavigationController, user, image, true);
						MSPNavigationController.PresentModalViewController (a, true);
					});
				}
				catch (Exception ex)
				{
					Util.ShowAlertSheet(ex.Message, View);
					Util.LogException("GoToPhotoDetailsView", ex);
					return;
				}
			};
			AppDelegateIPhone.ShowRealLoading(View, "Loading photo details", null, act);
		}
			
		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			if (indexPath.Row == Root[0].Count - 1)
				return 60 + 10;
			
			var element = Root[0].Elements [indexPath.Row];
		
			var sizable = element as ActivityElement;
			if (sizable == null)
				return tableView.RowHeight;
			return ActivityCell.GetCellHeight(new System.Drawing.RectangleF(0, 0, 320, 10), sizable.activity);
		}
		
		List<UIActivity> GetActivities(DateTime since)
		{
			int userId = AppDelegateIPhone.AIphone.MainUser.Id;
									
			var activities = new List<UIActivity>();			
			var activityResponses = new List<ActivityResponse>();
			
			var servActivityResponses = AppDelegateIPhone.AIphone.ActivServ.GetActivities(userId, since);
			if (servActivityResponses != null)
			{
				foreach (ActivityResponse actResp in servActivityResponses)
					activityResponses.Add(actResp);
			}
								
			foreach(ActivityResponse actResp in activityResponses)
			{				
				var activity = new UIActivity()
                { 
					Id = actResp.Activity.Id,
					DbActivity = actResp.Activity,
					User = actResp.User,
					Image = actResp.Image,
					Type =  (ActivityType)Enum.ToObject(typeof(ActivityType), actResp.Activity.Type),
				};				
				
				if (activity.Type == ActivityType.PhotoShare)
				{
					activity.Text = " shared a photo";
				}					
				if (activity.Type == ActivityType.PhotoComment)
				{
					activity.Text = actResp.Comment.Name;
				}
				if (activity.Type == ActivityType.UserFollow)
				{
					activity.Text = " follows you";
				}
				if (activity.Type == ActivityType.PhotoLike)
				{
					activity.Text = " liked your post";
				}
						
				activities.Add(activity);				
			}
			
			return activities;
		}
		
		private DateTime sinceDate = DateTime.MaxValue;
		
		void AddOlderActivities()
		{
			List<UIActivity> activities = GetActivities(sinceDate);
			if (activities != null)
			{
				if (activities.Count > 0)
				{
					sinceDate = activities[activities.Count - 1].DbActivity.Time;
					var newElements = new List<ActivityElement>();
					foreach (UIActivity activity in activities)
					{
						newElements.Add(new ActivityElement(activity, GoToMembersPhotoView, GoToPhotoDetailsView));
					}
					this.BeginInvokeOnMainThread (delegate {
						Root[0].Insert(Root[0].Count - 1, UITableViewRowAnimation.None, newElements.ToArray());
					});
				}
			}
		}
		
		void DownloadTweets ()
		{
			try
			{
				sinceDate = DateTime.MaxValue;
				List<UIActivity> activities = GetActivities(sinceDate);
				if (activities == null)
				{
					this.BeginInvokeOnMainThread (delegate { ReloadComplete (); });
					return;
				}
				
				long ticks = DateTime.UtcNow.Ticks;				
				Util.Defaults.SetDouble (ticks, "LastUpdate");	
				
				if (activities.Count > 0)
					sinceDate = activities[activities.Count - 1].DbActivity.Time;
				
				this.BeginInvokeOnMainThread (delegate {
					AppDelegateIPhone.navigationRoots[2].TabBarItem.BadgeValue = null;
					
					Root[0].RemoveRange(0, Root[0].Count);
					//NSTimer.CreateScheduledTimer (0, delegate {
						foreach (UIActivity activity in activities)
						{
							Root[0].Add(new ActivityElement(activity, GoToMembersPhotoView, GoToPhotoDetailsView));
						}
					
						CustomLoadMoreElement more = null;
						more = new CustomLoadMoreElement (delegate 
							{
							// Launch a thread to do some work
							ThreadPool.QueueUserWorkItem (delegate {
									AddOlderActivities();
									BeginInvokeOnMainThread(()=>
									{
										more.Animating = false;
									});
								});
							});
						
						more.Height = 60;						
						more.Image = Graphics.GetImgResource("more");						
						
						try {
							Root[0].Insert (Root[0].Count, UITableViewRowAnimation.None, more);
						} catch {
						}					
						
						// Notify the dialog view controller that we are done
						// this will hide the progress info				
						this.BeginInvokeOnMainThread (delegate { ReloadComplete (); });
					});
				//});
			}
			catch (Exception ex)
			{
				this.InvokeOnMainThread(ReloadComplete);
				Util.LogException("ActivityView ReloadTime", ex);
			}
		}
	}	
	
}

