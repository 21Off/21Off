
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
using Share;

namespace MSP.Client
{
	public class InstagramService
	{
		public EnvelopePopular GetPopular2()
		{
			EnvelopePopular res = null;
			
			string uri = "https://api.instagram.com/v1/media/popular?client_id=04d5d0f3bf304a449d3f2aeb74b91b54";
			
			var we = new ManualResetEvent(false);
			
			JsonUtility.Launch(uri, false, (Stream s) =>
	        {
				try
				{
					//var res = ServiceStack.Text.JsonSerializer.DeserializeFromString<EnvelopePopular>(s);
					res = new EnvelopePopular();
					
					var json = (JsonObject)JsonObject.Load (s);
					foreach (var eKey in json.Keys)
					{
						var eVal = json[eKey];
						if (eVal == null)
							continue;
						
						if (eKey == "meta")
						{
							res.meta = Des<Meta>(eVal.ToString());
						}
						if (eKey == "data")
						{
							res.data = GetInstapopulars((JsonArray)eVal);
						}
						if (eKey == "pagination")
						{
							res.pagination = Des<Pagination>(eVal.ToString());
						}
					}				
				}
				catch (Exception ex)
				{
					Util.LogException("GetPopular", ex);
				}
				we.Set();
			});
			
			we.WaitOne();
			
			return res;
		}
		
		private List<InstaPopular> GetInstapopulars(JsonArray ar)
		{
			List<InstaPopular> res = new List<InstaPopular>();
			
			foreach (JsonValue p in ar)
			{
				if (p.JsonType == JsonType.Object && p is JsonObject)
				{
/*
tags
location
comments
filter
created_time
link
likes
images
caption
type
id
user
*/										
					var popular = new InstaPopular();
					
					var jsonObj = (JsonObject)p;
					foreach (string key in jsonObj.Keys)
					{
						var val = jsonObj[key];
						if (val == null)
							continue;
						
						if (key == "location")
						{
							popular.location = Des<InstaLocation>(val.ToString());
						}
						if (key == "link")
						{
							popular.link = val.ToString();
						}
						if (key == "id")
						{
							popular.id = val.ToString();
						}						
						if (key == "user")
						{
							popular.user = Des<InstaUser>(val.ToString());
						}						
					}
					
					res.Add(popular);
				}
			}
			
			return res;
		}
		
		public static T Des<T>(string s)
		{
			return ServiceStack.Text.JsonSerializer.DeserializeFromString<T>(s);
		}
	}
}
