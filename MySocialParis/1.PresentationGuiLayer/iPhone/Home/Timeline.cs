using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;

namespace MSP.Client
{	
	public class TimelineViewController : BaseTimelineViewController
	{
		#region Members
		
		private int pageNumber = 0;		
		private RequestInfo request;
		private FilterType _FilterType;
		private List<ImageInfo> _list;
		private List<Image> previousList;
		private UINavigationController _MSP;
		private IMapLocationRequest _MapLocationRequest;
		private bool hasImage = false;
		
		#endregion
		
		#region Constructors
		
		public TimelineViewController (FilterType filterType, bool pushing, UINavigationController msp, 
		                               IMapLocationRequest maplocationRequest) 
			: base(pushing)
		{
			_FilterType = filterType;
			_MapLocationRequest = maplocationRequest;
			
			ShowLoadMorePhotos = true;

			this.TableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
			this.TableView.BackgroundColor = new UIColor (226f, 231f, 237f, 1f);
			this.TableView.AllowsSelection = false;

			switch(filterType)
			{
				case FilterType.Friends:
					TableView.BackgroundView = new UIImageView(Graphics.GetImgResource("fond1"));
					break;
				case FilterType.Recent:
					TableView.BackgroundView = new UIImageView(Graphics.GetImgResource("fond2"));
					break;
				case FilterType.All:
					TableView.BackgroundView = new UIImageView(Graphics.GetImgResource("fond3"));
					break;
			}

			_list = new List<ImageInfo>();
			_MSP = msp;
			
			OnGestSwipe += HandleOnSwipe;
		}
		
		#endregion
		
		#region Events
		
		private void HandleOnSwipe (SwipeDirection obj)
		{
			if (obj == SwipeDirection.Left)
			{
				ReloadTimeline();
			}
			if (obj == SwipeDirection.Right)
			{
				ReloadTimeline();
			}		
		}
		
		private void LoadAlbum(Image image)
		{
			Action act = () =>
			{						
				try
				{
					AlbumsResponse response = AppDelegateIPhone.AIphone.ImgServ.GetAlbum(0, image.IdAlbum);
					InvokeOnMainThread(()=>
                    {						
						_MSP.PushViewController(new AlbumDetailsViewController(_MSP, AppDelegateIPhone.AIphone, response.Album), true);
					});
				}
				catch (Exception ex)
				{
					Util.LogException("Initialize GetAlbum", ex);
				}
			};
			
			AppDelegateIPhone.ShowRealLoading(View, "Loading album details", null, act);
		}
				
		private void LoadPhoto(Image image)
		{
			Action act = ()=>
			{			
				try
				{
					int askerId = AppDelegateIPhone.AIphone.GetMainUserId();
					FullUserResponse fullUser = AppDelegateIPhone.AIphone.UsersServ.GetFullUserById(image.UserId, askerId);					
					if (fullUser == null)
						return;
					
					BeginInvokeOnMainThread(()=>
					{
						var a = new PhotoDetailsViewController(_MSP, fullUser, image, false);
						_MSP.PushViewController(a, true);
					});
				}
				catch (Exception ex)
				{
					Util.LogException("OnPhotoClicked", ex);
				}
			};
			AppDelegateIPhone.ShowRealLoading(View, "Loading photo details", null, act);			
		}

		public void OnPhotoClicked(BuzzPhoto photo)
		{			
			if (_MSP == null)
				return;
			
			if (photo.Photo == null)
				return;
			
			if (_FilterType == FilterType.Events && photo.Photo.IdAlbum > 0)
				LoadAlbum(photo.Photo);
			else
				LoadPhoto(photo.Photo);			
		}
		
		#endregion
		
		#region Overrides			
		
