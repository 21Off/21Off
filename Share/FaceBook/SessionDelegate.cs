using System;
using FacebookSdk;

namespace FaceBook
{
	public class SessionDelegate : FBSessionDelegate
	{
		protected FaceBookApplication _facebookApp;
		
		#region Constructors
		
		public SessionDelegate(FaceBookApplication facebookApp)
		{
			_facebookApp = facebookApp;
		}
		
		#endregion Constructors
		
		public override void FbDidLogin()
		{
			Console.WriteLine("Fb Did Login");
			_facebookApp.SaveSessionData(true);
		}
		
		public override void FbDidLogout()
		{
			Console.WriteLine("Fb Did Logout");
			_facebookApp.SaveSessionData(false);
		}
		
		public override void FbDidNotLogin(bool cancelled)
		{
			Console.WriteLine("FB Did Not Login");
		}
	}
}

