using System;
namespace MSP.Client
{
	public interface IConfiguration
	{		
		#region Properties
		
		int Id {get;}
		
		string Login {set;get;}		
		string Password {set;get;}
		string Email {set;get;}
		
		#endregion Properties		
	}
}

