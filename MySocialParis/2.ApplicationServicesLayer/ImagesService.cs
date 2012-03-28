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
	public class ImagesService : AppServiceBase, IBusinessImagesService
	{
		#region IBusinessImagesService implementation
		
		public Image StoreNewImage(Image image, string filePath, string mapPath, 
		                           List<Keyword> keywords, List<Comment> comments, int idRelation = 0)
		{
			var sni = new StoreNewImage()
			{			
				Image = image,
				ImageFile = GetIOFile(filePath),
				MapFile = mapPath == null ? null : GetIOFile(mapPath),
				Keywords = keywords,
				Comments = comments,
				IdRelation = idRelation,
			};
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/StoreNewImage");
			
			var we = new ManualResetEvent(false);
			JsonUtility.Upload (uri, sni, false, s =>
			{
				try
				{
					var json = JsonArray.Load (s);
					int Id = json.ContainsKey("ImageId") ? Convert.ToInt32(json["ImageId"].ToString()) : 0;
					image.Id = Id;
				}
				catch (Exception ex)
				{
					Util.LogException("StoreNewImage", ex);
				}
				we.Set();
			});
			
			we.WaitOne(10000);
			
			return image;
		}
		
		public Image CreateAlbumWithImage(Image image, string filePath, string mapPath, 
		                           List<Keyword> keywords, List<Comment> comments, string title, int idRelation = 0)
		{	
			var sni = new StoreNewImage()
			{			
				Image = image,
				ImageFile = GetIOFile(filePath),
				MapFile = mapPath == null ? null : GetIOFile(mapPath),
				Keywords = keywords,
				Comments = comments,
				IdRelation = idRelation,
			};
			
			var createAlbumWithImage = new CreateAlbumWithImage()
			{
				Image = sni,
				Title = title ?? image.Name,
			};			
			
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/CreateAlbumWithImage");
			
			var we = new ManualResetEvent(false);
			JsonUtility.Upload (uri, createAlbumWithImage, false, s =>
			{
				try
				{
					var json = JsonArray.Load (s);
					int Id = json.ContainsKey("ImageId") ? Convert.ToInt32(json["ImageId"].ToString()) : 0;
					int IdAlbum = json.ContainsKey("AlbumId") ? Convert.ToInt32(json["AlbumId"].ToString()) : 0;
					image.Id = Id;
					image.IdAlbum = IdAlbum;
				}
				catch (Exception ex)
				{
					Util.LogException("CreateAlbumWithImage", ex);
				}
				we.Set();
			});
			
			we.WaitOne(10000);
			
			return image;
		}		
		
		public Image AddNewImageToAlbum(Image image, string filePath, string mapPath, 
		                           List<Keyword> keywords, List<Comment> comments, int idAlbum, int idRelation = 0)
		{
			var sni = new StoreNewImage()
			{			
				Image = image,
				ImageFile = GetIOFile(filePath),
				MapFile = mapPath == null ? null : GetIOFile(mapPath),
				Keywords = keywords,
				Comments = comments,
				IdRelation = idRelation,
			};
			
			var addNewImageToAlbum = new AddNewImageToAlbum()
			{
				Image = sni,
				IdAlbum = idAlbum,
			};
			
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/AddNewImageToAlbum");
			
			var we = new ManualResetEvent(false);
			JsonUtility.Upload (uri, addNewImageToAlbum, false, s =>
			{
				try
				{
					var json = JsonArray.Load (s);
					int Id = json.ContainsKey("ImageId") ? Convert.ToInt32(json["ImageId"].ToString()) : 0;
					image.Id = Id;
				}
				catch (Exception ex)
				{
					Util.LogException("AddNewImageToAlbum", ex);
				}
				we.Set();
			});
			
			we.WaitOne(10000);
			
			return image;
		}		
		
		public static IOFile GetIOFile(string filePath)
		{
			var fileToUpload = new FileInfo(filePath);						
			using (FileStream fs = fileToUpload.OpenRead())
			{
				var fbytes = new byte[fs.Length];
				fs.Read(fbytes, 0, (int)fs.Length);

				return new IOFile()
				{
					ContentLength = fs.Length,
					InputBytes = fbytes,
					FileName = fileToUpload.Name,
					ContentType = MimeTypes.GetMimeType(fileToUpload.Name),
				};				
			}			
		}
		
		public void DeleteImage(Image image)
		{
			var we = new ManualResetEvent(false);			
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/images?Id={0}", image.Id);			
			
			JsonUtility.Launch(uri, false, s =>
			{
				try
				{
					var json = JsonArray.Load (s);
				}
				catch (Exception ex)
				{
					Util.LogException("DeleteImage", ex);
				}
				we.Set();
			}, "DELETE");
			
			we.WaitOne(5000);
		}		
		
		public ImageResponse GetFullImage(int id)
		{
			var images = new List<ImageResponse>();				
			
			var we = new ManualResetEvent(false);
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/FullImages?ImageId={0}", id);
			
			JsonUtility.Launch(uri, false, s =>
	        {
				try
				{
					var json = JsonArray.Load (s);
					foreach (JsonObject obj in json["Images"])
					{		
						try
						{
							JsonObject imgJson = (JsonObject)obj["Image"];							
							int likesCount = Convert.ToInt32(obj["LikesCount"].ToString());							
							
							var keywords = new List<Keyword>();
							foreach (JsonObject jsonObj in obj["Keywords"])
							{
								keywords.Add(KeywordsService.JsonToKeyword(jsonObj));
							}
							
							var comments = new List<CommentResponse>();
							foreach (JsonObject jsonObj in obj["Comments"])
							{
								JsonObject comm = (JsonObject)jsonObj["Comment"];
								JsonObject user = (JsonObject)jsonObj["User"];
								
								var cresp = new CommentResponse()
								{
									Comment = CommentsService.JsonToComment(comm),
									User = UsersService.JsonToUser(user),
								};
								comments.Add(cresp);
							}
							
							var imgResp = new ImageResponse()
							{
								Comments = comments,
								Image = JsonToImage(imgJson),
								LikesCount = likesCount,
								Keywords = keywords,
							};
							images.Add(imgResp);
						}
						catch (Exception ex)
						{
							Util.LogException("GetFullImagesOfUser", ex);
						}
					}
				}
				catch (Exception ex)
				{
					Util.LogException("GetaImagesOfUser", ex);
				}
				we.Set();
			});
			
			we.WaitOne();
			
			return images.FirstOrDefault();
		}
		
		public Image GetImage(int id)
		{
			Image image = null;
			var we = new ManualResetEvent(false);
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/Images?Id={0}", id);
			
			JsonUtility.Launch(uri, false, s =>
	        {
				try
				{
					var json = (JsonObject)JsonArray.Load (s);
					var imagesJson = json["Images"];
					if (imagesJson.Count > 0)
					{
						var obj = (JsonObject)imagesJson[0];
						image = JsonToImage(obj);
					}
				}
				catch (Exception ex)
				{
					Util.LogException("GetImage", ex);
				}
				we.Set();
			});
			
			we.WaitOne();
			
			return image;				
		}
		
		public static Image JsonToImage(JsonObject obj)
		{
			var image = new Image()
			{
				Id = Convert.ToInt32(obj["Id"].ToString()),
				Name = obj.ContainsKey("Name") ? obj["Name"].ToString().Replace("\"","") : null,
				Latitude = Convert.ToDouble(obj["Latitude"].ToString(), System.Globalization.CultureInfo.InvariantCulture),
				Longitude = Convert.ToDouble(obj["Longitude"].ToString(), System.Globalization.CultureInfo.InvariantCulture), 
				UserId = Convert.ToInt32(obj["UserId"].ToString()),
				IdAlbum = obj.ContainsKey("IdAlbum") ? Convert.ToInt32(obj["IdAlbum"].ToString()) : 0,				
				//Time =  DateTime.ParseExact (obj ["Time"], "ddd MMM dd HH:mm:ss zzz yyyy", CultureInfo.InvariantCulture),
			};
			
			return image;
		}	
		
		public static Albums JsonToAlbum(JsonObject obj)
		{
			return new Albums()
			{
				Id = Convert.ToInt32(obj["Id"].ToString()),
				Title = obj.ContainsKey("Title") ? obj["Title"].ToString().Replace("\"","") : null,
			};			
		}	
		
		public IEnumerable<Image> GetImagesOfUser(int userId, DateTime since)
		{
			var images = new List<Image>();			
			var we = new ManualResetEvent(false);
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/Images?WhereMyId={0}", userId);
			
			JsonUtility.Launch(uri, false, s =>
	        {
				try
				{
					var json = JsonArray.Load (s);
					foreach (JsonObject obj in json["Images"])
					{		
						try
						{
							images.Add(JsonToImage(obj));
						}
						catch (Exception ex)
						{
							Util.LogException("GetImagesOfUser", ex);
						}
					}
				}
				catch (Exception ex)
				{
					Util.LogException("GetaImagesOfUser", ex);
				}
				we.Set();
			});
			
			we.WaitOne();
			
			return images;
		}	
		
		public AlbumsResponse GetAlbum(int userId, int albumId)
		{
			try
			{
				var req = new GetAlbum() { Id = albumId };				
				var uri = new Uri("http://storage.21offserver.com/json/syncreply/GetAlbum");
				
				var slim = new ManualResetEventSlim(false);
				AlbumsResponse fullImgResp = null;
				
				ThreadPool.QueueUserWorkItem(o=> 
                {			
					try
					{
						var request = (HttpWebRequest) WebRequest.Create (uri);
						request.Method = "PUT";
						using (var reqStream = request.GetRequestStream())
						{				
							ServiceStack.Text.JsonSerializer.SerializeToStream(req, typeof(GetAlbum), reqStream);
						};
						using (var response = request.GetResponse())
						{
							using (var stream = response.GetResponseStream())
							{
								var json = JsonObject.Load(stream);
								fullImgResp = new AlbumsResponse()
								{
									Album = JsonToAlbum((JsonObject)json["Album"]),
								};
							}
						}
					}
					catch (Exception ex)
					{
						Util.LogException("GetAlbum", ex);
					}
					slim.Set();
				});
				
				slim.Wait(6000);
				
				return fullImgResp;
			}
			catch (Exception ex)
			{
				Util.LogException("GetAlbum", ex);
				return null;
			}
		}		
		
		public ImagesResponse GetAlbumImages(int startIndex, int maximumImages, int userId, int albumId)
		{
			try
			{
				var req = new AlbumImages() { WhereMyId = userId, Max = maximumImages, Start = startIndex, AlbumId = albumId };				
				var uri = new Uri("http://storage.21offserver.com/json/syncreply/AlbumImages");
				
				var slim = new ManualResetEventSlim(false);
				ImagesResponse fullImgResp = null;
				
				ThreadPool.QueueUserWorkItem(o=> 
                {			
					try
					{
						var request = (HttpWebRequest) WebRequest.Create (uri);
						request.Method = "PUT";
						using (var reqStream = request.GetRequestStream())
						{				
							ServiceStack.Text.JsonSerializer.SerializeToStream(req, typeof(AlbumImages), reqStream);
						};
						using (var response = request.GetResponse())
						{
							using (var stream = response.GetResponseStream())
							{
								var json = JsonObject.Load(stream);
								
								var images = new List<Image>();
								if (json.ContainsKey("Album"))
								{
									Albums albums = JsonToAlbum((JsonObject)json["Album"]);
								}
								
								foreach (JsonObject obj in json["Images"])
								{		
								   	images.Add(JsonToImage(obj));
								}
								fullImgResp = new ImagesResponse();
								fullImgResp.Images = images;
							}
						}
					}
					catch (Exception ex)
					{
						Util.LogException("GetAlbumImages", ex);
					}
					slim.Set();
				});
				
				slim.Wait(6000);
				
				return fullImgResp;
			}
			catch (Exception ex)
			{
				Util.LogException("GetAlbumImages", ex);
				return null;
			}
		}
		
		
		
		public AllImagesResponse GetFullImageList (FilterType filterType, GeoLoc geoLoc, int startIndex, 
			int maximumImages, int userId)			
		{
			DateTime dt = DateTime.Now;
			try
			{
				var req = new Images() { WhereGeo = geoLoc, Filter = (int)filterType, WhereMyId = userId, 
										Max = maximumImages, Start = startIndex };
				
				var slim = new ManualResetEventSlim(false);
				AllImagesResponse fullImgResp = null;
				
				ThreadPool.QueueUserWorkItem(o=> 
                {
					try
					{
						fullImgResp = GetFullImageListDel(req);
					}
					catch (Exception ex)
					{
						Util.LogException("GetFullImageListDel", ex);
					}
					slim.Set();
				});
				slim.Wait(6000);
				
				return fullImgResp;
			}
			catch (Exception ex)
			{
				Util.LogException("GetFullImageList", ex);
				return null;
			}
			finally
			{
				Util.Log("GetFullImageList: " +  (DateTime.Now - dt));				
			}
		}		
		
		public IEnumerable<Image> GetImageList (FilterType filterType, GeoLoc geoLoc, int startIndex, int maximumImages, int userId)
		{
			try
			{
				var req = new Images() { WhereGeo = geoLoc, Filter = (int)filterType, WhereMyId = userId, 
									Max = maximumImages, Start = startIndex };
				
				var uri = new Uri("http://storage.21offserver.com/json/syncreply/Images");
				
				var request = (HttpWebRequest) WebRequest.Create (uri);
				request.Method = "PUT";
				using (var reqStream = request.GetRequestStream())
				{				
					ServiceStack.Text.JsonSerializer.SerializeToStream(req, typeof(Images), reqStream);
				};
				using (var response = request.GetResponse())
				{
					using (var stream = response.GetResponseStream())
					{
						var json = JsonObject.Load(stream);
						
						var images = new List<Image>();
						
						foreach (JsonObject obj in json["Images"])
						{		
						   	images.Add(JsonToImage(obj));
						}
						return images;
					}
				}
			}
			catch (Exception ex)
			{
				Util.LogException("GetImageList", ex);
				return null;
			}
		}

		public FullImagesTimedResponse GetFullImagesOfUser(int userId, DateTime since)
		{
			FullImagesTimedResponse fullImagesTimed = null;
			List<ImageResponse> images = null;
			
			var we = new ManualResetEvent(false);
			
			string uri = string.Format("http://storage.21offserver.com/json/syncreply/FullImages?WhereUserId={0}&Since={1}&Count={2}", 
					userId, since.Ticks, 5);
			
			JsonUtility.Launch(uri, false, s =>
	        {
				try
				{
					fullImagesTimed = new FullImagesTimedResponse();
					images = new List<ImageResponse>();
					fullImagesTimed.Images = images;
					
					var json = JsonArray.Load (s);
					if (json.ContainsKey("Time"))
					{
						DateTime? time = ActivitiesService.JsonToTime((JsonValue)json["Time"]);
						fullImagesTimed.Time = time;
					}
					foreach (JsonObject obj in json["Images"])
					{		
						try
						{
							JsonObject imgJson = (JsonObject)obj["Image"];							
							int likesCount = Convert.ToInt32(obj["LikesCount"].ToString());							
							
							var keywords = new List<Keyword>();
							foreach (JsonObject jsonObj in obj["Keywords"])
							{
								keywords.Add(KeywordsService.JsonToKeyword(jsonObj));
							}
							
							var comments = new List<CommentResponse>();
							foreach (JsonObject jsonObj in obj["Comments"])
							{
								JsonObject comm = (JsonObject)jsonObj["Comment"];
								JsonObject user = (JsonObject)jsonObj["User"];
								
								var cresp = new CommentResponse()
								{
									Comment = CommentsService.JsonToComment(comm),
									User = UsersService.JsonToUser(user),
								};
								comments.Add(cresp);
							}
							
							var imgResp = new ImageResponse()
							{
								Comments = comments,
								Image = JsonToImage(imgJson),
								LikesCount = likesCount,
								Keywords = keywords,
							};
							images.Add(imgResp);
						}
						catch (Exception ex)
						{
							Util.LogException("GetFullImagesOfUser", ex);
						}
					}
				}
				catch (Exception ex)
				{
					Util.LogException("GetaImagesOfUser", ex);
				}
				we.Set();
			});
			
			we.WaitOne(5000);			
			
			return fullImagesTimed;			
		}

		void HandleReachabilityReachabilityChanged (object sender, EventArgs e)
		{
			AppDelegateIPhone.ShowRealLoading(null, "Alert", null, null);
		}		
		
		#endregion
		
		
		#region Private methods
		
		private AllImagesResponse GetFullImageListDel(Images req)
		{
			var uri = new Uri("http://storage.21offserver.com/json/syncreply/Images");
			
			var request = (HttpWebRequest) WebRequest.Create (uri);
			request.Method = "PUT";
			using (var reqStream = request.GetRequestStream())
			{				
				ServiceStack.Text.JsonSerializer.SerializeToStream(req, typeof(Images), reqStream);
			};
			
			using (var response = request.GetResponse())
			{
				using (var stream = response.GetResponseStream())
				{
					var json = JsonObject.Load(stream);
					
					var fullImgResp = new AllImagesResponse();
					
					foreach (JsonObject obj in json["FriendsImages"])
					{		
					   	fullImgResp.FriendsImages.Add(JsonToImage(obj));					
					}
					foreach (JsonObject obj in json["RecentImages"])
					{		
					   	fullImgResp.RecentImages.Add(JsonToImage(obj));					
					}
					foreach (JsonObject obj in json["LikedImages"])
					{		
					   	fullImgResp.LikedImages.Add(JsonToImage(obj));					
					}
					foreach (JsonObject obj in json["EventsImages"])
					{		
					   	fullImgResp.EventsImages.Add(JsonToImage(obj));					
					}					
					
					return fullImgResp;
				}
			}			
		}		
		
		#endregion
	}
}
