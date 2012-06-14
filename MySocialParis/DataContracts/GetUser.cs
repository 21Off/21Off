using System.Collections.Generic;
using System.Runtime.Serialization;
using SQLite;

namespace MSP.Client.DataContracts
{
	public class LastUserLogged
	{
		[AutoIncrement]
		[PrimaryKey]
		[DataMember]
		public int Id { get; set; }
		
		public int UserId { get; set; }
	}
	
	[DataContract(Namespace = "ExampleConfig.DefaultNamespace")]
	public class User : IUser
	{
		//[AutoIncrement]
		[PrimaryKey]
		[DataMember]
		public int Id { get; set; }

		[DataMember]
		public string Name { get; set; }
		
		[DataMember]
		public int HasPhoto {get;set;}
		
		public string Password {get;set;}
						
		public bool FBUser { get;set; }	
		
		[Ignore()]
		public long FriendsCount {
			get;set;
		}		
		
		[Ignore()]
		public long FollowersCount {
			get;set;
		}
		
		[Ignore()]
		public long FollowedCount {
			get;set;
		}
		
		[Ignore()]
		public long PhotoCount {
			get;set;
		}		
	}
	
	public class GetUsers
	{
		public long? UserId {get;set;}
		public long? FullUserId {get;set;}
		public long? AskerId {get;set;}
	}
	
	public class FullUserResponse
	{
		public FullUserResponse()
		{
			this.ResponseStatus = new ResponseStatus();
		}
		
		public int FriendsCount {get;set;}
		public int FollowersCount {get;set;}	//admirers
		public int FollowedCount {get;set;}		//charmers
		public int ImagesCount {get;set;}
		public User User { get; set; }
		/// <summary>
		/// 0 - no relation, 1 - you follow user, 2 - user follows you, 3 - mutual following
		/// </summary>
		public int InRelation {get;set;}
		

		public ResponseStatus ResponseStatus { get; set; }
	}
	
	public enum Relation
	{
		None = 0,
		YouFollowUser = 1,
		UserFollowsYou = 2,
		MutualFollowing = 3,
	}

	public class GetUsersResponse
	{
		public GetUsersResponse()
		{
			this.ResponseStatus = new ResponseStatus();
		}

		public User User { get; set; }

		public ResponseStatus ResponseStatus { get; set; }
	}
	
	public class GetSubscribersBySocialIds
	{
		public string SocialIds {get;set;}
		public int SocialType {get;set;}
	}
	
	public class GetSubscribersBySocialIdsResponse
	{
		public GetSubscribersBySocialIdsResponse()
		{
			this.ResponseStatus = new ResponseStatus();
			Subscribers = new List<long>();
		}

		public List<long> Subscribers { get; set; }

		public ResponseStatus ResponseStatus { get; set; }
	}	
	

	
    public class UpdateNewUser
    {
        public long UserId { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }		
        public IOFile ImageFile { get; set; }
    }	
	
	
	public class AuthFacebookUser
	{
		public decimal UserId {get;set;}
		public string UserName { get; set; }
		public string SignedRequest { get; set; }
		public string Version { get; set; }
	}

	public class AuthUser
	{
		public string UserName { get; set; }
		public string Password { get; set; }
		public string Version { get; set; }
	}
	
	public class AuthUserResponse
	{
		public AuthUserResponse()
		{
			this.ResponseStatus = new ResponseStatus();
		}

		public int UserId { get; set; }

		public ResponseStatus ResponseStatus { get; set; }
	}	
}

