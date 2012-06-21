using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using Share;

namespace MSP.Client
{
	public partial class WelcomePage : UIViewController, IMapLocationRequest
	{
		public UINavigationController Nav { get; set; }
		private AppDelegateIPhone _AppDel;
		
		public WelcomePage () : base ("WelcomePage", null)
		{
		}
		
		public WelcomePage(AppDelegateIPhone appDel) :this()
		{
			_AppDel = appDel;
		}
		
		public override void DidReceiveMemoryWarning ()
		{
			// Releases the view if it doesn't have a superview.
			base.DidReceiveMemoryWarning ();
			
			// Release any cached data, images, etc that aren't in use.
		}

		/*
		partial void leftAction (MonoTouch.Foundation.NSObject sender)
		{
			
		}
		
		partial void rightAction (MonoTouch.Foundation.NSObject sender)
		{
		}
		*/
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
			
			// Perform any additional setup after loading the view, typically from a nib.
			
			Initialize();
			
			this.request = new RequestInfo(RequestType.FirstLoad);
			
			ThreadPool.QueueUserWorkItem(o => LoadTimelineImages());
		}
		
		public override void ViewDidUnload ()
		{
			base.ViewDidUnload ();
			
			// Clear any references to subviews of the main view in order to
			// allow the Garbage Collector to collect them sooner.
			//
			// e.g. myOutlet.Dispose (); myOutlet = null;
			
			ReleaseDesignerOutlets ();
		}
		
		public override bool ShouldAutorotateToInterfaceOrientation (UIInterfaceOrientation toInterfaceOrientation)
		{
			// Return true for supported orientations
			return (toInterfaceOrientation != UIInterfaceOrientation.PortraitUpsideDown);
		}
		
		
		private void Initialize()
		{
			LocType = LocalisationType.Global;
			
			float width = UIScreen.MainScreen.Bounds.Width;
			float topY = 40;
			var size = new SizeF(width, 480 - topY);
			
			var likedRootElement = new RootElement ("recent") { UnevenRows = true };
			var likedSection = new Section("recent posts worldwide");
			likedRootElement.Add(likedSection);
			
			likedMediaView = new TimelineViewController(FilterType.Liked, true, Nav, this) 
			{ 
				Root = likedRootElement, 
				ShowLoadMorePhotos = false,				
			};
			
			likedMediaView.TableView.BackgroundView = new UIImageView(Graphics.GetImgResource("pagedegarde"));
			likedMediaView.View.Frame = new System.Drawing.RectangleF(new PointF(0, topY),  size);
			this.Add(likedMediaView.View);
			
			var bottomPanel = new UIView(new RectangleF(0, View.Bounds.Height - 48, width, 48));
			//bottomPanel.DrawBorder(UIColor.LightGray);
			bottomPanel.BackgroundColor = UIColor.White;
			
			UIButton createAccountBtn = UIButton.FromType (UIButtonType.RoundedRect);
			createAccountBtn.Frame = new RectangleF(4, 2, 120, 44);
			createAccountBtn.SetTitleColor(UIColor.LightGray, UIControlState.Normal);
			createAccountBtn.SetTitle("create account", UIControlState.Normal);
			createAccountBtn.TouchUpInside += HandleCreateAccountBtnTouchDown;
			
			UIButton signInBtn = UIButton.FromType (UIButtonType.RoundedRect);
			signInBtn.Frame = new RectangleF(128, 2, 60, 44);
			signInBtn.SetTitleColor(UIColor.LightGray, UIControlState.Normal);
			signInBtn.SetTitle("sign in", UIControlState.Normal);
			signInBtn.TouchUpInside += HandleSignInBtnTouchDown;			
			
			UIButton signInWithFacebookBtn = UIButton.FromType (UIButtonType.RoundedRect);
			float w = 320 - 192 - 4;
			signInWithFacebookBtn.Frame = new RectangleF(192, 2, w, 44);
			signInWithFacebookBtn.SetTitleColor(UIColor.LightGray, UIControlState.Normal);			
			signInWithFacebookBtn.ImageEdgeInsets = new UIEdgeInsets(5, w - 26 - 5, 5, 5);
			signInWithFacebookBtn.TitleEdgeInsets = new UIEdgeInsets(0, - 26, 0, 26 + 5);					
			
			signInWithFacebookBtn.SetTitle("sign in with", UIControlState.Normal);
			signInWithFacebookBtn.SetImage(Graphics.GetImgResource("logo_FB"), UIControlState.Normal);
			signInWithFacebookBtn.TouchUpInside += HandleSignInWithFacebookBtnTouchDown;
									
			bottomPanel.Add(createAccountBtn);
			bottomPanel.Add(signInBtn);
			bottomPanel.Add(signInWithFacebookBtn);
			this.View.Add(bottomPanel);
			
			var imgViesw = new UIImageView(UIImage.FromBundle("Images/Icon"));
			imgViesw.Frame = new RectangleF((320 - 26)/2, (topY - 26) / 2, 26, 26);
			this.Add(imgViesw);
		}

