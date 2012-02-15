using System;
using System.Collections.Generic;

namespace MSP.Client
{
	public class Pagination
	{
		public string next_url {get;set;}
		public string next_max_id {get;set;}
	}
	
	public class Meta
	{
		public string code {get;set;}
	}
	
	public class Data
	{
			
	}
	
	public class InstaComments
	{
		public int count {get;set;}
		public List<InstaComment> data {get;set;}
	}
	
	public class From
	{
		public string username {get;set;}
		public string id {get;set;}
	}
	
	public class InstaLike
	{
		public string username{get;set;}		
	}
	
	public class InstaCaption
	{
		public string text {get;set;}
	}
	
	public class InstaImages
	{
		public InstaImage low_resolution {get;set;}
		public InstaImage thumbnail {get;set;}
		public InstaImage standard_resolution {get;set;}
	}
	
	public class InstaImage
	{
		public string url {get;set;}
		public int width {get;set;}
		public int height {get;set;}
	}
	
	public class InstaLikes
	{
		public int count {get;set;}
		public List<InstaLike> data {get;set;}
	}
	
	public class InstaComment
	{		
		public string text {get;set;}
		public From from {get;set;}
		public string id {get;set;}
	}
	
	public class InstaPopular
	{
		public InstaLocation location{get;set;}
		public InstaComments comments {get;set;}
		public string link {get;set;}
		public InstaImages images {get;set;}
		public InstaCaption caption {get;set;}
		public string id {get;set;}
		public InstaUser user {get;set;}
	}
	
	//"location": {"latitude": 55.04345, "longitude": -1.447952}
	public class InstaLocation
	{
		public string latitude {get;set;}
		public string longitude {get;set;}
	}
	
	public class EnvelopePopular
	{
		public Meta meta {get;set;}
		public List<InstaPopular> data{get;set;}
		public Pagination pagination {get;set;} 
	}
	
	public class InstaUser 
	{
		public string username {get;set;}
		public string profile_picture {get;set;}
		public string id {get;set;}
	}			
	
	public class Envelope
	{
		public Meta meta {get;set;}
		public Data data{get;set;}
		public Pagination pagination {get;set;} 
	}
}

