using System;
using System.Collections.Generic;
using System.Threading;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;

namespace MSP.Client
{
	public class PhotoDetailsView : BaseTimelineViewController
	{
		private Image _Image;
		private User _ImageOwner;
		
		public PhotoDetailsView (bool pushing, Image image, User imageOwner) : base(pushing)
		{
			this.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			this.TableView.BackgroundColor = new UIColor (0, 0, 0, 1);
			this.TableView.AllowsSelection = false;
			
			_Image = image;
			_ImageOwner = imageOwner;
			
			ReloadTimeline ();
		}

		public override void ReloadTimeline ()
		{			
			if (_Image == null)
			{
				this.InvokeOnMainThread (delegate { ReloadComplete (); });
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

		void DownloadTweets ()
		{	
			try
			{				
				var imageResponse = AppDelegateIPhone.AIphone.ImgServ.GetFullImage(_Image.Id);
				if (imageResponse == null)
				{					
					this.BeginInvokeOnMainThread (delegate { ReloadComplete (); });
					return;
				}				
				
				List<Comment> comments = new List<Comment>();
				List<User> commentsUsers = new List<User>();
				foreach (CommentResponse cresp in imageResponse.Comments)
				{
					comments.Add(cresp.Comment);
					commentsUsers.Add(cresp.User);
				}								
				
				this.BeginInvokeOnMainThread (delegate {
					try					
					{		
						DownloadAsync(imageResponse, comments, commentsUsers);
					}		
					catch (Exception ex)
					{
						Util.LogException("PhotoDetailsView DownloadAsync", ex);
						this.BeginInvokeOnMainThread (delegate { ReloadComplete (); });
					}
				});
			}
			catch (Exception ex)
			{				
				Util.ShowAlertSheet(ex.Message, View);
				this.BeginInvokeOnMainThread (delegate { ReloadComplete (); });
			}			
		}
		
		private void DownloadAsync(ImageResponse imageResponse, List<Comment> comments, List<User> commentsUsers)
		{
			while (Root[0].Count > 0)
				Root[0].Remove (0);				
			
			NSTimer.CreateScheduledTimer (0.1, delegate {
				try
				{			
					var mPhotoEl = new MemberPhotoElement(new Tweet()
		            { 
						User = _ImageOwner,
						Image = _Image,
						Keywords = imageResponse.Keywords,
						
						Comments = comments,												
						CommentsUsers = commentsUsers,
						
						LikesCount = imageResponse.LikesCount,
						DeleteAction = DeleteAction,
						UrlTapAction = UrlTap,
					}, GoToUserPhotos);
					
					Root[0].Add(mPhotoEl);
					
				}
				catch (Exception ex)
				{
					Util.LogException("CreateScheduledTimer PhotoDetailsView", ex);
				}
				
				// Notify the dialog view controller that we are done
				// this will hide the progress info				
				this.BeginInvokeOnMainThread (delegate { ReloadComplete (); });
			});
		}
		
		private void UrlTap(string url)
		{			
			WebViewController.OpenUrl (this, url);
		}
		
		private void GoToUserPhotos(int userId)
		{
			Action act = ()=>
			{					
				var nav = AppDelegateIPhone.tabBarController.SelectedViewController as UINavigationController;
				var a = new MembersPhotoViewControler(nav, userId, false);
				InvokeOnMainThread(()=> nav.PushViewController(a, false));
			};			
			AppDelegateIPhone.ShowRealLoading(View, "Loading photos", null, act);			
		}
		
		private void DeleteAction(Tweet tweet)
		{			
			bool isPhotoOwner = tweet.Image.UserId == AppDelegateIPhone.AIphone.GetMainUserId();
			
			var section = Root[0];
			for (int i = 0; i < section.Elements.Count; i++)
			{
				var element = (MemberPhotoElement)section[i];
				if (element.Tweet == tweet)
				{
					if (isPhotoOwner)
					{
						section.Remove(element);
						AppDelegateIPhone.AIphone.ImgServ.DeleteImage(tweet.Image);
						DbImageStore.DeleteDBPic(tweet.Image.Id, tweet.Image.UserId, SizeDB.Size100);
						DbImageStore.DeleteDBPic(tweet.Image.Id, tweet.Image.UserId, SizeDB.Size50);
						DbImageStore.DeleteDBPic(tweet.Image.Id, tweet.Image.UserId, SizeDB.Size75);
						DbImageStore.DeleteDBPic(tweet.Image.Id, tweet.Image.UserId, SizeDB.SizeFull);
						
						var nav = AppDelegateIPhone.tabBarController.SelectedViewController as UINavigationController;
						nav.PopViewControllerAnimated(true);
					}
					break;
				}
			}			
		}		
	}
}