		void HandleCreateAccountBtnTouchDown (object sender, EventArgs e)
		{
			Nav.PopViewControllerAnimated(false);
			Nav.PushViewController (new NewAccountViewController (_AppDel, Nav), false);
		}

		void HandleSignInBtnTouchDown (object sender, EventArgs e)
		{
			Nav.PushViewController(new AuthentificationViewController(_AppDel, Nav), true);
		}

		void HandleSignInWithFacebookBtnTouchDown (object sender, EventArgs e)
		{
			FacebookAuth ();
		}
		
		private FaceBook.FaceBookApplication facebookApp;		
				
		void FacebookAuth ()
		{ 					
			facebookApp = new FaceBook.FaceBookApplication (this);
			facebookApp.OnExtraLoginComplete += (GraphUser u) =>
			{	
				GraphUser guser = u;//AppDelegateIPhone.AIphone.FacebookServ.GetMyProfile ();
				if (guser != null) {
					NSUserDefaults.StandardUserDefaults.SetDouble ((double)guser.id, "FacebookId");
					
					PostAuth(guser.name, guser.id);
				}				
			};
			
			facebookApp.Login ();
		}
		
		private void PostAuth(string username, decimal userid)
		{
			Action act = () =>
			{		
				try
				{
					User user = _AppDel.UsersServ.AuthentificateFacebook(username, userid);
					if (user == null)
					{
						InvokeOnMainThread(()=>
	                    {
							Util.ShowAlertSheet("Authentification failed", View);
						});
						return;
					}
					if (user.Id == 0)
					{
						InvokeOnMainThread(()=>
	                    {
							Util.ShowAlertSheet("User or password is wrong", View);
						});
						return;						
					}					
					
					string img = string.Format("{0}/21OffFB.jpg", UrlStore.streamingUrl);
					
					if (facebookApp != null)
						facebookApp.Publish("21Off", img, "21Off subscription", img);
					
					User dbUser = Database.Main.Table<User>().Where(el => el.Id == user.Id).FirstOrDefault();
					if (dbUser == null)
						Database.Main.Insert(user);
					else
					{
						Database.Main.Update(dbUser);
					}
					
					LastUserLogged lastUser = Database.Main.Table<LastUserLogged>().LastOrDefault();
					if (lastUser == null || lastUser.Id != user.Id)
						Database.Main.Insert(new LastUserLogged(){ UserId = user.Id });
					
					_AppDel.MainUser = user;
					
					InvokeOnMainThread(()=>
                    {
						_AppDel.MainWnd.WillRemoveSubview(this.View);
						_AppDel.InitApp();
						_AppDel.LoadFacebookFriends();
					});
				}
				catch (Exception ex)
				{
					Util.LogException("Authentification error", ex);
					Util.ShowAlertSheet(ex.Message, View);
					return;
				}
			};
			
			AppDelegateIPhone.ShowRealLoading(View, "Connexion", null, act);			
		}
				
		private TimelineViewController likedMediaView;
		private RequestInfo request;

		#region IMapLocationRequest implementation
		
		bool IMapLocationRequest.InitializeMap (RequestInfo request)
		{
			this.request = request;
			ThreadPool.QueueUserWorkItem(o => LoadTimelineImages());
						
			return true;
		}
		
		private void LoadTimelineImages()
		{
			try
			{
				GeoLoc geoLoc = LocType == LocalisationType.Global ? null : (Location == null ? null :
					new GeoLoc()
				{
					Latitude = Location.Coordinate.Latitude,
					Longitude = Location.Coordinate.Longitude,
				});
				
				AllImagesResponse allImgResp = AppDelegateIPhone.AIphone.ImgServ.GetFullImageList(FilterType.All, geoLoc, 0, 21, -1);
				likedMediaView.ShowLoadedImages(allImgResp == null ? null : allImgResp.RecentImages, request);	
			}
			catch (Exception ex)
			{
				Util.LogException("LoadTimelineImages",ex);			
			}
		}	

		IEnumerable<Image> IMapLocationRequest.GetDbImages (FilterType filterType, int start, int count)
		{
			User user = AppDelegateIPhone.AIphone.MainUser;
			GeoLoc geoLoc = LocType == LocalisationType.Global ? null : (Location == null ? null :
				new GeoLoc()
			{
				Latitude = Location.Coordinate.Latitude,
				Longitude = Location.Coordinate.Longitude,
			});				
			
			return AppDelegateIPhone.AIphone.ImgServ.GetImageList(FilterType.Recent, geoLoc, start, count, user.Id);	
		}

		FilterType IMapLocationRequest.GetFilterType ()
		{
			return FilterType.Recent;
		}

		List<ImageInfo> IMapLocationRequest.GetCurrentLoadedImages ()
		{
			return likedMediaView.GetLoadedImages();
		}

		public LocalisationType LocType { get;set; }
		public MonoTouch.CoreLocation.CLLocation Location { get;set; }
		
		#endregion
	}
}