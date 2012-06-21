using System;
using TweetStation;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public static class UrlStore
	{
		//http://storage.21offserver.com/files
		public const string streamingUrl = "http://storage.21offserver.com:82/streaming";
		
		public static string GetDBImagePath(Tuple<long, long, SizeDB> pendReq)
		{
			return GetDBImagePath(pendReq.Item1, pendReq.Item2,pendReq.Item3);
		}
		
		public static string GetDBImagePath(long id, long userid, SizeDB sizeDB)
		{		
			string path = ImageStore.FileDB;
			
			if (sizeDB == SizeDB.Size100)
			{
				path = ImageStore.FileDB100;
			}
			if (sizeDB == SizeDB.Size75)
			{
				path = ImageStore.FileDB75;
			}
			if (sizeDB == SizeDB.Size50)
			{
				path = ImageStore.FileDB50;
			}
			if (sizeDB == SizeDB.SizeFacebook)
			{
				return ImageStore.ProfilesFacebook + userid + ".png";
			}
			if (sizeDB == SizeDB.SizeProfil)
			{
				return ImageStore.Profiles + userid + ".png";
			}			
			if (sizeDB == SizeDB.SizeMiniMap)
			{
				return ImageStore.MapLocations + id + ".png";
			}
			
			path += userid + "/";
			return path + id + ".png";
		}				
		
		public static Uri GetPicUrlFromId (long id, long userid, SizeDB sizeDB)
		{			
			Uri res = null;
			try
			{
				if (sizeDB == SizeDB.Size100)
				{
					return (res = new Uri(string.Format("{0}/Zoom_100/{1}/{2}.jpg", streamingUrl, userid, id)));
				}
				if (sizeDB == SizeDB.Size75)
				{
					return (res = new Uri(string.Format("{0}/Zoom_75/{1}/{2}.jpg", streamingUrl, userid, id)));
				}
				if (sizeDB == SizeDB.Size50)
				{
					return (res = new Uri(string.Format("{0}/Zoom_50/{1}/{2}.jpg", streamingUrl, userid, id)));
				}
				if (sizeDB == SizeDB.SizeMiniMap)
				{
					return (res = new Uri(string.Format("{0}/MapLocations/{1}.jpg", streamingUrl, id)));
				}
				if (sizeDB == SizeDB.SizeProfil)
				{
					return (res = new Uri(string.Format("{0}/Profiles/{1}.jpg", streamingUrl, userid)));
				}
				if (sizeDB == SizeDB.SizeFacebook)
				{
					string accessToken = NSUserDefaults.StandardUserDefaults.StringForKey("FacebookAccessToken");
					return (res = new Uri(string.Format("https://graph.facebook.com/{0}/picture?access_token={1}", userid, accessToken)));
				}				
				return (res = new Uri(string.Format("{0}/Zoom_308_307/{1}/{2}.jpg", streamingUrl, userid, id)));
			}
			finally
			{
				//idToUrl[new Tuple<long, SizeDB>(id, sizeDB)] = res.OriginalString;
			}
		}
	}
}

