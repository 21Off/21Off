using MSP.Client;
using MSP.Client.DataContracts;
using SQLite;

namespace MSP.Client
{
	public class Database : SQLiteConnection
	{
		internal Database (string file) : base (file)
		{
			Util.ReportTime ("Database init");
			CreateTable<LastUserLogged>();
			CreateTable<User>();
			CreateTable<Configuration>();
			CreateTable<Image>();
			CreateTable<Keyword>();
			CreateTable<Activity>();
			//CreateTable<Category>();
			CreateTable<Follow>();
			CreateTable<Comment> ();
			CreateTable<Like>();
			Util.ReportTime ("Database finish");
		}
		
		static Database ()
		{
			// For debugging
			var db = Util.BaseDir + "/Documents/msp.db";
			//System.IO.File.Delete (db);
			Main = new Database (db);
		}
		
		static public Database Main { get; private set; }
	}
}

