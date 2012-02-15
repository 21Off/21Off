
using System;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Net;
using System.Threading;
using System.Web;
using FacebookN;
using MonoTouch.Foundation;
using MSP.Client.DataContracts;
namespace MSP.Client
{
	public class CDel : NSUrlConnectionDelegate
	{
		private NSUrlRequest req;
		
		public CDel(NSUrlRequest req)
		{
			this.req = req;
			data = new NSMutableData();			
		}				
		
		public override void FinishedLoading (NSUrlConnection connection)
		{
			//var query = HttpUtility.ParseQueryString(req.MainDocumentURL.Fragment);
			//Console.WriteLine(query);
			// TODO: Implement - see: http://go-mono.com/docs/index.aspx?link=T%3aMonoTouch.Foundation.ModelAttribute
		}
		
		NSMutableData data;
		
		public override void ReceivedResponse (NSUrlConnection connection, NSUrlResponse response)
		{
			// TODO: Implement - see: http://go-mono.com/docs/index.aspx?link=T%3aMonoTouch.Foundation.ModelAttribute
			
		}

		public override void ReceivedData (NSUrlConnection connection, NSData d)
		{
			data.AppendData(d);
		}
		
		public override void FailedWithError (NSUrlConnection connection, NSError error)
		{
	
		}
	}
}
