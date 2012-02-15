using System.Collections.Generic;
using System;

namespace MSP.Client.DataContracts
{
    public class Keywords
    {
		public int? Id{get;set;}
		public int? ImageId {get;set;}
        public Keyword Keyword { get; set; }
	}
	
    public class KeywordsResponse
    {
        public KeywordsResponse()
        {
            this.ResponseStatus = new ResponseStatus();
            this.Keywords = new List<Keyword>();
        }		
		
        public ResponseStatus ResponseStatus { get; set; }

        public List<Keyword> Keywords { get; set; }
	}	
	
	public class SimilarImage
	{
		public Image Image {get;set;}
		public string Keyword {get;set;}
	}
	
	public class SearchImagesByKeyword
	{
		public string Name {get;set;}
	}
	
	public class SearchImagesResponse
	{
		public SearchImagesResponse()
		{
            this.ResponseStatus = new ResponseStatus();
            this.Images = new List<SimilarImage>();
        }
		
		public DateTime Time {get; set;}
		
        public ResponseStatus ResponseStatus { get; set; }

        public List<SimilarImage> Images { get; set; }
	}
}

