using System;

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
