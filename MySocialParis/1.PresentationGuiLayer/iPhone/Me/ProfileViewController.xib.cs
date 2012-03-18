using System;
using System.Drawing;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using System.Linq;
using MSP.Client.DataContracts;
using Share;

namespace MSP.Client
{
	public partial class ProfileViewController : UIViewController
	{
		#region Constructors

		// The IntPtr and initWithCoder constructors are required for items that need 
		// to be able to be created from a xib rather than from managed code

		public ProfileViewController (IntPtr handle) : base(handle)
		{

		}

		[Export("initWithCoder:")]
		public ProfileViewController (NSCoder coder) : base(coder)
		{

		}
		
		private UINavigationController _MSP;
		
		public ProfileViewController (UINavigationController mSPNavigationController) : base("ProfileViewController", null)
		{
			this._MSP = mSPNavigationController;
			this.root = CreateRoot ();
		}
		
		#endregion
		
		private RootElement root;
		private DialogViewController mainDv;
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
				
			var dv = new DialogViewController (root, true);			
			dv.TableView.BackgroundView = new UIImageView(Graphics.GetImgResource("fond"));
			dv.View.Frame = new RectangleF(0, 45, 320, 480 - 40 - 25);
			mainDv = dv;
			
			this.View.WillRemoveSubview(mainView);
			this.View.AddSubview(dv.View);
			
			var view = new UIView(new RectangleF(0, 40 , 320, 1));
			view.Layer.BackgroundColor = UIColor.LightGray.CGColor;
			this.View.AddSubview(view);
			
			Initialize();
		}
		
		private void Initialize()
		{		
			searchBtn.SetImage(Graphics.GetImgResource("search"), UIControlState.Normal);
			this.searchBtn.TouchDown += HandleSearchBtnhandleTouchDown;
		}

		void HandleSearchBtnhandleTouchDown (object sender, EventArgs e)
		{
			_MSP.DismissModalViewControllerAnimated(true);
			
			var search = new SearchViewController(_MSP);
			_MSP.PushViewController(search, true);
		}
				
		RootElement CreateRoot ()
		{
			return new RootElement ("Settings") {
				new Section (){
					new StringElement ("my posts", ()=>
	                   {
							Action act = ()=>
							{
								int userId = AppDelegateIPhone.AIphone.MainUser.Id;
								InvokeOnMainThread(()=>
			                    {
									var m = new MembersPhotoViewControler(AppDelegateIPhone.meNavigationController, userId, false);
									_MSP.PushViewController(m, true);
								});
							};
							AppDelegateIPhone.ShowRealLoading(View, "Loading posts", null, act);
						}
					),
					new StringElement("facebook friends", ()=>
					{
						Action act = () => LoadFacebookFriends();
						AppDelegateIPhone.ShowRealLoading(null, "Loading facebook friends", null, act);
					}),
					new StringElement ("my profile", ()=>
					{
						var pref = new PreferencesViewController(_MSP);
						_MSP.PushViewController(pref, true);
					}),					
					new StringElement ("other networks", ()=>
					{
						var vc = new SocialNetworksParentViewController(_MSP);
						_MSP.PushViewController(vc, true);
					}),
				},
				new Section(){
					new CustomHtmlElement("terms of service", "http://storage.21offserver.com:82/legal.html", _MSP),
					new CustomHtmlElement("help",  "http://storage.21offserver.com:82/help.html", _MSP),
					//"http://www.21off.net/help.html"
					//"http://www.21off.net/termsofservice.html"
					
				},
				
				new Section(){
					new StringElement("log out", () =>
	                {
						AppDelegateIPhone.AIphone.Logout();
						AppDelegateIPhone.AIphone.InitLoginPage();
					})
				}
					
			};
		}
		
		public void LoadFacebookFriends()
		{
			var facebookApp = new FaceBook.FaceBookApplication(this);
			if (!facebookApp.LoggedIn())
			{
				InvokeOnMainThread(()=>
				{
					var soc = new SocialNetworksParentViewController(_MSP);
					_MSP.PushViewController(soc, true);
				});								
				return;
			}
	
			List<Decimal> friends = AppDelegateIPhone.AIphone.FacebookServ.GetFriends();
			if (friends == null)
			{
				Util.LogException("friends", new Exception());
				return;
			}
			
			var socialIds = new List<long>();
			foreach (decimal d in friends)
			{
				socialIds.Add((long)d);
			}
			List<User> facebookFriends = AppDelegateIPhone.AIphone.UsersServ.GetSocialIds(socialIds, 1);
			if (facebookFriends == null)
			{
				Util.LogException("facebookFriends", new Exception());
				return;
			}
			if (facebookFriends.Count == 0)
			{
				InvokeOnMainThread(()=>
				{
					var alert = new UIAlertView("Search", "No facebook friend found", null, "Ok");
					alert.Show();
				});
				return;
			}
					
			facebookApp = new FaceBook.FaceBookApplication (this);
			InvokeOnMainThread(()=>
			{								
				var root = new RootElement("") { new Section() };
				var dv = new DialogViewController(root, true);
				var friendsFacebooksIds = new List<long>();
				
				foreach (User fuser in facebookFriends)
				{
					var user = new UserElementII(fuser, RelationType.Friends);
					root[0].Add(user);
				}
				foreach (long socialId in socialIds)
				{
					if (friends.Contains(socialId))
						continue;
					
					var guser = new GraphUser() { id = socialId };										
					var fbUser = new FbUserElement(guser, u => 
                    {													
						//facebookApp.HandleOpenURL(new NSUrl(u));
						//facebookApp.HandleOpenURL(NSUrl.FromString(u));
						//return;
						
						//WebViewController.OpenUrl (dv, u);
						return;
						
	  					var fac = new FacebookAuthorizationViewController("168889879843414", 
                             new string[] {"read_stream", "publish_stream", "user_groups"}, FbDisplayType.Touch);
						
					    fac.AccessToken += delegate(string accessToken, DateTime expires) {
						   //Logged in, got your accessToken here and when it expires!
						   //_MSP.PopViewControllerAnimated(true);
							fac.OpenUrl(u);
						 
						   //Do something else here (eg: Save the accessToken and expiry date to be used in your Graph API calls)
						 };
	 
	 					_MSP.PushViewController(fac, true);						 
					});
					root[0].Add(fbUser);
				}				
				
				var ev = new EmptyViewController(() => _MSP.PopViewControllerAnimated(true), "Facebook friends");
				dv.TableView.BackgroundView = new UIImageView(UIImage.FromBundle("Images/Ver4/fond"));
				dv.View.Frame = new RectangleF(0, 40, 320, 480 - 40);
				ev.Add(dv.View);
				_MSP.PushViewController(ev, false);
			});		
		}
		
		private FaceBook.FaceBookApplication facebookApp;
	}
}
