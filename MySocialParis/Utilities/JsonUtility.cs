using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using MonoTouch.Foundation;
using System.Threading;
using System.Collections.Specialized;
using System.Text;

namespace MSP.Client
{
	public class JsonUtility
	{				
		internal struct Request {
			public string Url;
			public Action<Stream> Callback;
			public bool CallbackOnMainThread;
			
			public Request (string url, bool callbackOnMainThread, Action<Stream> callback)
			{
				Url = url;
				Callback = callback;
				CallbackOnMainThread = callbackOnMainThread;
			}
		}
		
        private const string UserName = "user";
        private const string Password = "pass";		
		
		const int MaxPending = 200;
		static Queue<Request> queue = new Queue<Request> ();
		static int pending;		
		
		static long lastLaunchTick;
		static object minuteLock = new object ();				
		
		public static void Launch (string url, bool callbackOnMainThread, Action<string> callback)
		{
			Util.PushNetworkActive ();
			Uri uri = new Uri (url);
			
			Util.Log(uri.OriginalString);
			
			// Wake up 3G if it has been more than 3 minutes
			lock (minuteLock){
				var nowTicks = DateTime.UtcNow.Ticks;
				if (nowTicks-lastLaunchTick > TimeSpan.TicksPerMinute*3)
					MonoTouch.ObjCRuntime.Runtime.StartWWAN (uri);
				lastLaunchTick = nowTicks;
			}			
			
			var req = new NSMutableUrlRequest(new NSUrl(url), NSUrlRequestCachePolicy.ReloadIgnoringCacheData,5);			
			//req["User-Agent"]=_UserAgent;			
			var mydelegate = new EscozUrlDelegate(url,(result)=>
           { 				
				try
				{
					lock (queue)
						pending--;
					
					Util.PopNetworkActive ();					
					
					if (callbackOnMainThread)
						invoker.BeginInvokeOnMainThread (delegate { callback(result); });
					else 
						callback(result);
					
				} catch (Exception e){
					Util.LogException ("Launch", e);
				}
				lock (queue){
					if (queue.Count > 0){
						var nextRequest = queue.Dequeue ();
						Launch (nextRequest.Url, nextRequest.CallbackOnMainThread, nextRequest.Callback);
					}
				}
			});
			//new NSUrlConnectionWrapper(req, mydelegate);
			invoker.BeginInvokeOnMainThread(()=>
            {
				new NSUrlConnection(req, mydelegate, true);
			});
		}
		
		public static void LaunchDown (string url, bool callbackOnMainThread, Action<Stream> callback)
		{
			Util.PushNetworkActive ();
			Uri uri = new Uri (url);			
			
			// Wake up 3G if it has been more than 3 minutes
			lock (minuteLock){
				var nowTicks = DateTime.UtcNow.Ticks;
				if (nowTicks-lastLaunchTick > TimeSpan.TicksPerMinute*3)
					MonoTouch.ObjCRuntime.Runtime.StartWWAN (uri);
				lastLaunchTick = nowTicks;
			}			
			
			var req = new NSMutableUrlRequest(new NSUrl(url), NSUrlRequestCachePolicy.ReloadIgnoringCacheData,5);			
			//req["User-Agent"]=_UserAgent;			
			var mydelegate = new EscozUrlStreamDelegate(url,(result)=>
           { 				
				try
				{
					
					Util.PopNetworkActive ();					
					
					if (callbackOnMainThread)
						invoker.BeginInvokeOnMainThread (delegate { callback(result); });
					else 
						callback(result);
					
				} catch (Exception e){
					Util.LogException("LaunchDown", e);
				}
			}
			);
			//new NSUrlConnectionWrapper(req, mydelegate);
			invoker.BeginInvokeOnMainThread(()=>
            {
				new NSUrlConnection(req, mydelegate, true);
			});
		}		
		
		public static void Launch (string url, bool callbackOnMainThread, Action<Stream> callback)
		{
			Launch(url, callbackOnMainThread, callback, null);
		}
		
		public static void HttpUploadFile(string url, string file, string paramName, string contentType, NameValueCollection nvc) 
		{
	        Util.Log(string.Format("Uploading {0} to {1}", file, url));
	        string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
	        byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");
	
	        HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
	        wr.ContentType = "multipart/form-data; boundary=" + boundary;
	        wr.Method = "POST";
	        wr.KeepAlive = true;
	        //wr.Credentials = System.Net.CredentialCache.DefaultCredentials;
	
	        using (Stream rs = wr.GetRequestStream())
			{	
		        string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
		        foreach (string key in nvc.Keys)
		        {
		            rs.Write(boundarybytes, 0, boundarybytes.Length);
		            string formitem = string.Format(formdataTemplate, key, nvc[key]);
		            byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
		            rs.Write(formitembytes, 0, formitembytes.Length);
		        }
		        rs.Write(boundarybytes, 0, boundarybytes.Length);
		
		        string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
		        string header = string.Format(headerTemplate, paramName, file, contentType);
		        byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
		        rs.Write(headerbytes, 0, headerbytes.Length);
		
		        using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
				{
			        byte[] buffer = new byte[4096];
			        int bytesRead = 0;
			        while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0) {
			            rs.Write(buffer, 0, bytesRead);
			        }
				}
		
		        byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
		        rs.Write(trailer, 0, trailer.Length);
			}
	
	        WebResponse wresp = null;
	        try {
	            wresp = wr.GetResponse();
	            Stream stream2 = wresp.GetResponseStream();
	            StreamReader reader2 = new StreamReader(stream2);
	            Util.Log(string.Format("File uploaded, server response is: {0}", reader2.ReadToEnd()));
	        } catch(Exception ex) {
	            Util.LogException ("Error uploading file", ex);
	            if(wresp != null) {
	                wresp.Close();
	                wresp = null;
	            }
	        } finally {
	            wr = null;
	        }
	    }		
		
