
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Net;
using System.Threading;
using MonoTouch.Foundation;
using MonoTouch.UIKit;
using MSP.Client;
namespace TweetStation
{
	public static class StreamExtensions
	{
		public static void CopyStream (this Stream source, Stream dest)
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
