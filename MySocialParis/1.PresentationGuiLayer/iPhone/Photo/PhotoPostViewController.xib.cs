using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using MonoTouch.CoreLocation;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using TweetStation;

namespace MSP.Client
{
	public partial class PhotoPostViewController : UIViewController
	{
		#region Constructors

		public PhotoPostViewController (IntPtr handle) : base(handle)
		{

		}

		[Export("initWithCoder:")]
		public PhotoPostViewController (NSCoder coder) : base(coder)
		{

		}
		
		public PhotoPostViewController(): base("PhotoPostViewController", null)
		{
			postOptions = PostOptions.PostNew;
		}
		
		public PhotoPostViewController (UINavigationController msp, UIImage image, CLLocation photoLocation, 
		                                string locationMapPhotoCapture) 
			: this()
		{
			this._image = image;
			this._MSP = msp;
			
			LocationMapPhotoCapture = locationMapPhotoCapture;
			PhotoLocation = photoLocation;
			
			// Add the views
			//NSNotificationCenter.DefaultCenter.AddObserver ("UIKeyboardWillShowNotification", KeyboardWillShow);			
			
			this.root = CreateRoot ();
		}
				
		public PhotoPostViewController (UINavigationController msp, UIImage image, Tweet tweet) : this()
		{
			this._image = image;
			this._MSP = msp;
			this.eventImage = tweet.Image;
			this.postOptions = tweet.Options;
			this.isForEvent = true;
			
			PhotoLocation = new CLLocation(eventImage.Latitude, eventImage.Longitude);
			
			this.root = CreateRoot ();			
			
			Description.Value = eventImage.Name;
			Keywords.AddRange(tweet.Keywords);
		}	
		
		#endregion	
		
		#region Fields
			
		private List<Keyword> Keywords = new List<Keyword>();
		private UINavigationController _MSP;
		private RootElement root;
		private UIImage _image;
		private Image eventImage;
		private PostOptions postOptions;
		private bool isForEvent;
		private bool isOnKeywordScreen = false;
		
		public MyEntryElement Description {get; set;}
		public MyEntryElement FirstComment {get; set;}
		private AddLoadMoreElement addLoadMore {get; set;}
		
		public CLLocation PhotoLocation {get; set;}
		public string LocationMapPhotoCapture {get;set;}
		
		private BooleanElement createAlbumCbx;		
		private DialogViewController _dialogView;
		private RectangleF mapFrame;		
		private UIView topBar;
		private Settings _Settings;		
		
		#endregion
		
		#region Overrides
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();		
						
			okBtn.SetImage(Graphics.GetImgResource("ok"), UIControlState.Normal);
			backBtn.SetImage(Graphics.GetImgResource("back"), UIControlState.Normal);
			
			okBtn.TouchUpInside += HandleOkBtnTouchUpInside;
			backBtn.TouchUpInside += HandleBackBtnTouchUpInside;			
			
			mapFrame = new RectangleF(0, 50, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height - 45 - 25);
			
			_dialogView = new DialogViewController (root, true);
			_dialogView.TableView.BackgroundView = new UIImageView(Graphics.GetImgResource("fond"));
			_dialogView.View.Frame = mapFrame;
			
			this.View.Add(_dialogView.View);
						
			topBar = new UIView(new RectangleF(0, 45, 320, 1));
			topBar.Layer.BackgroundColor = UIColor.LightGray.CGColor;
			this.View.AddSubview(topBar);
		}
				
		#endregion				
		
		#region Events
				
		private void KeyboardWillShow (NSNotification notification)
		{
			var kbdBounds = (notification.UserInfo.ObjectForKey (UIKeyboard.BoundsUserInfoKey) as NSValue).RectangleFValue;
			Console.WriteLine(kbdBounds);
			//composerView.Frame = ComputeComposerSize (kbdBounds);
		}			
		
