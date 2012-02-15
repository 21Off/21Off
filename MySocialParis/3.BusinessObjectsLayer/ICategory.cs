using System;

namespace MSP.Client
{
	public interface ICategory
	{
		#region Properties
		
		/// <summary>Image identity - Primary key</summary>
		int Id {get;}
		
		/// <summary>name</summary>
		string Name {set;get;}
		
		#endregion Properties		
	}
}

