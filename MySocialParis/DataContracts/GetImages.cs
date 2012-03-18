using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using SQLite;
using System.Collections;

namespace MSP.Client.DataContracts
{
	[DataContract]
	public class ResponseStatus
	{
		[DataMember]
		public string ErrorCode { get; set; }

		[DataMember]
		public string ErrorMessage { get; set; }

		[DataMember]
		public string StackTrace { get; set; }

		public bool IsSuccess { get { return ErrorCode == null; } }
	}	
	
    [DataContract(Namespace = ExampleConfig.DefaultNamespace)]
    public class Image: IImage, IEqualityComparer
    {
        //[AutoIncrement]
		[PrimaryKey]
        [DataMember]
        public int Id { get; set; }

        /// <summary>Identity of user who took the image</summary>
        [DataMember]
        public int UserId { get; set; }

        /// <summary>Image name</summary>
        [DataMember]
        public string Name { set; get; }

        /// <summary>Image shot time</summary>
        [DataMember]
        public DateTime Time { set; get; }

        /// <summary>Image shot position latitude</summary>
        [DataMember]
        public double Latitude { set; get; }

        /// <summary>Image shot position longitude</summary>
        [DataMember]
        public double Longitude { set; get; }

        /// <summary>Image shot position altitude</summary>
        [DataMember]
        public double Altitude { set; get; }

		#region IEqualityComparer implementation
		public bool Equals (object x, object y)
		{
			return ((Image)x).Id == ((Image)y).Id;
		}

		public int GetHashCode (object obj)
		{
			return Id;
		}
		#endregion
    }
	
	//[Description("Find images by geolocation and filter type")]
    public class Images
    {
        public int? Id { get; set; }
        public Image Image { get; set; }
		
		public GeoLoc WhereGeo {get;set;}
		public int? Filter {get;set;}
		public int Start {get;set;}
		public int Max {get;set;}
		public int WhereMyId {get;set;}
    }
	
	[DataContract(Namespace = ExampleConfig.DefaultNamespace)]
	public enum FilterType : int
	{
		Recent,
		Liked,
		Friends,
		All,
		Events,
	}
	
	[DataContract(Namespace = ExampleConfig.DefaultNamespace)]	
	public class GeoLoc
	{
		[DataMember]
		public double Longitude { get; set; }
		[DataMember]		
		public double Latitude { get; set; }
		[DataMember]
		public double Altitude { get; set; }		
	}
	
	public class AllImagesResponse
	{
        public AllImagesResponse()
        {
            this.ResponseStatus = new ResponseStatus();
			
            this.RecentImages = new List<Image>();
			this.FriendsImages = new List<Image>();
			this.LikedImages = new List<Image>();
        }

        public ResponseStatus ResponseStatus { get; set; }
		
		public List<Image> EventsImages { get; set; }
        public List<Image> RecentImages { get; set; }
		public List<Image> LikedImages { get; set; }
		public List<Image> FriendsImages { get; set; }
	}
	
    public class ImagesResponse
    {
        public ImagesResponse()
        {
            this.ResponseStatus = new ResponseStatus();
            this.Images = new List<Image>();
        }

        public ResponseStatus ResponseStatus { get; set; }

        public List<Image> Images { get; set; }
	}	
	
	[DataContract(Namespace = ExampleConfig.DefaultNamespace)]
	public enum LocalisationType
	{
		Local,
		Global,
	}	
}

