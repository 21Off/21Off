using System.Collections.Generic;

namespace MSP.Client.DataContracts
{	
    public class LikeResponse
    {
        public LikeResponse()
        {
            this.ResponseStatus = new ResponseStatus();
        }
		
		public int Count {get;set;}
        public ResponseStatus ResponseStatus { get; set; }
	}
	
	public class SendLikeOp
	{
   	 	public Like Like { get; set; }
		public bool IsLike {get;set;}
	}
	
    public class Likes
    {
        public int? ImageId { get; set; }
		public int? FullImageId {get;set;}
	}
	
	public class FullLike
	{
		public Like Like {get;set;}
		public User User {get;set;}
	}
	
    public class FullLikesResponse
    {
        public FullLikesResponse()
        {
            this.ResponseStatus = new ResponseStatus();
            this.Likes = new List<FullLike>();
        }

        public ResponseStatus ResponseStatus { get; set; }

        public List<FullLike> Likes { get; set; }
	}	
	
    public class LikesResponse
    {
        public LikesResponse()
        {
            this.ResponseStatus = new ResponseStatus();
            this.Likes = new List<Like>();
        }

        public ResponseStatus ResponseStatus { get; set; }

        public List<Like> Likes { get; set; }
	}
}

