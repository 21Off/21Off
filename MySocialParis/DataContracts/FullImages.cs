using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SQLite;

namespace MSP.Client.DataContracts
{
	public class FullImages
	{
        public int? WhereUserId { get; set; }
        public int? ImageId { get; set; }
        public long? Since { get; set; }	
	}
	
	public class ImageResponse
	{
		public Image Image {get;set;}
		public List<Keyword> Keywords {get;set;}
		public List<CommentResponse> Comments {get;set;}
		
		public int LikesCount {get;set;}
	}
	
	public class FullImagesResponse	
	{
		public FullImagesResponse()
		{
            this.ResponseStatus = new ResponseStatus();
            this.Images = new List<ImageResponse>();
        }

        public ResponseStatus ResponseStatus { get; set; }

        public List<ImageResponse> Images { get; set; }	
		
		public DateTime Time { get;set; }
	}
	
	public class FullImagesTimedResponse
	{
		public IEnumerable<ImageResponse> Images {get;set;}
		public DateTime? Time {get;set;}
	}
}
