
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using MonoTouch.CoreLocation;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
using MonoTouch.MapKit;
namespace MSP.Client
{
	public class RequestInfo
	{
		public Guid Id {get;set;}
		public RequestType Type {get;set;}
		public DateTime Time {get;set;}
		
		public RequestInfo(RequestType type)
		{
			Type = type;
			Time = DateTime.UtcNow;
			Id = Guid.NewGuid();
		}
	}
}
