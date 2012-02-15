using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using TweetStation;
using System;
using MSP.Client.DataContracts;
using System.Drawing;
using MonoTouch.CoreGraphics;

namespace MSP.Client
{	
	public class PhotoLikersView : DialogViewController				
	{		
		public override Source CreateSizingSource (bool unevenRows)
		{
			return new EditingSource (this);
		}		
		
		private Image _photo;
		private User _photoOwner;
		
		public PhotoLikersView (bool pushing, Image photo, User photoOwner) : base(null, pushing)
		{
			Autorotate = false;
			
			RefreshRequested += delegate { ReloadTimeline (); };
			Style = UITableViewStyle.Plain;
			
			this.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			this.TableView.BackgroundColor = new UIColor (0, 0, 0, 0);
			this.TableView.AllowsSelection = false;
			this.TableView.BackgroundView = new UIImageView(Graphics.GetImgResource("fond"));
			
			var view = new UIView(new RectangleF(0, 0, 320, 1));
			view.Layer.BackgroundColor = UIColor.LightGray.CGColor;
			this.View.AddSubview(view);			

			
			var rootElement = new RootElement ("PhotoLikers") { UnevenRows = true };
			var section = new Section();
			rootElement.Add(section);
			this.Root = rootElement;			
			
			_photo = photo;	
			_photoOwner = photoOwner;
			
			ReloadTimeline();
		}

		public void ReloadTimeline ()
		{			
			ThreadPool.QueueUserWorkItem(o => DownloadTweets());
		}				

		void DownloadTweets ()
		{
			try
			{
				bool isMyPost = AppDelegateIPhone.AIphone.MainUser.Id == _photo.UserId;
				
				var fullLikes = AppDelegateIPhone.AIphone.LikesServ.GetFullLikesOfImage(_photo.Id);
				if (fullLikes == null)
				{
					this.BeginInvokeOnMainThread (delegate { ReloadComplete (); });
					return;					
				}
				
				fullLikes = fullLikes.OrderByDescending(c => c.Like.Time);
				
				this.BeginInvokeOnMainThread (delegate {
					while (Root[0].Count > 0)
						Root[0].Remove (0);
					
					NSTimer.CreateScheduledTimer (0.1, delegate {						
						int i = 0;
						foreach (FullLike like in fullLikes)
						{							
							Activity dbAct = new Activity()
							{
								Id = i,
								IdPhoto = _photo.Id,
								UserId = like.Like.UserId,
								Time = like.Like.Time,
							};
							var activity = new UIActivity()
			                { 
								Id = i,
								DbActivity = dbAct,
								User = like.User,
								Image = _photo,
								Type =  ActivityType.PhotoLiker,
								Text = string.Format(" liked {0} post", isMyPost ? "your" : "the"),
							};
							
							Root[0].Add(new ActivityElement(activity, null, null));
							i++;
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
				Util.LogException("Download photo likers", ex);
			}
		}
		
		// This is our subclass of the fixed-size Source that allows editing
		public class EditingSource : DialogViewController.Source {
			public EditingSource (DialogViewController dvc) : base (dvc) {}
			
			public override bool CanEditRow (UITableView tableView, NSIndexPath indexPath)
			{
				
				// Trivial implementation: we let all rows be editable, regardless of section or row
				return false;
			}
			
			public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
			{
				return 60;
			}
			
			public override UITableViewCellEditingStyle EditingStyleForRow (UITableView tableView, NSIndexPath indexPath)
			{
				// trivial implementation: show a delete button always
				return UITableViewCellEditingStyle.Delete;
			}
			
			public override void CommitEditingStyle (UITableView tableView, UITableViewCellEditingStyle editingStyle, 
			                                         NSIndexPath indexPath)
			{
				//
				// In this method, we need to actually carry out the request
				//
				var section = Container.Root [indexPath.Section];
				var element = section [indexPath.Row];
				section.Remove (element);
			}
		}		
	}	
}
