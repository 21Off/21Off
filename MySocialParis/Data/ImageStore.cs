// Copyright 2010 Miguel de Icaza
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
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
	//
	// Provides an interface to download pictures in the background
	// and keep a local cache of the original files + rounded versions
	//
	// The IDs used here have the following meaning:
	//   Positive numbers are the small profile pictures and correspond to a twitter ID
	//   Negative numbers are medium size pictures for the same twitter ID
	//   Numbers above TmpStartId are transient pictures, used because Twitter
	//   search returns a *different* set of userIds on search results
	// 

	public static class ImageStore
	{
		public const long TempStartId = 100000000000000;
		const int MaxRequests = 6;
		public static string PicDir, TmpDir, MapLocations;
		public static string FileDB, FileDB50, FileDB100, FileDB75, Profiles;
		
		public readonly static UIImage DefaultImage;
		public readonly static UIImage EmptyProfileImage;
		
		static LRUCache<long,UIImage> cache;		
		
		static LRUCache<long,UIImage> DBcachefull;
		static LRUCache<long,UIImage> DBcache50;
		static LRUCache<long,UIImage> DBcache75;
		static LRUCache<long,UIImage> DBcache100;
		static LRUCache<long,UIImage> DBcacheMiniMap;
		static LRUCache<long,UIImage> DBcacheProfiles;
				
		// A list of requests that have been issues, with a list of objects to notify.
		static Dictionary<Tuple<long, long, SizeDB>, List<IImageUpdated>> pendingRequests;
		
#if DEBUGIMAGE
		static Dictionary<long,long> pendingTimes;
#endif
		
		// A list of updates that have completed, we must notify the main thread about them.
		static HashSet<Tuple<long, long, SizeDB>> queuedUpdates;
		
		// A queue used to avoid flooding the network stack with HTTP requests
		static Stack<ImgRequest> requestQueue;
		
		public class ImgRequest
		{
			public long ID {get;set;}
			public long UserId {get;set;}
			public SizeDB SizeDb {get;set;}
		}
		
		// Keeps id -> url mappings around
		static Dictionary<Tuple<long, SizeDB>, string> idToUrl;

		static NSString nsDispatcher = new NSString ("x");
		
		static ImageStore ()
		{
			PicDir = Path.Combine (Util.BaseDir, "Library/Caches/Pictures/");
			TmpDir = Path.Combine (Util.BaseDir, "tmp/");
			FileDB = Path.Combine(PicDir, "fileDB/");
			FileDB50 = Path.Combine(PicDir, "fileDB50/");
			FileDB75 = Path.Combine(PicDir, "fileDB75/");
			FileDB100 = Path.Combine(PicDir, "fileDB100/");
			Profiles = Path.Combine(PicDir, "Profiles/");
			MapLocations = Path.Combine(PicDir, "MapLocations/");
			
			if (!Directory.Exists (PicDir))
				Directory.CreateDirectory (PicDir);
			
			if (!Directory.Exists (FileDB))
				Directory.CreateDirectory (FileDB);
			
			if (!Directory.Exists (FileDB50))
				Directory.CreateDirectory (FileDB50);
			
			if (!Directory.Exists (FileDB75))
				Directory.CreateDirectory (FileDB75);
			
			if (!Directory.Exists (FileDB100))
				Directory.CreateDirectory (FileDB100);
			
			if (!Directory.Exists (Profiles))
				Directory.CreateDirectory (Profiles);
			
			if (!Directory.Exists (MapLocations))
				Directory.CreateDirectory (MapLocations);			

			DefaultImage = UIImage.FromFile ("Images/carre.png");
			EmptyProfileImage = UIImage.FromFile("Images/Login/emptyProfile.png");
			cache = new LRUCache<long,UIImage> (200);
			
			DBcachefull = new LRUCache<long, MonoTouch.UIKit.UIImage>(200);
			DBcache50 = new LRUCache<long, MonoTouch.UIKit.UIImage>(200);
			DBcache75 = new LRUCache<long, MonoTouch.UIKit.UIImage>(200);
			DBcache100 = new LRUCache<long, MonoTouch.UIKit.UIImage>(200);
			DBcacheMiniMap = new LRUCache<long, MonoTouch.UIKit.UIImage>(200);
			DBcacheProfiles = new LRUCache<long, MonoTouch.UIKit.UIImage>(200);
			
			pendingRequests = new Dictionary<Tuple<long, long, SizeDB>, List<IImageUpdated>>();
#if DEBUGIMAGE
			pendingTimes = new Dictionary<long, long> ();
#endif
			idToUrl = new Dictionary<Tuple<long, SizeDB>, string>();
			queuedUpdates = new HashSet<Tuple<long, long, SizeDB>>();
			requestQueue = new Stack<ImgRequest> ();
		}
		
		public static void Purge ()
		{
			cache.Purge ();
			
			lock (DBcache100.SyncValue) DBcache100.Purge();
			lock (DBcache50.SyncValue) DBcache50.Purge();
			lock (DBcache75.SyncValue) DBcache75.Purge();
			lock (DBcachefull.SyncValue) DBcachefull.Purge();
			lock (DBcacheMiniMap.SyncValue) DBcacheMiniMap.Purge();
			lock (DBcacheProfiles.SyncValue) DBcacheProfiles.Purge();
		}
		
		public static LRUCache<long,UIImage> GetCache(SizeDB sizeDB)
		{
			LRUCache<long,UIImage> dbCache = DBcachefull;
			if (sizeDB == SizeDB.Size100)
			{
				dbCache = DBcache100;
			}
			if (sizeDB == SizeDB.Size75)
			{
				dbCache = DBcache75;
			}
			if (sizeDB == SizeDB.Size50)
			{
				dbCache = DBcache50;
			}
			if (sizeDB == SizeDB.SizeMiniMap)
			{
				dbCache = DBcacheMiniMap;
			}
			if (sizeDB == SizeDB.SizeProfil)
			{
				dbCache = DBcacheProfiles;
			}
			
			return dbCache;
		}
		
		public static UIImage GetLocalFullPicture (long id, long userid, SizeDB sizeDB)
		{
			try
			{
				LRUCache<long,UIImage> dbCache = GetCache(sizeDB);
				
				lock (dbCache.SyncValue)
				{
					var ret = dbCache[id];
					if (ret == null)
					{
						var path = GetDBImagePath(new Tuple<long, long, SizeDB>(id, userid, sizeDB));
						if (File.Exists(path))
						{
							UIImage image = UIImage.FromFile(path);
							dbCache[id] = image;
							return image;
						}
						else
							return null;
					}
					else
						return ret;
				}
			}
			catch (Exception ex)
			{
				Util.LogException("GetLocalFullPicture", ex);
				return null;
			}		
		}
		
		public static UIImage RequestFullPicture (long id, long userid, SizeDB sizeDB, IImageUpdated notify)
		{
			var pic = GetLocalFullPicture (id, userid, sizeDB);
			if (pic == null){
				QueueRequestForPicture (id, userid, sizeDB, notify); 
				
				// return default picture or null while waiting for the high-res version to come				
				return null;
			}
			
			return pic;
		}		
		
		static Uri GetPicUrlFromId (long id, long userid, SizeDB sizeDB)
		{
			if (sizeDB == SizeDB.Size100)
			{
				return new Uri(string.Format("http://storage.21offserver.com/files/Zoom_100/{0}/{1}.jpg", userid, id));
			}
			if (sizeDB == SizeDB.Size75)
			{
				return new Uri(string.Format("http://storage.21offserver.com/files/Zoom_75/{0}/{1}.jpg", userid, id));
			}
			if (sizeDB == SizeDB.Size50)
			{
				return new Uri(string.Format("http://storage.21offserver.com/files/Zoom_50/{0}/{1}.jpg", userid, id));
			}
			if (sizeDB == SizeDB.SizeMiniMap)
			{
				return new Uri(string.Format("http://storage.21offserver.com/files/MapLocations/{0}.jpg", id));
			}
			if (sizeDB == SizeDB.SizeProfil)
			{
				return new Uri(string.Format("http://storage.21offserver.com/files/Profiles/{0}.jpg", userid));
			}
			return new Uri(string.Format("http://storage.21offserver.com/files/Zoom_308_307/{0}/{1}.jpg", userid, id));
		}
		
		//
		// Requests that the picture for "id" be downloaded, the optional url prevents
		// one lookup, it can be null if not known
		//
		public static void QueueRequestForPicture (long id, long userid, SizeDB sizeDB, IImageUpdated notify)
		{									
			if (notify == null)
			{
				var argNullEx = new ArgumentNullException ("notify");
				Util.LogException("notifier is null!", argNullEx);				
				throw argNullEx;
			}
			
			Uri url;
			lock (requestQueue)
				url = GetPicUrlFromId (id, userid, sizeDB);
			
			if (url == null)
				return;		
			
			var pendReq = new Tuple<long, long, SizeDB>(id, userid, sizeDB);
			lock (requestQueue){
				if (pendingRequests.ContainsKey (pendReq)){
					Util.Log ("pendingRequest: added new listener for {0}", id);
					pendingRequests [pendReq].Add (notify);
					return;
				}
				var slot = new List<IImageUpdated> ();
				slot.Add (notify);
				pendingRequests [pendReq] = slot;
#if DEBUGIMAGE
				pendingTimes [id] = DateTime.UtcNow.Ticks;
#endif	
				if (picDownloaders > MaxRequests){
					Util.Log ("Queuing Image request because {0} >= {1} {2}", requestQueue.Count, MaxRequests, picDownloaders);
					var imgRequest = new ImgRequest() { ID = id, UserId = userid, SizeDb = sizeDB };
					requestQueue.Push (imgRequest);
				} else {
					ThreadPool.QueueUserWorkItem (delegate { 
						
							try {
								StartPicDownload (pendReq, url); 
							} catch (Exception e){
								Util.LogException("QueueRequestForPicture", e);
							}
						});
				}
			}
		}

		static void EnsureTmpIsClean ()
		{
			if (TmpCleaned)
				return;
			
			foreach (string f in Directory.GetFiles (TmpDir, "*.jpg"))
				File.Delete (f);
			TmpCleaned = true;
		}
		
		static bool TmpCleaned;
		
		static bool Download (Uri url, string target)
		{
			try
			{           
				var dirInfo = new FileInfo(target).Directory;
				if (!dirInfo.Exists)
					dirInfo.Create();
				
				string tempPath = Path.Combine(TmpDir, Guid.NewGuid().ToString().Replace("-", "") + ".jpg");
								
				if (!Downloader.DownloadImage(url, tempPath))
					return false;
			
				File.Copy(tempPath, target, true);
				File.Delete(tempPath);
			} 
			catch (Exception e) 
			{
				Util.LogException(string.Format("Problem with {0}", url), e);
				return false;
			}
			return true;
		}					
		
		static int picDownloaders;
		
		static void StartPicDownload (Tuple<long, long, SizeDB> pendReq, Uri url)
		{
			Interlocked.Increment (ref picDownloaders);
			try {
				_StartPicDownload (pendReq, url);
			} catch (Exception e){
				Util.Log ("CRITICAL: should have never happened {0}", e);
			}
			Util.Log ("Leaving StartPicDownload {0}", picDownloaders);
			Interlocked.Decrement (ref picDownloaders);
		}
		
		static void _StartPicDownload (Tuple<long, long, SizeDB> pendReq, Uri url)
		{
			long id = -1;
			do {
				string picdir = GetDBImagePath(pendReq);
				bool downloaded = false;
				id = pendReq.Item1;				
				
				downloaded = Download (url, picdir);
				if (!downloaded)
					Util.Log ("Error fetching picture for {0} from {1}", id, url);
				else
					Util.Log("Finished downloading picture for {0}", id);
				
				// Cluster all updates together
				bool doInvoke = false;
				
				lock (requestQueue){
					if (downloaded){
						queuedUpdates.Add (pendReq);
					
						// If this is the first queued update, must notify
						if (queuedUpdates.Count == 1)
							doInvoke = true;
					} else {
						pendingRequests.Remove (pendReq);
#if DEBUGIMAGE
						pendingTimes.Remove (id);
#endif
					}
					idToUrl.Remove (new Tuple<long, SizeDB>(id, pendReq.Item3));

					// Try to get more jobs.
					if (requestQueue.Count > 0){
						ImgRequest imgRequest = requestQueue.Pop ();
						id = imgRequest.ID;						
						url = GetPicUrlFromId (id, imgRequest.UserId, imgRequest.SizeDb);
						if (url == null){
							Util.Log ("Dropping request {0} because url is null", id);
							var dropReq = new Tuple<long, long, SizeDB>(id, imgRequest.UserId, imgRequest.SizeDb);
							pendingRequests.Remove (dropReq);
#if DEBUGIMAGE
							pendingTimes.Remove (id);
#endif
							id = -1;
						}
					} else {
						Util.Log ("Leaving because requestQueue.Count = {0} NOTE: {1}", requestQueue.Count, pendingRequests.Count);
						id = -1;
					}
				}	
				if (doInvoke)
					nsDispatcher.BeginInvokeOnMainThread (NotifyImageListeners);
				
			} while (id != -1);
		}
		
		// Runs on the main thread
		static void NotifyImageListeners ()
		{
			try
			{
				lock (requestQueue){
					foreach (var qid in queuedUpdates){
						var list = new List<IImageUpdated>();
						if (pendingRequests.ContainsKey(qid))
						{
							list = pendingRequests [qid];
							pendingRequests.Remove (qid);
						}
	#if DEBUGIMAGE
						pendingTimes.Remove (qid);
	#endif
						foreach (var pr in list){
							try {
								pr.UpdatedImage (qid.Item1, qid.Item2, qid.Item3);
							} catch (Exception e){
								Console.WriteLine (e);
							}
						}
					}
					queuedUpdates.Clear ();
				}
			}
			catch (Exception ex)
			{
				Util.LogException("NotifyImageListeners", ex);
			}
		}
		
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
		
		public static bool DeleteDBPic (long id, long userid, SizeDB sizeDB)
		{
			try
			{
				LRUCache<long,UIImage> dbCache = GetCache(sizeDB);
				string picfile = GetDBImagePath(id, userid, sizeDB);
				
				lock (dbCache.SyncValue)
				{
					dbCache.Remove(id);
					
					if (File.Exists (picfile))
					{	
						File.Delete(picfile);
					}
				}
				return true;
			}
			catch (Exception ex)
			{
				Console.WriteLine("DeleteDBPic: " + ex.Message);
				return false;
			}			
		}
		
		public static UIImage SaveDBPic (string picfile, long id, long userid, SizeDB sizeDB)
		{
			LRUCache<long,UIImage> dbCache = GetCache(sizeDB);
			
			lock (dbCache.SyncValue){				
				using (var pic = UIImage.FromFileUncached (picfile)){
					if (pic == null)
						return null;
					
					SizeF size = pic.Size;
					string path = ImageStore.FileDB;
					
					if (sizeDB == SizeDB.Size100)
					{
						path = ImageStore.FileDB100;
						size = new SizeF(100, 100);
					}
					if (sizeDB == SizeDB.Size75)
					{
						path = ImageStore.FileDB75;
						size = new SizeF(75, 75);
					}
					if (sizeDB == SizeDB.Size50)
					{
						path = ImageStore.FileDB50;
						size = new SizeF(50, 50);
					}
					
					path = path + userid + "/";
					if (!Directory.Exists(path))
						Directory.CreateDirectory(path);
					
					UIImage cute = UIImageUtils.resizeImage(pic, size);

					var bytes = cute.AsPNG ();
					NSError err;
					bytes.Save (path + id + ".png", false, out err);
					
					// we might as well add it to the cache
					dbCache [id] = cute;
					
					return cute;
				}
			}
		}		
	}
		
	public enum SizeDB
	{
		SizeFull,
		Size50,
		Size75,
		Size100,
		SizeMiniMap,
		SizeProfil,
	}
}
