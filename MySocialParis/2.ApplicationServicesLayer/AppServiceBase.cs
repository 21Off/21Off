using System;
using System.Linq;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public class AppServiceBase
	{
		public const string WebServiceHostUrl = "http://storage.21offserver.com";
		//internal JsonServiceClient gateway = new JsonServiceClient(WebServiceHostUrl + "/json/syncreply");
		
		public AppServiceBase()
		{			
			
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
