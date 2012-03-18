using System;
using System.IO;
using System.Net;
using System.Threading;
using MonoTouch.Foundation;
using MSP.Client;

namespace TweetStation
{
	public static class Downloader
	{
		public static bool DownloadImage(Uri url, string tempPath)
		{						
			var webRequest = new HttpWebRequest(url);
			webRequest.Method = "GET";
			var wr = webRequest.GetResponse();
			using (var rs = wr.GetResponseStream())
			{
				int readTotal = 0;
				using (var fs = new FileStream(tempPath, FileMode.Create))
				{			
					byte[] bytes = new byte[1024];
					int read = 0;
					while ((read = rs.Read(bytes, 0, 1024)) > 0)
					{
						fs.Write(bytes, 0, read);
						readTotal += read;
					}
				}
				long contLeng = 0;
				object contLengStr = wr.Headers["ContentLength"];
				if (contLengStr != null && long.TryParse((string)contLengStr, out contLeng))
				{				
					return readTotal == contLeng;
				}
				return true;
			}
		}
		
		static NSString invoker = new NSString ("");
		
		public static bool DownloadImage1(Uri url, string tempPath)
		{
			bool res = false;
			
						
			var request = new NSUrlRequest(new NSUrl(url.OriginalString));			
			var wait = new ManualResetEventSlim(false);
			
			invoker.BeginInvokeOnMainThread(()=>
            {
				new NSUrlConnection(request, new ConnectionDelegate(data =>
				{					
					NSError err = null;
					if (data.Save (tempPath, false, out err)) 
					{
						res = true;
					}					
					wait.Set();
				}), true);
			});
			wait.Wait();
			
			return res;
		}
		
		public static bool DownloadImage3(Uri url, string tempPath)
		{		
			bool res = true;			
			var wait = new ManualResetEventSlim(false);
			
			JsonUtility.LaunchDown(url.OriginalString, false, rs =>
			{
				try
				{
					int readTotal = 0;
					
					var ms = new MemoryStream ();
					rs.CopyStream (ms);
					ms.Position = 0;
					rs.Close ();
					rs = ms;
					
					using (var fs = new FileStream(tempPath, FileMode.Create))
					{	
						byte[] bytes = new byte[1024];
						int read = 0;
						while ((read = rs.Read(bytes, 0, 1024)) > 0)
						{
							fs.Write(bytes, 0, read);
							readTotal += read;
						}					
					}
				}
				catch (Exception ex)
				{
					Util.LogException("DownloadImage", ex);
					res = false;
				}
				
				wait.Set();
			});
			
			wait.Wait();
			
			return res;
		}			
	}
}
