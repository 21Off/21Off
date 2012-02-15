using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using FacebookSdk;

namespace FaceBook
{
	public class RequestDelegate : FBRequestDelegate
	{
		protected FaceBookApplication _facebookApp;
		
		#region Constructors
		
		public RequestDelegate(FaceBookApplication facebookApp)
		{
			_facebookApp = facebookApp;
		}
		
		#endregion Constructors
		
		public override void RequestLoading(FBRequest request) {}
		
		public override void Request (FBRequest request, NSUrlResponse response) {}
		
		public override void Request (FBRequest request, NSError error) {}
		
		public override void Request (FBRequest request, NSObject result)
		{
			NSDictionary dict;
			
			if(result is NSDictionary)
			{	
				dict = result as NSDictionary;
			}
			else if(result is NSArray)
			{
				var arr = (NSArray)result;
				dict = new NSDictionary(arr.ValueAt(0));
			}
			else if(result is NSData)
			{
				var data = (NSData)result;
				Console.WriteLine("post_id : " + data.ToString());
				return;
			}
			else
			{
				throw new Exception("cannot handle result in FBRequestDelegate callback");
			}
			
			if (dict.ObjectForKey(new NSString("owner")) != null)
		    {
			     Console.WriteLine("Photo upload Success");
			}
			else 
			{
				NSObject name =	dict.ObjectForKey(new NSString("name"));
			    Console.WriteLine(name.ToString());
			}
		}
	
		public override void Request (FBRequest request, NSData data)
		{}
	}
}