		private void HandleOkBtnTouchUpInside (object sender, EventArgs e)
		{
			if (isForEvent)
			{
				isForEvent = false;
			}
			else
				Keywords = GetKeyword();
			
			Section section = this.root.ElementAtOrDefault(0);
			
			// We are in the first screen (POST) and we click on Ok button to go to Keyboard screen
			if (section.Elements[0].GetType() == typeof(MyEntryElement))
			{
				mapFrame = new RectangleF(0, 45, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height - 50 - 5);
				
				section.Clear();
				root[1].Clear();
				if (eventImage == null)
					root[2].Clear();
				
				addLoadMore = CreateLoadMore(section);
				section.Add(addLoadMore);
				SetTitleText("keywords", "enter keywords");
				
				Keywords.ForEach(el => section.Add(new StringElement(el.Name)));
				_dialogView.View.Frame = mapFrame;
				this.View.SetNeedsLayout();
				
				_dialogView.TriggerRefresh();
				_dialogView.TableView.ReloadData();				
				
				topBar.Frame = new RectangleF(0, 45, 320, 1);
				this.View.WillRemoveSubview(topBar);
				this.View.AddSubview(topBar);
				
				isOnKeywordScreen = true;
				return;
			}
			
			// We go on the Share screen
			if (_Settings == null)
			{
				_Settings = new Settings();
			
				_Settings.TableView.BackgroundView = new UIImageView(Graphics.GetImgResource("fond"));
				_Settings.View.Frame = mapFrame;				
				
				this.View.WillRemoveSubview(_dialogView.View);
				this.View.AddSubview(_Settings.View);
				_Settings.View.SendSubviewToBack(topBar);
				
				SetTitleText("post", "share");
				
				this.View.BringSubviewToFront(topBar);

				return;
			}			
			
			AppDelegateIPhone.ShowRealLoading(View, "Posting photo", null, SavePhoto);
		}
		
		private void HandleBackBtnTouchUpInside (object sender, EventArgs e)
		{
			Section section = this.root.ElementAtOrDefault(0);
			
			// We go back to the first screen, at the Post initialization
			if (isOnKeywordScreen && _Settings == null)
			{
				_dialogView.View.SendSubviewToBack(topBar);
				
				section.RemoveRange(0, section.Count);
				section.Add(Description);
				root[1].Add(FirstComment);
				if (eventImage == null)
					root[2].Add(createAlbumCbx);
				
				SetTitleText("post", "describe your post");
				
				this.View.BringSubviewToFront(topBar);
				
				isOnKeywordScreen = false;
				
				return;
			}
			
			// We go back to the Keywords screen
			if (_Settings != null)
			{
				this.View.WillRemoveSubview(_Settings.View);
				this.View.AddSubview(_dialogView.View);
				_dialogView.View.SendSubviewToBack(topBar);
				_Settings = null;
				
				SetTitleText("keywords", "enter keywords");
				
				this.View.BringSubviewToFront(topBar);
				
				return;
			}
			else			
				_MSP.PopViewControllerAnimated(true);
		}
		
		#endregion							
		
		#region Private methods
		
		private SizeF GetTextSize (string text, UILabel label)
		{
			return new NSString (text).StringSize (label.Font, UIScreen.MainScreen.Bounds.Width, UILineBreakMode.TailTruncation);
		}		
		
		private void SetTitleText(string text, string subTitle)
		{
			photoPostTitle.Text = text;
			photoPostSubTitle.Text = subTitle;			
			
			SizeF sizeF = GetTextSize(text, photoPostTitle);
			PointF point = photoPostTitle.Frame.Location;
			point.X = (320 - sizeF.Width) / 2;
			
			photoPostTitle.Frame = new RectangleF(point, sizeF);
			
			sizeF = GetTextSize(subTitle, photoPostSubTitle);
			point = photoPostSubTitle.Frame.Location;
			point.X = (320 - sizeF.Width) / 2;
			point.Y = 25;
			
			photoPostSubTitle.Frame = new RectangleF(point, sizeF);						
		}
		
