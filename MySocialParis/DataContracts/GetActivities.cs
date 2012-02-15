using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SQLite;

namespace MSP.Client.DataContracts
{
	[DataContract(Namespace = ExampleConfig.DefaultNamespace)]
	public enum ActivityType
	{
		PhotoLike,
		UserFollow,
		PhotoComment,
        PhotoShare,
		PhotoLiker,
	}	
	
    [DataContract(Namespace = ExampleConfig.DefaultNamespace)]
    public class Activity
    {
		[PrimaryKey]
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int UserId { set; get; }

        [DataMember]
        public int? IdOwner { set; get; }

        [DataMember]
        public DateTime Time { set; get; }

        [DataMember]
        public int Type { set; get; }

        [DataMember]
        public int? IdComment { set; get; }

        [DataMember]
        public int? IdPhoto { get; set; }
		
		[DataMember]
        public long? IdShare { get; set; }		
    }
	
	public class ActivityResponse
	{
		public Activity Activity {get;set;}
		public Comment Comment {get;set;}
		public User User {get;set;}
		public Image Image {get;set;}
	}
	
    public class Activities
    {
		public int? UserId {get;set;}
      	public long? Since { get; set; }
	}
	
    public class ActivitiesResponse
    {
        public ActivitiesResponse()
        {
            this.ResponseStatus = new ResponseStatus();
            this.Activities = new List<ActivityResponse>();
        }

        public ResponseStatus ResponseStatus { get; set; }

        public List<ActivityResponse> Activities { get; set; }
	}
}

