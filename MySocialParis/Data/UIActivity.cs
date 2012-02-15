using System;
using System.Collections.Generic;
using MSP.Client;
using MSP.Client.DataContracts;

namespace TweetStation
{
	public class UIActivity
	{		
		public int Id { get; set; }
		
		// To remove
		public string Text { get; set; }
		
		public Activity DbActivity {get;set;}
		public ActivityType Type {get;set;}
		public User User {get;set;}
		public Image Image {get;set;}
	}
}
