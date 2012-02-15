// Copyright 2010 Miguel de Icaza
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Linq;
using System.Text;
using System.Threading;
using SQLite;
using MonoTouch.Foundation;
using System.IO;
using System.Net;
using MSP.Client;

namespace TweetStation
{
	public partial class TwitterAccount
	{
		[PrimaryKey, AutoIncrement]
		public int LocalAccountId { get; set; }
		
		public static TwitterAccount FromId (int id)
		{
			if (accounts.ContainsKey (id)){
				return accounts [id];
			}
			
			lock (Database.Main){
				var account = Database.Main.Query<TwitterAccount> ("select * from TwitterAccount where LocalAccountId = ?", id).FirstOrDefault ();
				if (account != null)
					accounts [account.LocalAccountId] = account;
				
				return account;
			}
		}
		
		public static TwitterAccount Create (object oauth)
		{
			var account = new TwitterAccount () {
				Username = "AccessScreenname",
				AccountId = 1,
			};
			lock (Database.Main)
				Database.Main.Insert (account);
			accounts [account.LocalAccountId] = account;
			
			return account;
		}

		public static void Remove (TwitterAccount account)
		{
			var id = account.LocalAccountId;
			bool pickNewDefault = id == 1;
			
			if (accounts.ContainsKey (id))
				accounts.Remove (id);
			
			lock (Database.Main){
				Database.Main.Execute ("DELETE FROM Tweet where LocalAccountId = ?", account.LocalAccountId);
				Database.Main.Delete<TwitterAccount> (account);
			
				if (pickNewDefault){
					var newDefault = Database.Main.Query<TwitterAccount> ("SELECT LocalAccountId FROM TwitterAccount WHERE OAuthToken != \"\"").FirstOrDefault ();
					if (newDefault != null)
						Util.Defaults.SetInt (newDefault.LocalAccountId, "DEFAULT_ACCOUNT");
				}
			}
		}
		
		public string lasterror;
		
		public class QueuedTask2 {
			[PrimaryKey, AutoIncrement]
			public int TaskId { get; set; }
			public long AccountId { get; set; }
			public string Url { get; set; }

			public string PostData { get; set; }
			public string Verb { get; set; }
		}
	}
}

