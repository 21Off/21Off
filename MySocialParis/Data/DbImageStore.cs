using System;
using TweetStation;
using MonoTouch.UIKit;
using System.IO;
using System.Drawing;
using MonoTouch.Foundation;

namespace MSP.Client
{
	public class DbImageStore
	{		
		public static bool DeleteDBPic (long id, long userid, SizeDB sizeDB)
		{
			try
			{
				LRUCache<long,UIImage> dbCache = ImageStore.GetCache(sizeDB);
				string picfile = UrlStore.GetDBImagePath(id, userid, sizeDB);
				
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
				Util.LogException("DeleteDBPic", ex);
				return false;
			}			
		}
		
		public static UIImage SaveDBPic (string picfile, long id, long userid, SizeDB sizeDB)
		{
			LRUCache<long,UIImage> dbCache = ImageStore.GetCache(sizeDB);
			
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
}

