using System;
using System.Collections.Generic;
using MSP.Client;
using MSP.Client.DataContracts;

namespace TweetStation
{
	/// <summary>
	///   Represents a tweet in memory.   Not all the data from the original tweet
	///   is kept around, most of the data is discarded.
	/// </summary>
	public class Tweet {	
		
		public User User {get;set;}
		public Image Image {get;set;}
		public List<Keyword> Keywords {get;set;}
		public List<Comment> Comments {get;set;}
		public List<User> CommentsUsers {get;set;}
		public int LikesCount {get;set;}

		public Action<Tweet> DeleteAction {get;set;}
	}
	

}

