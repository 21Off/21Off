
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MonoTouch.CoreGraphics;
using MonoTouch.Dialog.Utilities;
using System.Security.Cryptography;
namespace MonoTouch.Dialog.Utilities
{
	/// <summary>
	///    This interface needs to be implemented to be notified when an image
	///    has been downloaded.   The notification will happen on the UI thread.
	///    Upon notification, the code should call RequestImage again, this time
	///    the image will be loaded from the on-disk cache or the in-memory cache.
	/// </summary>
	public interface IImageUpdated {
		void UpdatedImage (Uri uri);
	}
}
