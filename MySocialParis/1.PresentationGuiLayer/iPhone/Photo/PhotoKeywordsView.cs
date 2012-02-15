using System;
using System.Threading;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;
using System.Linq;

namespace MSP.Client
{	
	public class PhotoKeywordsView : BaseTimelineViewController				
	{
		private int _ImageID;
		private User _photoOwner;
		
		public PhotoKeywordsView (bool pushing, int imageID, User photoOwner) : base(pushing)
		{
			Style = UITableViewStyle.Plain;
			
			this.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			this.TableView.BackgroundColor = new UIColor (0, 0, 0, 0);
			this.TableView.AllowsSelection = false;
			this.TableView.BackgroundView = new UIImageView(Graphics.GetImgResource("fond"));
			
			var rootElement = new RootElement ("keywords") { UnevenRows = true };
			var section = new Section();
			rootElement.Add(section);
			this.Root = rootElement;			
			
			_ImageID = imageID;	
			_photoOwner = photoOwner;
			
			ReloadTimeline();
		}

		public override void ReloadTimeline ()
		{			
			ThreadPool.QueueUserWorkItem(o => DownloadTweets());
		}
		
		public override Source CreateSizingSource (bool unevenRows)
		{
			return new EditingSource(this);
		}		
		
		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			var element = Root[0].Elements [indexPath.Row];
		
			var sizable = element as KeywordElement;
			if (sizable == null)
				return 60;
			
			return 40;
			//return base.GetHeightForRow (tableView, indexPath);
		}

		void DownloadTweets ()
		{
			try
			{
				var keywords = AppDelegateIPhone.AIphone.KeywServ.GetKeywordsOfImage(_ImageID);
				
				this.BeginInvokeOnMainThread (delegate {
					while (Root[0].Count > 0)
						Root[0].Remove (0);
					
					NSTimer.CreateScheduledTimer (0.1, delegate {
						
						Root[0].Add(CreateLoadMore(Root[0]));
						foreach (Keyword keyword in keywords)
						{
							Root[0].Add(new KeywordElement(keyword.Name, keyword));
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
		
		private Element CreateLoadMore(Section section)
		{
           var loadMore2 = new AddLoadMoreWithImageElement("Add keyword", ImageStore.DefaultImage, lme => {
				lme.FetchValue();
				if (!string.IsNullOrWhiteSpace(lme.Value))
				{			
					// Launch a thread to do some work
					ThreadPool.QueueUserWorkItem (delegate {
						try
						{
							var keyword = new Keyword()
							{
								Name = lme.Value,
								ParentId = _ImageID,
								//UserId = AppDelegateIPhone.AIphone.MainUser.Id,
								//Time = DateTime.UtcNow,
							};
							
							//	TODO : get the keyword id from the server. We need it
							//	TODO : for the deletion process
							AppDelegateIPhone.AIphone.KeywServ.PutNewKeyword(keyword);
						
							// Now make sure we invoke on the main thread the updates
							this.BeginInvokeOnMainThread(delegate {
								lme.Animating = false;
														
								var act = new KeywordElement(keyword.Name, keyword);
								section.Insert(1, act);
								lme.Value = null;							
							});
						}
						catch (Exception ex)
						{
							Util.LogException("Add keyword", ex);
							this.BeginInvokeOnMainThread(delegate {
								lme.Animating = false;
							});
						}					
					});
				}
				else
					lme.Animating = false;
				
			});
			
			return loadMore2;
		}
	}
	
	public class KeywordElement : StringElement
	{
		public Keyword keyword;
		
		public KeywordElement(string @value, Keyword keyword) : base(@value)
		{
			this.keyword = keyword;
		}
	}
}