		public override void ReloadTimeline ()
		{
			ThreadPool.QueueUserWorkItem(o => 
            {
				request = new RequestInfo(RequestType.Refresh);
				
				if (_MapLocationRequest.InitializeMap(request))
				{	
					ThreadPool.QueueUserWorkItem(ob =>
					{
						Thread.Sleep(6000);
						
						if (request != null)
						{
							request = null;
							Util.Log("ReloadTimeline timedout");
							this.BeginInvokeOnMainThread (delegate { ReloadComplete (); });
						}
					});
				}
				else
					this.BeginInvokeOnMainThread (delegate { ReloadComplete (); });
			});
		}
		
		public override float GetHeightForRow (UITableView tableView, NSIndexPath indexPath)
		{
			var element = Root[0].Elements [indexPath.Row];
		
			var sizable = element as UIViewElement;
			if (sizable != null)
			{
				return sizable.GetHeight(tableView, indexPath);
			}
			
			if (indexPath.Row == Root[0].Count - 1)
				return 60 + 10;
								
			return base.GetHeightForRow (tableView, indexPath);
		}
		
		#endregion
		
		#region Public methods
		
		public void ShowLoadedImages(List<Image> images, RequestInfo req)
		{
			request = null;
			
			if (images != null)
			{
				try
				{
					if (InitList(images))
						DownloadTweets ();
					else
						this.InvokeOnMainThread (delegate { ReloadComplete (); });
				}
				catch (Exception ex)
				{
					this.BeginInvokeOnMainThread (delegate { ReloadComplete (); });
					Util.LogException("ShowLoadedImages", ex);
				}
			}
			else
				this.BeginInvokeOnMainThread (delegate { ReloadComplete (); });
		}
			
		public List<ImageInfo> GetLoadedImages()
		{
			return _list;
		}
		
		public void SetEmptyImages()
		{
			hasImage = true;
			previousList = null;
			pageNumber = 0;
			
			_list.Clear();			
			
			for (int i = 1; i <= 21; i++)
			{
				var imgInfo = new ImageInfo() { Index = i };
				_list.Add(imgInfo);
			}
			
			Root[0].RemoveRange(0, Root[0].Count);
			for (int i = 0; i < 7; i++) {
				Root[0].Add (new PhotosElement (this, i));
			}
		}
		
		public List<ImageInfo> GetImages (int cellIndex)
		{
			try
			{
				var img1 = _list[cellIndex * 3 + 0];
				var img2 = _list[cellIndex * 3 + 1];
				var img3 = _list[cellIndex * 3 + 2];
				
				return new List<ImageInfo> { img1, img2, img3 };
			}
			catch (Exception ex)
			{
				Util.LogException("GetImages cellIndex", ex);
				return null;
			}
		}
		
		#endregion
		
		#region Properties
		
		public bool ShowLoadMorePhotos {get;set;}
		
		#endregion
		
		#region Private methods
		
		private void AddMorePhotoElements(CustomLoadMoreElement more, ManualResetEventSlim wait)
		{						
			try
			{												
				pageNumber++;
		
				var images = _MapLocationRequest.GetDbImages(_FilterType, (pageNumber - 1) * 21, 21);
				if (images.Count() == 0)
				{
					pageNumber--;
					this.BeginInvokeOnMainThread(delegate {	more.Animating = false; });
					
					return;
				}
				
				Image[] resa = images.ToArray();
				int count = resa.Count();
				
				for (int i = 1; i <= 21; i++)
				{
					var imgInfo = new ImageInfo() { Index = i };
					if (i <= count)
					{
						imgInfo.Img = resa[i - 1];
					}
					_list.Add(imgInfo);
					previousList.Add(imgInfo.Img);
				}				
				
				// Now make sure we invoke on the main thread the updates
				this.BeginInvokeOnMainThread(delegate {		
					more.Animating = false;
					
					try
					{
						for (int i = (pageNumber - 1) * 7; i < pageNumber * 7; i++) 
						{
							Root[0].Insert(Root[0].Count - 1, new PhotosElement (this, i));
						}
					}
					catch (Exception ex)
					{
						Util.LogException("AddMorePhotoElements InsertElements", ex);
					}					
				});
				//TableView.ScrollToRow (NSIndexPath.FromRowSection (Root[0].Count - 1, 0), UITableViewScrollPosition.Middle, false);
			}
			catch (Exception ex)
			{
				Util.LogException("AddMorePhotoElements", ex);
				pageNumber--;
				
				// Now make sure we invoke on the main thread the updates
				this.BeginInvokeOnMainThread(delegate {
						more.Animating = false;					
					});
			}
			finally
			{
				wait.Set();
			}
		}		
		
