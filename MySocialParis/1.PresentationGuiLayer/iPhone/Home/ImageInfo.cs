
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MonoTouch.Dialog;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client.DataContracts;
namespace MSP.Client
{
	public class ImageInfo
	{
		public string Path {get;set;}
		public Image Img {get;set;}
		public int Index {get;set;}
	}
}
