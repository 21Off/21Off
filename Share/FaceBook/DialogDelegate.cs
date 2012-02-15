using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using FacebookSdk;

namespace FaceBook
{
	public class DialogDelegate : FBDialogDelegate
	{		
		protected FaceBookApplication _facebookApp;
		
		#region Constructors
		
		public DialogDelegate(FaceBookApplication facebookApp)
		{
			_facebookApp = facebookApp;
		}
		
		#endregion Constructors
		
		public override void DialogDidComplete (FBDialog dialog)
		{
		}
		
		public override void DialogCompleteWithUrl (NSUrl url)
		{
		}
		
		public override void DialogDidNotCompleteWithUrl (NSUrl url)
		{
		}

		public override void DialogDidNotComplete (FBDialog dialog)
		{
		}
		
		public override void Dialog (FBDialog dialog, NSError error)
		{
		}
		
		public override bool Dialog (FBDialog dialog, NSUrl url)
		{
			return true;
		}
	}
}
