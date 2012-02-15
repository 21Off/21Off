using System.Linq;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public class DBUsersService : AppServiceBase, IApplicationUsersService
	{
		private bool CheckIfUserExists (string userName)
		{						
			lock (Database.Main)
			{
				var user = Database.Main.Query<User> ("SELECT * FROM User where Name = ?", userName).FirstOrDefault ();
				return (user != null);
			}
		}		
		
		#region IApplicationUsersService implementation
		
		public User GetUserById(int id)
		{
			lock (Database.Main)
			{
				return Database.Main.Get<User>(id);
			}
		}
				
		public User CreateUser (string userName, string password, string emailAddress, string profileImagePath)
		{
			if (CheckIfUserExists(userName))
				return null;
			
			lock (Database.Main)
			{			
				var user = new User(){ Name = userName };
				
				var config = new Configuration()
				{
					Login = userName,
					Password = password,
					Email = emailAddress,
				};
				
				Database.Main.RunInTransaction(()=>
                {
					Database.Main.Insert(user);
					Database.Main.Insert(config);
				});
				
				return user;
			}
		}

		public User Authentificate (string userName, string password)
		{			
			lock (Database.Main)
			{
				var user = Database.Main.Query<User> ("SELECT * FROM User where Name = ?", userName).FirstOrDefault ();
				if (user == null)
				{
					//Util.ShowAlertSheet("L'utilisateur n'existe pas", View);
					return null;
				}
				
				var config = Database.Main.Query<Configuration> ("SELECT * FROM Configuration where Id = ?", user.Id).FirstOrDefault ();
				if (config == null)
				{
					//Util.ShowAlertSheet("Erreur de recuperation du profil", View);
					return null;
				}
				
				if (!(config.Login == userName && config.Password == password))
				{
					//Util.ShowAlertSheet("L'authentification a echouée", View);
					return null;
				}
				Util.Log("User connected : {0}", user.Id);
				
				return user;
			}
		}
		
		#endregion
	}
}
