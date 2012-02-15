using System.Collections.Generic;

namespace MSP.Client.DataContracts
{
	public class SendFollowOp
	{
   	 	public Follow Follow { get; set; }
		public bool IsFollow {get;set;}
	}
	
    public class FollowResponse
    {
        public FollowResponse()
        {
            this.ResponseStatus = new ResponseStatus();
        }

        public ResponseStatus ResponseStatus { get; set; }

        public Follow Follow { get; set; }
    }	
	
    public class Followers
    {
        public int Id { get; set; }
        public Follow Follow { get; set; }
		public int? UserFollowedId {get;set;}
		public int? UserFollowerId {get;set;}
		
		public int? FollowersOfId {get;set;}
		public int? FollowedById {get;set;}
		public int? FriendsOfId {get;set;}
	}
	
    public class FollowersResponse
    {
        public FollowersResponse()
        {
            ResponseStatus = new ResponseStatus();
            Followers = new List<Follow>();
            Users = new List<User>();
        }

        public ResponseStatus ResponseStatus { get; set; }

        public List<Follow> Followers { get; set; }
        public List<User> Users { get; set; }
    }
}