		public static void Upload (string url, object obj, bool callbackOnMainThread, Action<Stream> callback)
		{
			Util.PushNetworkActive ();
			var uri = new Uri (url);
			
			// Wake up 3G if it has been more than 3 minutes		
			lock (minuteLock){
				var nowTicks = DateTime.UtcNow.Ticks;
				if (nowTicks-lastLaunchTick > TimeSpan.TicksPerMinute*3)
					MonoTouch.ObjCRuntime.Runtime.StartWWAN (uri);
				lastLaunchTick = nowTicks;
			}
			
			var request = (HttpWebRequest) WebRequest.Create (uri);
			request.AutomaticDecompression = DecompressionMethods.GZip;
			request.Method = "PUT";
			request.ContentType = "application/json";
			request.PreAuthenticate = true;
			request.Credentials = new NetworkCredential(UserName, Password);
			
			request.BeginGetRequestStream(ar => 
			{
				Stream reqStream = null;
				
				try
				{
					reqStream = (Stream) request.EndGetRequestStream (ar);					
					ServiceStack.Text.JsonSerializer.SerializeToStream(obj, obj.GetType(), reqStream);
					reqStream.Close();
					
					request.BeginGetResponse (aq => 
					{						
						Stream stream = null;
						
						try {
							var resp = (HttpWebResponse) request.EndGetResponse (aq);
							stream = resp.GetResponseStream ();
							
							// Since the stream will deliver in chunks, make a copy before passing to the main UI
							if (callbackOnMainThread){
								var ms = new MemoryStream ();
								CopyStream (stream, ms);
								ms.Position = 0;
								stream.Close ();
								stream = ms;
							}
							
							Util.PopNetworkActive();
							
							InvokeCallback(callbackOnMainThread, callback, stream);
						}
						catch (Exception ex)
						{
							Util.PopNetworkActive();
							Util.LogException("Upload", ex);
							InvokeCallback(callbackOnMainThread, callback);
						}
						
					}, null);
				} 
				catch (Exception e)
				{
					Util.PopNetworkActive();
					Util.LogException("Upload", e);
					InvokeCallback(callbackOnMainThread, callback);
				}	
			}, null);
		}
		
		public static void Launch (string url, bool callbackOnMainThread, Action<Stream> callback, string method)
		{
			Util.PushNetworkActive ();
			var uri = new Uri (url);
			
			// Wake up 3G if it has been more than 3 minutes
			lock (minuteLock){
				var nowTicks = DateTime.UtcNow.Ticks;
				if (nowTicks-lastLaunchTick > TimeSpan.TicksPerMinute*3)
					MonoTouch.ObjCRuntime.Runtime.StartWWAN (uri);
				lastLaunchTick = nowTicks;
			}			
			
			var request = (HttpWebRequest) WebRequest.Create (uri);
			request.AutomaticDecompression = DecompressionMethods.GZip;
			if (method != null) request.Method = method;
			//request.Headers [HttpRequestHeader.Authorization] = OAuthAuthorizer.AuthorizeRequest (OAuthConfig, OAuthToken, OAuthTokenSecret, "GET", uri, null);			
				
			request.BeginGetResponse (ar => {
				try {
					lock (queue)
						pending--;
					Util.PopNetworkActive ();
					Stream stream = null;
					
					try {
						var response = (HttpWebResponse) request.EndGetResponse (ar);
						stream = response.GetResponseStream ();

						// Since the stream will deliver in chunks, make a copy before passing to the main UI
						if (callbackOnMainThread){
							var ms = new MemoryStream ();
							CopyStream (stream, ms);
							ms.Position = 0;
							stream.Close ();
							stream = ms;
						}
					} catch (WebException we){
						var response = we.Response as HttpWebResponse;
						if (response != null){
							switch (response.StatusCode){
							case HttpStatusCode.Unauthorized:
								// This is the case of sharing two keys
								break;
							}
							stream = null;
						}
						Util.LogException ("Launch webException:", we);
					} catch (Exception e) {
						Util.LogException ("Launch1:", e);
						stream = null;
					}
					
					InvokeCallback(callbackOnMainThread, callback, stream);					
				} catch (Exception e){
					Util.LogException ("Launch2:", e);
				}
				lock (queue){
					if (queue.Count > 0){
						var nextRequest = queue.Dequeue ();
						Launch (nextRequest.Url, nextRequest.CallbackOnMainThread, nextRequest.Callback);
					}
				}
			}, null);
		}	
		
		public static void InvokeCallback(bool callbackOnMainThread, Action<Stream> callback, Stream stream = null)
		{
			if (callbackOnMainThread)
				invoker.BeginInvokeOnMainThread (delegate { InvokeCallback (callback, stream); });
			else 
				InvokeCallback (callback, stream);
		}		
		
		static void InvokeCallback (Action<Stream> callback, Stream stream)
		{
			try {
				callback (stream);
			} catch  (Exception ex){
				Util.Log("InvokeCallback", ex);
			}
			if (stream != null)
				stream.Close ();
		}
		
		static NSString invoker = new NSString ("");		
		
		// Temporary
		static void CopyStream (Stream source, Stream dest)
		{
			var buffer = new byte [4096];
			int n = 0;
			long total = 0;

			while ((n = source.Read (buffer, 0, buffer.Length)) != 0){
				total += n;
				dest.Write (buffer, 0, n);
			}
		}	
	}
}

