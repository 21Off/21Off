using System;
using System.Drawing;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.Collections.Generic;
using MonoTouch.CoreGraphics;
using System.Threading;
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
		
		public override void ViewDidLoad ()
		{
			base.ViewDidLoad ();
				
			var dv = new DialogViewController (root, true);			
			dv.TableView.BackgroundView = new UIImageView(Graphics.GetImgResource("fond"));
			dv.View.Frame = new RectangleF(0, 45, 320, 480 - 40 - 25);
			
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
					new StringElement ("my posts"
						/*, el => 
	                 	{				
							int userId = AppDelegateIPhone.AIphone.MainUser.Id;
							return new MembersPhotoViewControler(AppDelegateIPhone.meNavigationController, userId);
						}
						*/, ()=>
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
						Action act = () =>
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
															
							InvokeOnMainThread(()=>
							{
								var root = new RootElement("") { new Section() };
								foreach (User fuser in facebookFriends)
								{
									var user = new UserElementII(fuser, RelationType.Friends);
									root[0].Add(user);
								}
								
								var dv = new DialogViewController(root);
								
								var ev = new EmptyViewController(() => _MSP.PopViewControllerAnimated(true), "Facebook friends");
								dv.TableView.BackgroundView = new UIImageView(UIImage.FromBundle("Images/Ver4/fond"));
								dv.View.Frame = new RectangleF(0, 40, 320, 480 - 40);
								ev.Add(dv.View);
								_MSP.PushViewController(ev, true);
							});
						};
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
	}
	
	public class FbUserElement : OwnerDrawnElement
	{
		private static object lock_graph = new object();
		private static Dictionary<decimal, GraphUser> graph = new Dictionary<decimal, GraphUser>();
		
		private GraphUser _User;
		private UIFont fromFont = UIFont.BoldSystemFontOfSize(14.0f);
		private UIImage userImage;
		
		public FbUserElement(GraphUser user) : base(UITableViewCellStyle.Default, "FbUserElement")
		{
			_User = user;
		}
		
		private NSString nss = new NSString("x");
		
		public override void Draw (RectangleF bounds, CGContext context, UIView view)
		{
			UIColor.White.SetFill ();
			context.FillRect (bounds);
			
			//if (userImage == null)
			//	userImage = UIImageUtils.GetPreview (string.Format("Images/Profiles/{0}.jpg", _User.Id), new SizeF (40, 40));

			if (userImage != null)
				context.DrawImage(new RectangleF(0, 0, 40, 40), userImage.CGImage);
			
			UIColor.Black.SetColor ();
			if (_User.name != null)			
				view.DrawString(_User.name, new RectangleF(50, 5, bounds.Width/2, 10 ), fromFont, UILineBreakMode.TailTruncation);
			else
			{
				ThreadPool.QueueUserWorkItem(o =>
				{					
					GraphUser gUser = AppDelegateIPhone.AIphone.FacebookServ.GetFriend(_User.id);
					if (gUser == null)
						return;
					
					lock (lock_graph)
					{
						graph[gUser.id] = gUser;
					}
					
					if (gUser.id == _User.id)
					{
						_User = gUser;						
						nss.InvokeOnMainThread(()=>
						{
							var tv = this.GetContainerTableView();
							if (tv != null)
							{
								var cell = GetCell(tv);
								if (cell != null)
									cell.SetNeedsDisplay();
							}
						});							
					}
					else
						nss.InvokeOnMainThread(()=> view.SetNeedsDisplay());
				});
			}
		}
		
		public override float Height (RectangleF bounds)
		{
			return 40.0f;
		}
	}
}
