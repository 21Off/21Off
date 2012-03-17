
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using Krystalware.UploadHelper;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public class ShareService : AppServiceBase
	{
		public void PhotoShare(Image image)
		{			
			//var ttt = DataContractSerializer.Instance.Parse(image);
			var id = AppDelegateIPhone.AIphone.MainUser.Id;
			
			var we = new ManualResetEvent(false);			
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/ShareImage?ImageId={0}&SharerId={1}", 
				image.Id, id);
				
			JsonUtility.Launch(uri, false, s =>
			{
				try
				{
					var json = JsonArray.Load (s);
				}
				catch (Exception ex)
				{
					Util.LogException("PhotoShare", ex);
				}
				we.Set();
			});
			
			we.WaitOne(5000);
		}		
	}
	
	public class ReportService : AppServiceBase
	{
		public void ReportContent(Image image, string content)
		{			
			//var ttt = DataContractSerializer.Instance.Parse(image);
			var id = AppDelegateIPhone.AIphone.MainUser.Id;
			
			var we = new ManualResetEvent(false);			
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/ReportImage?ImageId={0}&IdUser={1}&Reason={2}", 
				image.Id, id, content);
				
			JsonUtility.Launch(uri, false, s =>
			{
				try
				{
					var json = JsonArray.Load (s);
					Console.WriteLine(json.ToString());
				}
				catch (Exception ex)
				{
					Util.LogException("Report Content", ex);
				}
				we.Set();
			});
			
			we.WaitOne(5000);
		}		
	}
}
