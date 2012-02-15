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
	public class KeywordsService : AppServiceBase
	{
		public static Keyword JsonToKeyword(JsonObject obj)
		{			
			var keyword = new Keyword()
			{				
				Id = Convert.ToInt32(obj["Id"].ToString()),
				ParentId = Convert.ToInt32(obj["ParentId"].ToString()),
				Name = obj["Name"].ToString().Replace("\"",""),						
				//Time =  DateTime.ParseExact (obj ["Time"], "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture),
			};			
			
			return keyword;
		}
		
		public static SimilarImage JsonToSimilarImage(JsonObject obj)
		{			
			var keyword = new SimilarImage()
			{				
				Image = ImagesService.JsonToImage((JsonObject)obj["Image"]),
				Keyword = obj.ContainsKey("Keyword")? obj["Keyword"].ToString().Replace("\"","") : null,
			};
			
			return keyword;
		}		
		
		public SearchImagesResponse GetSimilarImages(string name, long since)
		{
			var resp = new SearchImagesResponse();
			
			var we = new ManualResetEvent(false);
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/SearchImagesByKeyword?Name={0}&Since={1}&Count=20",
				name, since);			
						
			JsonUtility.Launch(uri, false, s =>
	        {
				var similarImages = new List<SimilarImage>();
				
				var json = JsonArray.Load (s);
				foreach (JsonObject obj in json["Images"])
				{		
					try
					{
						similarImages.Add(JsonToSimilarImage(obj));
					}
					catch (Exception ex)
					{
						Util.LogException("GetSimilarImages", ex);
					}
				}
				DateTime? time = ActivitiesService.JsonToTime(json["Time"]);
				if (time.HasValue)
					resp.Time = time.Value;
				
				resp.Images = similarImages;
				
				we.Set();
			});
			
			we.WaitOne(10000);
			
			return resp;			
		}
		
		public IEnumerable<Keyword> GetKeywordsOfImage(int id)
		{
			/*
			var restClient = CreateRestClient();
			var response = restClient.Send<KeywordsResponse>(
                     new Keywords() { ImageId = id });
			
			if (response.ResponseStatus.ErrorCode != null)
				throw new Exception(response.ResponseStatus.ErrorCode);
			
			return response.Keywords;
			*/
			
			var keywords = new List<Keyword>();			
			var we = new ManualResetEvent(false);
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/Keywords?ImageId={0}", id);
			
			JsonUtility.Launch(uri, false, s =>
	        {
				var json = JsonArray.Load (s);
				foreach (JsonObject obj in json["Keywords"])
				{		
					try
					{
						keywords.Add(JsonToKeyword(obj));
					}
					catch (Exception ex)
					{
						Util.LogException("GetKeywordsOfImage", ex);
					}
				}
				we.Set();
			});
			
			we.WaitOne(10000);
			
			return keywords;
		}
		
		public void PutNewKeyword(Keyword keyword)
		{	
			var cms = new Keywords() { Keyword = keyword };
			var uri = string.Format("http://storage.21offserver.com/json/syncreply/Keywords");
			
			var request = (HttpWebRequest) WebRequest.Create (uri);
			request.Method = "PUT";	
			using (var reqStream = request.GetRequestStream())
			{
				ServiceStack.Text.JsonSerializer.SerializeToStream(cms, typeof(Keywords), reqStream);
				//reqStream.Write(bytes, 0, bytes.Length);
			};
			using (var response = request.GetResponse())
			{
				using (var stream = response.GetResponseStream())
				{
					var responseString = new StreamReader(stream).ReadToEnd();
				}
			}			
		}
		
		public void DeleteKeyword(int idKeyword)
		{			
			var we = new ManualResetEvent(false);			
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/keywords?Id={0}", idKeyword);			
			
			JsonUtility.Launch(uri, false, s =>
			{
				try
				{
					var json = JsonArray.Load (s);
					Console.WriteLine(json.ToString());
				}
				catch (Exception ex)
				{
					Util.LogException("DeleteKeyword", ex);
				}
				we.Set();
			}, "DELETE");
			
			we.WaitOne(5000);			
		}
	}
	
}