		private AddLoadMoreElement CreateLoadMore(Section section)
		{
           var loadMore2 = new AddLoadMoreElement("New keyword", lme => {
				lme.FetchValue();
				if (!string.IsNullOrWhiteSpace(lme.Value))
				{			
					ThreadPool.QueueUserWorkItem (delegate {						
						System.Threading.Thread.Sleep(200);					
						this.BeginInvokeOnMainThread(delegate {
							lme.Animating = false;
							section.Insert(1, new StringElement(lme.Value));							
							lme.Value = null;
						
						});
					
					});
				}
				else
					lme.Animating = false;
			});

			return loadMore2;
		}
		
		private void AdvancedConfigEdit (DialogViewController dvc)
		{
			// Activate row editing & deleting
			//dvc.TableView.SetEditing (true, true);
		}		
		
		private Image GetPhoto(AppDelegateIPhone AppDel, ref string jpgstr)
		{			
			if (Description != null)
				Description.FetchValue();
			
			string desc = Description == null ? null : Description.Value;
			
			var image = new Image() 
			{
				Name = desc, 
				UserId = AppDel.MainUser.Id,
				Longitude = PhotoLocation.Coordinate.Longitude,
				Latitude = PhotoLocation.Coordinate.Latitude,
				Time = DateTime.UtcNow,
			};
			
			var err = new NSError(new NSString("Photo"), 1);				
			jpgstr = Path.Combine(ImageStore.TmpDir, string.Format("{0}.jpg", image.Id));
			int level = Util.Defaults.IntForKey ("sizeCompression");
			
			var size = _image.Size;
			float maxWidth = 640;
			switch (level){
			case 0:
				maxWidth = 640;
				break;
			case 1:
				maxWidth = 1024;
				break;
			default:
				maxWidth = size.Width;
				break;
			}
			
			if (size.Width > maxWidth || size.Height > maxWidth)
				_image = GraphicsII.Scale (_image, new SizeF (maxWidth, maxWidth*size.Height/size.Width));				
			
			_image.AsJPEG((float)0.7).Save(jpgstr, true, out err);
			
			return image;
		}
				
		private void SavePhoto()
		{
			if (PhotoLocation == null || (LocationMapPhotoCapture == null && eventImage == null))
			{
				return;
			}
			
			try
			{
				var AppDel = AppDelegateIPhone.AIphone;
				string jpgstr = null;
				Image image = GetPhoto(AppDel, ref jpgstr);
				
				FirstComment.FetchValue();
				var comments = new List<Comment>();
				if (!string.IsNullOrWhiteSpace(FirstComment.Value))
				{
					comments.Add(new Comment()
		            { 
						Name = FirstComment.Value, 
						UserId = AppDel.MainUser.Id, 
						Time = DateTime.UtcNow,						
						// ParentId : Set this on server side on post time 
					});
				}
				
				if (postOptions == PostOptions.PostNew)
				{
					image = AppDel.ImgServ.StoreNewImage(image, jpgstr, LocationMapPhotoCapture, Keywords, comments, 
					                                     eventImage == null ? 0 : eventImage.Id);
				}
				if (postOptions == PostOptions.CreateAlbum)
				{
					image = AppDel.ImgServ.CreateAlbumWithImage(image, jpgstr, LocationMapPhotoCapture, Keywords, comments, 
				                                    image.Name, eventImage == null ? 0 : eventImage.Id);					
				}
				if (postOptions == PostOptions.ShareDescriptions)
				{
					image = AppDel.ImgServ.StoreNewImage(image, jpgstr, LocationMapPhotoCapture, Keywords, comments, 
                                    eventImage == null ? 0 : eventImage.Id);						
				}
				if (postOptions == PostOptions.AddToAlbum)
				{
					image = AppDel.ImgServ.AddNewImageToAlbum(image, jpgstr, LocationMapPhotoCapture, Keywords, 
                              comments, eventImage.IdAlbum, eventImage == null ? 0 : eventImage.Id);
				}
				
				Thread.Sleep(200);
				
				InvokeOnMainThread(()=>
				{					
					if (image.Id == 0)
						return;
					
					NetworkPost(image);

					if (eventImage != null)
					{
						_MSP.PopViewControllerAnimated(true);
					}
					else
					{
						_MSP.PopViewControllerAnimated(true);
						_MSP.PopViewControllerAnimated(true);
						_MSP.SetViewControllers(new UIViewController[0], false);
						
						var aaa =  _MSP.TabBarController;
						aaa.SelectedViewController = aaa.ViewControllers[0];
						
						var rotatingTb = (RotatingTabBar)AppDelegateIPhone.tabBarController;
						rotatingTb.SelectTab(0);
					}
				});
			}
			catch (Exception ex)
			{
				Util.ShowAlertSheet(ex.Message, View);
			}			
		}
		
