using System;
using System.IO;
using System.Net;
using System.Threading;
using MonoTouch.UIKit;
using MonoTouch.Facebook.Authorization;
using MonoTouch.Foundation;

namespace SocialLogin
{
	public interface ISocialApplication
	{
		event Action OnLoginComplete;
	}
}
