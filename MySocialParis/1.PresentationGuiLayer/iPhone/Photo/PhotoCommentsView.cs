using System;
using System.Threading;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;
using System.Linq;
using System.Drawing;

namespace MSP.Client
{	
	public class UIComment
	{
		public Comment Comment {get;set;}
		public User PhotoOwner {get;set;}
		public User CommentOwner {get;set;}
	}
	
	public class PhotoCommentsView : BaseTimelineViewController				
	{
		private int _ImageID;
		private User _photoOwner;
		
		public PhotoCommentsView (bool pushing, int imageID, User photoOwner) : base(pushing)
		{
			_ImageID = imageID;
			_photoOwner = photoOwner;
			
			this.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			this.TableView.BackgroundColor = new UIColor (0, 0, 0, 0);
			this.TableView.AllowsSelection = false;		
			
			var rootElement = new RootElement ("timelineTitle") { UnevenRows = true };
			var section = new Section();
			rootElement.Add(section);
			this.Root = rootElement;			

			ReloadTimeline();
		}
		
		#region Overrides
		
		public override Source CreateSizingSource (bool unevenRows)
		{
			return new EditingSource (this);
		}		
		
		public override void ReloadTimeline ()
		{
			if (_ImageID == 0)
			{
				ReloadComplete();
				return;
			}
			
			ThreadPool.QueueUserWorkItem(o => DownloadTweets ());
		}		
		
		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			var element = Root[0].Elements [indexPath.Row];
		
			var sizable = element as CommentElement;
			if (sizable == null)
				return 60;
			
			return CommentCell.GetCellHeight(new RectangleF(0, 0, 320, 10), sizable._comment);
		}
		
		#endregion
		
		#region Private methodes

		private void DownloadTweets ()
		{
			try
			{
				var comments = AppDelegateIPhone.AIphone.CommentServ.GetCommentsOfImage(_ImageID);
				comments = comments.OrderByDescending(c => c.Time);
				
				this.BeginInvokeOnMainThread (delegate {
					while (Root[0].Count > 0)
						Root[0].Remove (0);
					
					NSTimer.CreateScheduledTimer (0.1, delegate {
						
						if (AppDelegateIPhone.AIphone.MainUser != null)
							Root[0].Add(CreateLoadMore(Root[0]));
						
						foreach (Comment comment in comments)
						{
							var uicomment = new UIComment()
							{
								Comment = comment,
								PhotoOwner = _photoOwner,
							};
							Root[0].Add(new CommentElement(uicomment));
						}
						
						// Notify the dialog view controller that we are done
						// this will hide the progress info				
						this.BeginInvokeOnMainThread (delegate { ReloadComplete (); });
					});
				});
			}
			catch (Exception ex)			
			{
				this.BeginInvokeOnMainThread (delegate { ReloadComplete (); });				
				Util.LogException("Download comments", ex);
			}
		}	
		
		/// <summary>
		/// Called only on user authentification
		/// </summary>
		private void AddNewCommentAsync(AddLoadMoreWithImageElement lme, Section section)
		{
			try
			{
				var comment = new Comment()
				{
					Name = lme.Value,
					ParentId = _ImageID,
					UserId = AppDelegateIPhone.AIphone.MainUser.Id,
					Time = DateTime.UtcNow,
				};
				
				AppDelegateIPhone.AIphone.CommentServ.PutNewComment(comment);
			
				// Now make sure we invoke on the main thread the updates
				this.BeginInvokeOnMainThread(delegate {
					lme.Animating = false;
											
					var uicomment = new UIComment()
					{
						Comment = comment,
						PhotoOwner = _photoOwner,
					};
					
					var act = new CommentElement(uicomment);
					section.Insert(1, act);
					lme.Value = null;							
				});
			}
			catch (Exception ex)
			{
				Util.LogException("Add comment", ex);
			}
		}
		
		/// <summary>
		/// Called only on user authentification
		/// </summary>
		private Element CreateLoadMore(Section section)
		{
           var loadMore2 = new AddLoadMoreWithImageElement("Add comment", ImageStore.DefaultImage, lme => {
				lme.FetchValue();
				if (!string.IsNullOrWhiteSpace(lme.Value))
				{			
					// Launch a thread to do some work
					ThreadPool.QueueUserWorkItem (delegate {
							AddNewCommentAsync(lme, section);
					});
				}
				else
					lme.Animating = false;
				
			});
			
			return loadMore2;
		}
				
		#endregion
	}				
}