		private void NetworkPost(Image image)
		{
			try
			{			
				string img = string.Format("http://storage.21offserver.com/files/{0}/{1}.jpg", image.UserId, image.Id);
				if (_Settings.twitter.Value)
				{
					var twitterApp = new Twitter.TwitterApplication(this);
					if (twitterApp.LoggedIn())
					{
						twitterApp.Publish("21Off", img, image.Name ?? "No comment" , img);
					}
					else
					{
						using(var alert = new UIAlertView("Warning","Not logged in",null,"OK",null))
						{ alert.Show();}
					}
				}
				if (_Settings.facebook.Value)
				{
					var faceBookApp = new FaceBook.FaceBookApplication(this);
					if (faceBookApp.LoggedIn())
					{
						faceBookApp.Publish(image.Name ?? "No comment", img, "Posted with 21Off", img);
					}
					else
					{
						using(var alert = new UIAlertView("Warning","Not logged in",null,"OK",null))
						{ alert.Show();}
					}
				}
			}
			catch (Exception ex)
			{
				Util.LogException("NetworkPost", ex);
			}
		}
		
		private RootElement CreateRoot ()
		{
			Description = new MyEntryElement ("title", null, false, UIReturnKeyType.Default);
			FirstComment = new MyEntryElement ("first comment", null, false, UIReturnKeyType.Default);
			
			if (eventImage != null)
			{
				return new RootElement ("post") {
					new Section () {  Description, },
					new Section() { FirstComment, }
				};
			}
			else
			{
				createAlbumCbx = new BooleanElement (Locale.GetText ("create album"), false);
				createAlbumCbx.ValueChanged += (sender, e) => 
				{
					postOptions = createAlbumCbx.Value ? PostOptions.CreateAlbum : PostOptions.PostNew;
				};
				return new RootElement ("post") {
					new Section () {  Description, },
					new Section() { FirstComment, },
					new Section() { createAlbumCbx }
				};
			}
		}
				
		private List<Keyword> GetKeyword()
		{
			Section section = this.root.ElementAtOrDefault(0);
			var keywords = new List<Keyword>();
			for (int i = 0; i < section.Elements.Count; i++)
			{
				if (section.Elements[i] is StringElement)
				{
					var strElement = (StringElement)section.Elements[i];					
					var keyword = new Keyword()
					{
						Name = strElement.Caption
					};
					keywords.Add(keyword);
				}
				if (addLoadMore != null)
				{
					addLoadMore.FetchValue();
					
					if (!keywords.Any(e => e.Name == addLoadMore.Value))
					{
						var keyword = new Keyword()
						{
							Name = addLoadMore.Value,
						};
						keywords.Add(keyword);
					}
				}
			}
			
			return keywords;
		}		
		
		#endregion
	}	
}

