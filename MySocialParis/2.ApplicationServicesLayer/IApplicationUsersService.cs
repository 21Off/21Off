using System;
using System.Linq;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public interface IApplicationUsersService
	{
		User CreateUser(string userName, string password, string emailAddress, string profileImagePath);
		User Authentificate(string userName, string password);
		User GetUserById(int id);
	}
}

