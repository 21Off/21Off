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
using System.Collections.Specialized;

namespace MSP.Client
{
	public partial class PhotoPostViewController : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public PhotoPostViewController (IntPtr handle) : base(handle)
		{

		}

		[Export("initWithCoder:")]
		public PhotoPostViewController (NSCoder coder) : base(coder)
		{

		}
		
		private UINavigationController _MSP;
		private RootElement root;
		private UIImage _image;
		
		public PhotoPostViewController (UINavigationController msp, UIImage image, CLLocation photoLocation, 
		                                string locationMapPhotoCapture) 
			: base("PhotoPostViewController", null)
		{
			this._image = image;
			this._MSP = msp;
			
			LocationMapPhotoCapture = locationMapPhotoCapture;
			PhotoLocation = photoLocation;
			
			// Add the views
			//NSNotificationCenter.DefaultCenter.AddObserver ("UIKeyboardWillShowNotification", KeyboardWillShow);			
			
			this.root = CreateRoot ();
		}
		
		void KeyboardWillShow (NSNotification notification)
		{
			var kbdBounds = (notification.UserInfo.ObjectForKey (UIKeyboard.BoundsUserInfoKey) as NSValue).RectangleFValue;
			//Console.WriteLine(kbdBounds);
			//composerView.Frame = ComputeComposerSize (kbdBounds);
		}		
		
		#endregion	
		
		#region Members
		
		public MyEntryElement Description {get;set;}
		public CLLocation PhotoLocation {get;set;}
		
		private DialogViewController _dialogView;
		private RectangleF mapFrame;
		
		private Settings _Settings;
		public string LocationMapPhotoCapture {get;set;}
		
		#endregion
		
		
		private UIView topBar;
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
		
		public override void ViewDidAppear (bool animated)
		{
			base.ViewDidAppear (animated);
			if(_Settings!=null)
			{
				//_Settings.facebook.SetValue(_Settings.facebookApp.LoggedIn());
				//_Settings.twitter.SetValue(_Settings.twitterApp.LoggedIn());
			}
		}
		
		
		private bool isOnKeywordScreen = false;
		
		#region Events

		void HandleOkBtnTouchUpInside (object sender, EventArgs e)
		{
			Keywords = GetKeyword();
			
			Section section = this.root.ElementAtOrDefault(0);
			
			if (section.Elements[0].GetType() == typeof(MyEntryElement))
			{				
				mapFrame = new RectangleF(0, 45, UIScreen.MainScreen.Bounds.Width, UIScreen.MainScreen.Bounds.Height - 50 - 5);
				
				section.Remove(0);							
				section.Add(CreateLoadMore(section));
				SetTitleText("keywords", "enter keywords");
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
		
		void HandleBackBtnTouchUpInside (object sender, EventArgs e)
		{
			Section section = this.root.ElementAtOrDefault(0);
			if (isOnKeywordScreen && _Settings == null)
			{
				_dialogView.View.SendSubviewToBack(topBar);
				
				section.RemoveRange(0, section.Count);
				section.Add(Description);
				SetTitleText("post", "describe your post");
				
				this.View.BringSubviewToFront(topBar);
				
				isOnKeywordScreen = false;
				
				return;
			}
			
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
		
		SizeF GetTextSize (string text, UILabel label)
		{
			return new NSString (text).StringSize (label.Font, UIScreen.MainScreen.Bounds.Width, 
			                                       UILineBreakMode.TailTruncation);
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
					// Launch a thread to do some work
					ThreadPool.QueueUserWorkItem (delegate {
						
						// We just wait for 2 seconds.
						System.Threading.Thread.Sleep(200);
					
						// Now make sure we invoke on the main thread the updates
						this.BeginInvokeOnMainThread(delegate {
							lme.Animating = false;
							if (section.Count == 1)
								section.Insert(1, new StringElement(lme.Value));
							else
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
		
		void AdvancedConfigEdit (DialogViewController dvc)
		{
			// Activate row editing & deleting
			//dvc.TableView.SetEditing (true, true);
		}		
				
		private void SavePhoto()
		{
			if (PhotoLocation == null || LocationMapPhotoCapture == null)
			{
				Util.ShowAlertSheet("Couldnt get location", View);
				return;
			}
			
			try
			{
				var AppDel = AppDelegateIPhone.AIphone;				
				string desc = Description == null ? null : Description.Value;
				
				var image = new Image() 
				{
					Name = desc, 
					UserId = AppDel.MainUser.Id,
					Longitude = PhotoLocation.Coordinate.Longitude,
					Latitude = PhotoLocation.Coordinate.Latitude,
					Altitude = PhotoLocation.Altitude,
					Time = DateTime.UtcNow,
				};
				
				var err = new NSError();				
				var jpgstr = Path.Combine(ImageStore.TmpDir, string.Format("{0}.jpg", image.Id));
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
				
				//_image = _image.resizeImage(new SizeF(612, 612));
				_image.AsJPEG((float)0.7).Save(jpgstr, true, out err);				
								
				image = AppDel.ImgServ.StoreNewImage(image, jpgstr, LocationMapPhotoCapture, Keywords);
								
				//TODO Save all sizes photo + map in order to avoid further downloading
				//var mapLocationstr = Path.Combine(ImageStore.MapLocations, string.Format("{0}.png", image.Id));
				//var jpgstrFinal = Path.Combine(ImageStore.TmpDir, string.Format("{0}.png", image.Id));
				
				//File.Copy(LocationMapPhotoCapture, mapLocationstr, true);
				//File.Copy(jpgstr, jpgstrFinal, true);
				
				InvokeOnMainThread(()=>
				{
					if (image.Id == 0)
						return;
					
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
							using(UIAlertView alert = new UIAlertView("Warning","Not logged in",null,"OK",null))
							{ alert.Show();}
						}
					}
					
					if (_Settings.facebook.Value)
					{
						var faceBookApp = new FaceBook.FaceBookApplication(this);
						if (faceBookApp.LoggedIn())
						{
							faceBookApp.Publish("21Off", img, image.Name ?? "No comment", img);
						}
						else
						{
							using(UIAlertView alert = new UIAlertView("Warning","Not logged in",null,"OK",null))
							{ alert.Show();}
						}
					}
				});
				
				InvokeOnMainThread(()=>
				{
					_MSP.PopViewControllerAnimated(true);
					_MSP.PopViewControllerAnimated(true);
					_MSP.SetViewControllers(new UIViewController[0], false);
					
					var aaa =  _MSP.TabBarController;			
					aaa.SelectedViewController = aaa.ViewControllers[0];
					
					var rotatingTb = (RotatingTabBar)AppDelegateIPhone.tabBarController;
					rotatingTb.SelectTab(0);
				});				
			}
			catch (Exception ex)
			{
				Util.ShowAlertSheet(ex.Message, View);
			}			
		}
		
		RootElement CreateRoot ()
		{
			Description = new MyEntryElement ("title", null, false, UIReturnKeyType.Default);
			
			return new RootElement ("post") {
				new Section (){ Description/*, KeywordsRoot*/ }, 					
			};
		}
		
		private List<Keyword> Keywords = new List<Keyword>();
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
			}
			
			return keywords;
		}		
		
		#endregion
	}	
}

