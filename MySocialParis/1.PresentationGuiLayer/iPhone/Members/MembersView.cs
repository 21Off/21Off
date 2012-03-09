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
	public class MembersView : BaseTimelineViewController
	{		
		private int _UserID;
		
		public MembersView (bool pushing, int userID) : base(pushing)
		{
			this.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			this.TableView.BackgroundColor = new UIColor (0, 0, 0, 0);
			this.TableView.AllowsSelection = false;
			
			_UserID = userID;
			
			ReloadTimeline();
		}

		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
						
			ReloadTimeline ();
		}

		public override void ReloadTimeline ()
		{
			if (_UserID == 0)
			{
				ReloadComplete();
				return;
			}		
			
			ThreadPool.QueueUserWorkItem(o => DownloadTweets ());
		}				
			
		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			var element = Root[0].Elements [indexPath.Row];
		
			var sizable = element as MemberPhotoElement;
			if (sizable == null)
				return tableView.RowHeight;
			return sizable.GetHeight (tableView, indexPath);
		}

		private List<Tweet> GetTweets(User user, DateTime since)
		{
			var tweets = new List<Tweet>();
			var imagesTimedResp = AppDelegateIPhone.AIphone.ImgServ.GetFullImagesOfUser(_UserID, since);
			if (imagesTimedResp == null)
			{
				return null;
			}
			
			oldestTime = imagesTimedResp.Time.HasValue ? imagesTimedResp.Time.Value : DateTime.MaxValue;
			
			foreach (var image in imagesTimedResp.Images)
			{				
				List<Comment> comments = new List<Comment>();
				List<User> commentsUsers = new List<User>();
				foreach (CommentResponse cresp in image.Comments)
				{
					comments.Add(cresp.Comment);
					commentsUsers.Add(cresp.User);
				}
								
				var tweet = new Tweet()
				{
					Image = image.Image,
					User = user,
					Keywords = image.Keywords,
					Comments = comments,
					
					CommentsUsers = commentsUsers,
					LikesCount = image.LikesCount,
						
					DeleteAction = DeleteAction,
					UrlTapAction = UrlTap,
				};
				tweets.Add(tweet);
			}
			return tweets;
		}
		
		private void UrlTap(string url)
		{			
			WebViewController.OpenUrl (this, url); 
			
			//InvokeOnMainThread(()=> nav.PushViewController(a, false));
		}
		
		private void AddOlderPhotos(LoadMoreElement more)
		{
			try
			{
				var tweets = GetTweets(photoUser, oldestTime);
				if (tweets != null && tweets.Count > 0)
				{
					var newElements = new List<MemberPhotoElement>();
					foreach (Tweet tweet in tweets)
					{
						newElements.Add(new MemberPhotoElement(tweet, GoToUserPhotos));
					}
					this.BeginInvokeOnMainThread (delegate {
						more.Animating = false;
						Root[0].Insert(Root[0].Count - 1, UITableViewRowAnimation.None, newElements.ToArray());
					});
				}
			}
			catch (Exception ex)
			{
				Util.LogException("AddOlderPhotos", ex);
			}
		}
		
		private DateTime oldestTime = DateTime.MaxValue;
		private User photoUser;
		
		private void DownloadTweets ()
		{
			oldestTime = DateTime.MaxValue;
			try
			{
				photoUser = photoUser ??  AppDelegateIPhone.AIphone.UsersServ.GetUserById(_UserID);
				if (photoUser == null)
				{
					BeginInvokeOnMainThread(() => ReloadComplete());
					return;											
				}
				
				var tweets = GetTweets(photoUser, DateTime.MaxValue);
				
				if (tweets == null)
				{
					BeginInvokeOnMainThread(delegate 
					{
						ReloadComplete ();
						AppDelegateIPhone.ShowMessage(View, "image stream loading failed", null);
					});
					return;
				}				
				        			
				this.BeginInvokeOnMainThread (delegate {
					Root[0].RemoveRange(0, Root[0].Count);											
									
					NSTimer.CreateScheduledTimer (0, delegate {
						try
						{
							foreach (Tweet tweet in tweets)
							{
								Root[0].Add(new MemberPhotoElement(tweet, GoToUserPhotos));
							}
						}
						catch (Exception ex)
						{
							Util.LogException("DownloadTweets", ex);
						}	
						
						LoadMoreElement more = null;
						more = new LoadMoreElement (delegate {
						
							// Launch a thread to do some work
							ThreadPool.QueueUserWorkItem (delegate {
									AddOlderPhotos(more);								
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
				});
			}
			catch (Exception ex)
			{
				Util.LogException("MembersView ReloadTimeline", ex);
				BeginInvokeOnMainThread(() => ReloadComplete());
			}
		}
		
		private void GoToUserPhotos(int userId)
		{			
			if (userId == AppDelegateIPhone.AIphone.MainUser.Id)
				return;
			
			Action act = ()=>
			{	
				InvokeOnMainThread(()=>
				{
					var nav = AppDelegateIPhone.tabBarController.SelectedViewController as UINavigationController;
					var a = new MembersPhotoViewControler(nav, userId, false);
					
					InvokeOnMainThread(()=> nav.PushViewController(a, false));
				});
			};			
			AppDelegateIPhone.ShowRealLoading(View, "Loading photos", null, act);			
		}
		
		private void DeleteAction(Tweet tweet)
		{			
			bool isPhotoOwner = tweet.Image.UserId == AppDelegateIPhone.AIphone.MainUser.Id;
			
			var section = Root[0];
			for (int i = 0; i < section.Elements.Count; i++)
			{
				var element = (MemberPhotoElement)section[i];
				if (element.Tweet == tweet)
				{
					if (isPhotoOwner)
					{
						AppDelegateIPhone.AIphone.ImgServ.DeleteImage(tweet.Image);
						
						ImageStore.DeleteDBPic(tweet.Image.Id, tweet.Image.UserId, SizeDB.Size100);
						ImageStore.DeleteDBPic(tweet.Image.Id, tweet.Image.UserId, SizeDB.Size50);
						ImageStore.DeleteDBPic(tweet.Image.Id, tweet.Image.UserId, SizeDB.Size75);
						ImageStore.DeleteDBPic(tweet.Image.Id, tweet.Image.UserId, SizeDB.SizeFull);
						
						section.Remove(element);
					}
					break;
				}
			}			
		}
	}
}
