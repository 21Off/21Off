using System;
using System.Collections.Generic;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using System.IO;

namespace MSP.Client
{	
	public class NSUrlConnectionWrapper:NSUrlConnection {		
		private static Dictionary<string, NSUrlConnectionWrapper> Connections = new Dictionary<string, NSUrlConnectionWrapper>();

		public static void KillAllConnections() {

			foreach (NSUrlConnectionWrapper c in Connections.Values) {
				c.Cancel();
			}
			Connections.Clear();
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
		}

		protected static void KillConnection(string name) {
			Connections[name].Cancel();
			Connections.Remove(name);
		}

		public static void ConnectionEnded(string name) {
			Connections.Remove(name);
		}

		public static bool IsDownloading(string name) {
			return Connections.ContainsKey(name);	
		}
		
		public NSUrlConnectionWrapper(NSUrlRequest request, EscozUrlDelegate del) : base(request, del, true)
		{
			if (Connections.ContainsKey(del.Name)) {
				KillConnection(del.Name);
			}
			Connections.Add(del.Name, this);			
		}
		
		public NSUrlConnectionWrapper(NSUrlRequest request, EscozUrlStreamDelegate del) : base(request, del, true)
		{
			if (Connections.ContainsKey(del.Name)) {
				KillConnection(del.Name);
			}
			Connections.Add(del.Name, this);			
		}		
		
		public NSUrlConnectionWrapper(string name, NSUrlRequest request, Action<string> c):base(request, new EscozUrlDelegate(name, c), true) {
			if (Connections.ContainsKey(name)) {
				KillConnection(name);
			}
			Connections.Add(name, this);
		}
		
		public NSUrlConnectionWrapper(string name, NSUrlRequest request, Action<Stream> c)
		: base(request, new EscozUrlStreamDelegate(name, c), true) {
			if (Connections.ContainsKey(name)) {
				KillConnection(name);
			}
			Connections.Add(name, this);
		}		

		public NSUrlConnectionWrapper(string name, NSUrlRequest request, Action<string> success, Action failure):base(request, new EscozUrlDelegate(name, success, failure), true) {
			if (Connections.ContainsKey(name)) {
				KillConnection(name);
			}
			Connections.Add(name, this);
		}
		
		public NSUrlConnectionWrapper(string name, NSUrlRequest request, Action<Stream> success, Action failure)
			: base(request, new EscozUrlStreamDelegate(name, success, failure), true) {
			if (Connections.ContainsKey(name)) {
				KillConnection(name);
			}
			Connections.Add(name, this);
		}		
	}	
	
	public class ConnectionDelegate : NSUrlConnectionDelegate {
	
		Action<NSMutableData> _success;
		NSMutableData imageData;

		public ConnectionDelegate(Action<NSMutableData> success){
			_success = success;
		}

		public override void ReceivedData (NSUrlConnection connection, NSData data)
		{
			if (imageData==null)
				imageData = new NSMutableData();

			imageData.AppendData(data);	
		}

		public override void FinishedLoading (NSUrlConnection connection)
		{
			if (_success != null)
				_success(imageData);
		}
	}
	
	public class EscozUrlStreamDelegate : NSUrlConnectionDelegate {
		Action<Stream> callback;
		Action _failure;
		NSMutableData data;
		long totalLength = 0;
		string _name;
		
		public string Name {get {return _name;}}			

		public EscozUrlStreamDelegate(string name, Action<Stream> success) {
			_name = name;
			callback = success;
			data = new NSMutableData();
		}

		public EscozUrlStreamDelegate(string name, Action<Stream> success, Action failure) : this(name, success) {			
			_failure = failure;
		}

		public override void ReceivedData (NSUrlConnection connection, NSData d)
		{
			totalLength += d.Length;
			data.AppendData(d);
		}		

		public override void FailedWithError (NSUrlConnection connection, NSError error)
		{
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
			if (_failure!=null)
				_failure();
		}

		public override void FinishedLoading (NSUrlConnection connection)
		{
			NSUrlConnectionWrapper.ConnectionEnded(_name);
			unsafe 
			{
				var stream = new UnmanagedMemoryStream((byte*)data.MutableBytes, data.Length);
				callback(stream);			
			}			
		}
	}	
	
	public class EscozUrlDelegate : NSUrlConnectionDelegate {
		Action<string> callback;
		Action _failure;
		NSMutableData data;
		string _name;
		
		public string Name {get {return _name;}}			

		public EscozUrlDelegate(string name, Action<string> success) {
			_name = name;
			callback = success;
			data = new NSMutableData();
		}

		public EscozUrlDelegate(string name, Action<string> success, Action failure) : this(name, success) {			
			_failure = failure;
		}

		public override void ReceivedData (NSUrlConnection connection, NSData d)
		{
			data.AppendData(d);
		}
		
		/*
		public override bool CanAuthenticateAgainstProtectionSpace (NSUrlConnection connection, NSUrlProtectionSpace protectionSpace)
		{
			return true;
		}		

		bool showError = true;
				
		public override void ReceivedAuthenticationChallenge (NSUrlConnection connection, NSUrlAuthenticationChallenge challenge)
		{
			if (challenge.PreviousFailureCount>0){
				showError = false;
				challenge.Sender.CancelAuthenticationChallenge(challenge);
				//Application.AuthenticationFailure();
				return;
			}

			if (challenge.ProtectionSpace.AuthenticationMethod=="NSURLAuthenticationMethodServerTrust")
				challenge.Sender.UseCredentials(NSUrlCredential.FromTrust(challenge.ProtectionSpace.ServerTrust), challenge);
			
			if (challenge.ProtectionSpace.AuthenticationMethod=="NSURLAuthenticationMethodDefault" && 
			    Application.Account!=null && Application.Account.Login!=null && Application.Account.Password!=null) {
				challenge.Sender.UseCredentials(NSUrlCredential.FromUserPasswordPersistance(
				          Application.Account.Login, Application.Account.Password, NSUrlCredentialPersistence.None), challenge);

			}
		}
		*/

		public override void FailedWithError (NSUrlConnection connection, NSError error)
		{
			UIApplication.SharedApplication.NetworkActivityIndicatorVisible = false;
			if (_failure!=null)
				_failure();
		}

		public override void FinishedLoading (NSUrlConnection connection)
		{
			NSUrlConnectionWrapper.ConnectionEnded(_name);
			callback(data.ToString());
		}
	}	
}

