using System;
using System.Linq;
using MSP.Client.DataContracts;
using System.Json;
using System.Threading;

namespace MSP.Client
{
	public class AppServiceBase
	{
		public const string WebServiceHostUrl = "http://storage.21offserver.com";
		//internal JsonServiceClient gateway = new JsonServiceClient(WebServiceHostUrl + "/json/syncreply");
		
		public AppServiceBase()
		{			
			
		}
				
		public TR GetServiceResponse<TR, T>(T request, Func<JsonValue, TR> GetResponse, string operationName = null) where T : class
		{
			TR tr = default(TR);
			
			if (string.IsNullOrWhiteSpace(operationName))
			{
				operationName = typeof(T).Name;
			}
			
			var we = new ManualResetEvent(false);
			string uri = string.Format("{0}/json/syncreply/{1}", WebServiceHostUrl, operationName);
			
			JsonUtility.Upload (uri, request, false, s =>
			{
				
				try
				{
					var json = JsonArray.Load (s);
					
					tr = GetResponse(json);
				}
				catch (Exception ex)
				{
					Util.LogException(operationName, ex);
				}
				we.Set();
			});
			
			we.WaitOne(20000);
			
			return tr;
		}	
		
		public static TR GetServiceResponse<TR>(object request, string operationName = null)
		{
			TR tr = default(TR);
			
			if (string.IsNullOrWhiteSpace(operationName))
			{
				operationName = request.GetType().Name;
			}
			
			var we = new ManualResetEvent(false);
			string uri = string.Format("{0}/json/syncreply/{1}", WebServiceHostUrl, operationName);
			
			JsonUtility.Upload (uri, request, false, s =>
			{
				
				try
				{
					var json = JsonArray.Load (s);
					
					tr = new ServiceStack.Text.JsonSerializer<TR>().DeserializeFromString(json.ToString());
				}
				catch (Exception ex)
				{
					Util.LogException(operationName, ex);
				}
				we.Set();
			});
			
			we.WaitOne(20000);
			
			return tr;
		}		
		
		/*
		
		public ServiceClientBase CreateRestClient()
		{	
			return new JsonServiceClient(WebServiceHostUrl);  //Best choice for Ajax web apps, 3x faster than XML
			
			//return new XmlServiceClient(WebServiceHostUrl); //Ubiquitous structured data format best for supporting non .NET clients
			//return new JsvServiceClient(WebServiceHostUrl); //Fastest, most compact and resilient format great for .NET to .NET client > server
		}
		*/		
	}
}
