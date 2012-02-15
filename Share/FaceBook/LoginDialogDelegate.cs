using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using FacebookSdk;

namespace FaceBook
{
	public class LoginDialogDelegate : FBLoginDialogDelegate
	{		
		protected FaceBookApplication _facebookApp;
		
		#region Constructors
		
		public LoginDialogDelegate(FaceBookApplication facebookApp)
		{
			_facebookApp = facebookApp;
		}
		
		#endregion Constructors
		
		public override void FbDialogLogin(string accessToken, NSDate expirationDate)
		{
			Console.WriteLine("Fb Dialog Login");
			_facebookApp.SaveSessionData(true);
		}
				
		public override void FbDialogNotLogin(bool cancelled)
		{
			Console.WriteLine(cancelled ? "FB Dialog Cancelled" : "FB Dialog Not Login");
		}
	}
}

