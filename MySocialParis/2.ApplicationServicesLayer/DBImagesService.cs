using System;
using System.Collections.Generic;
using System.Linq;
using MonoTouch.CoreLocation;
using MSP.Client.DataContracts;
using TweetStation;
using MonoTouch.Foundation;
using System.IO;
using System.Drawing;
using MonoTouch.UIKit;

namespace MSP.Client
{
	public class DBImagesService : AppServiceBase, IBusinessImagesService
	{		
		#region Private methods
				
		private int GetLikedCount(Image image)
		{
			return Database.Main.Table<Like>().Count(l => l.ParentId == image.Id);
		}
				
		private IEnumerable<Image> GetDBImageList (FilterType filterType, int userId)
		{
			lock (Database.Main)
			{
				if (filterType == FilterType.Recent)
				{
					return Database.Main.Table<Image>().OrderByDescending(el => el.Time);
				}
				if (filterType == FilterType.Friends)
				{
					List<Follow> friends = Database.Main.Table<Follow>().Where(el => el.FollowerId == userId).ToList();					
					var images = new List<Image>();
					//Add my friends photos
					foreach (Follow friend in friends)
					{
						images.AddRange(GetImagesOfUser(friend.UserId));
					}
					//Add my own photos
					images.AddRange(GetImagesOfUser(userId));
					
					return images.OrderByDescending(el => el.Time);
				}
				if (filterType == FilterType.Liked)
				{
					int minLikes = 1;
					var dateFrom = DateTime.UtcNow.Subtract(TimeSpan.FromDays(2));
					var recentImages = Database.Main.Table<Image>().Where(el => el.Time >= dateFrom).ToList();
					return recentImages.Where(el => GetLikedCount(el) >= minLikes).OrderByDescending(el => GetLikedCount(el));
				}
			}
			return new List<Image>();
		}
		
		#endregion	
		
		#region IBusinessImagesService implementation
		
		public void DeleteImage(Image image)
		{
			lock (Database.Main)
			{
				Database.Main.RunInTransaction(()=>
                {				
					ImageStore.DeleteDBPic(image.Id, image.UserId, SizeDB.SizeFull);
					ImageStore.DeleteDBPic(image.Id, image.UserId, SizeDB.Size50);
					ImageStore.DeleteDBPic(image.Id, image.UserId, SizeDB.Size75);
					ImageStore.DeleteDBPic(image.Id, image.UserId, SizeDB.Size100);							
					
					Database.Main.Execute("delete from like where ParentId = ?", image.Id);
					Database.Main.Execute("delete from comment where ParentId = ?", image.Id);
					Database.Main.Execute("delete from keyword where ParentId = ?", image.Id);							
					Database.Main.Delete<Image>(image);
				});
			}			
		}
		
		public IEnumerable<Image> GetImagesOfUser(int userId)
		{
			lock (Database.Main)
			{
				return Database.Main.Table<Image>().Where(el => el.UserId == userId);
			}
		}
		
		public Image StoreNewImage(Image image, string LocationMapPhotoCapture, string jpgstr, List<Keyword> Keywords)
		{
			int mainUserID = AppDelegateIPhone.AIphone.MainUser.Id;
			try
			{
				lock (Database.Main)
				{											
					Database.Main.BeginTransaction();
					if (Database.Main.Insert(image) == 1)
					{	
						ImageStore.SaveDBPic(jpgstr, image.Id, mainUserID, SizeDB.SizeFull);
						ImageStore.SaveDBPic(jpgstr, image.Id, mainUserID, SizeDB.Size50);
						ImageStore.SaveDBPic(jpgstr, image.Id, mainUserID, SizeDB.Size75);
						ImageStore.SaveDBPic(jpgstr, image.Id, mainUserID, SizeDB.Size100);
						
						var url = new Uri("http://www.univestu.com/MySocialParis/Profiles/" + string.Format("{0}.jpg", image.Id));			
						foreach(Keyword keyword in Keywords)
						{
							keyword.ParentId = image.Id;
							Database.Main.Insert(keyword);
						}
						Database.Main.Commit();
						
					}
					else
						throw new Exception("Image db insertion failed");
				}
			}
			catch (Exception ex)
			{
				Database.Main.Rollback();
				Util.LogException("SaveImage", ex);
			}
			return image;
		}
		
		public Image GetImage(int id)
		{
			lock (Database.Main)
			{
				return Database.Main.Get<Image>(id);
			}			
		}
		
		public IEnumerable<Image> GetImageList (FilterType filterType, GeoLoc geoLoc, int startIndex, int maximumImages, int userId)
		{
			CLLocationCoordinate2D location = geoLoc == null ? 
				default(CLLocationCoordinate2D) : 
				new CLLocationCoordinate2D(geoLoc.Latitude, geoLoc.Longitude);
			
			var res = GetDBImageList(filterType, userId);
			res = res.Where(img => 
			{
				if (geoLoc == null)
					return true;
				
				var coord = new CLLocationCoordinate2D(img.Latitude, img.Longitude);
				if (MapUtilities.DistanceBetween(location, coord) <= 0.2)				
					return true;
				else
					return false;				
			});
			return res.Take(maximumImages);
		}
		
		#endregion
	}
}