		private void DownloadTweets ()
		{			
			pageNumber = 1;
			
			this.BeginInvokeOnMainThread (delegate {
				Root[0].RemoveRange(0, Root[0].Count);
				
					DownloadAsync();
					
					// Notify the dialog view controller that we are done
					// this will hide the progress info	
					this.BeginInvokeOnMainThread (delegate { ReloadComplete (); });
				//});
			});
		}
		
		private void DownloadAsync()
		{
			try
			{
				if (!hasImage)
				{
					var imgElement = new UIImageView(Graphics.GetImgResource("messagenoposts"));
					imgElement.Frame = new System.Drawing.RectangleF(5, 5, 310, imgElement.Bounds.Height);
					var viewEl = new UIViewElement(null, imgElement, true);
					
					Root[0].Add(viewEl);
					ReloadComplete();
					return;
				}
				
				for (int i = 0; i < 7; i++) {
					Root[0].Add (new PhotosElement (this, i));
				}
				if (ShowLoadMorePhotos)
				{
					CustomLoadMoreElement more = null;
					more = new CustomLoadMoreElement (delegate 
						{					
							var wait = new ManualResetEventSlim(false);					
							AppDelegateIPhone.ShowRealLoading("Loading more photos", null, wait);
							
							// Launch a thread to do some work
							ThreadPool.QueueUserWorkItem (delegate 
							{
								AddMorePhotoElements(more, wait);
							});
						});
						
					more.Height = 60;
					more.Image = Graphics.GetImgResource("more");
					
					try {
						Root[0].Insert (Root[0].Count, UITableViewRowAnimation.None, more);
					}  catch { }
				}
			}
			catch (Exception ex)
			{
				Util.LogException("DownloadAsync", ex);
			}
		}
		
		private bool InitList()
		{
			try
			{				
				var dbImages = _MapLocationRequest.GetDbImages(_FilterType, 0, 21);
				if (dbImages == null)
					return false;
			
				return InitList(dbImages.ToList());
			}
			catch (Exception ex)
			{
				Util.LogException("InitList " + _FilterType.ToString(), ex);
				return false;
			}
		}

		private bool Compare(List<Image> resa, List<Image> previousList)
		{
			bool diff = false;
			if (previousList != null)
			{
				for (int i = 0; i < previousList.Count && i < resa.Count; i++)
				{
					if (previousList[i] != null)
					{
						if (previousList[i].Id != resa[i].Id)
						{
							diff = true;
							break;
						}
					}
					else
					{
						diff = true;
						break;
					}
				}
				if (previousList.Count != resa.Count)
					diff = true;
			}
			else
			{
				previousList = new List<Image>();
				diff = true;
			}

			return diff;
		}
		
		private bool InitList(List<Image> resa)
		{
			if (resa == null)
				return false;
			
			hasImage = !(_FilterType == FilterType.Friends && resa.Count == 0);

			if (!Compare(resa, previousList))
				return false;
			
			previousList = resa;
			_list.Clear();
			int count = resa.Count();
			
			for (int i = 1; i <= 21; i++)
			{
				var imgInfo = new ImageInfo() { Index = i };
				if (i <= count)
				{
					imgInfo.Img = resa[i - 1];
				}
				_list.Add(imgInfo);
			}
			
			return true;
		}				
		
		#endregion
	}
}

