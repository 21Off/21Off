using System;
using System.Collections.Generic;
using MSP.Client;
using MSP.Client.DataContracts;

namespace TweetStation
{
	public class Tweet {	
		
		public User User {get;set;}
		public Image Image {get;set;}
		public List<Keyword> Keywords {get;set;}
		public List<Comment> Comments {get;set;}
		public List<User> CommentsUsers {get;set;}
		public int LikesCount {get;set;}

		public Action<Tweet> DeleteAction {get;set;}
		public Action<string> UrlTapAction {get;set;}
		
		public PostOptions Options {get;set;}
	}
	
	public enum PostOptions
	{
		CreateAlbum,
		AddToAlbum,
		ShareDescriptions,
		PostNew,
	}
}

