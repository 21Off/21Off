using System;

namespace MSP.Client
{
	/// <summary>
	/// Singleton template class.
	/// </summary>
	public class Singleton<T> where T : class, new()
	{
		/// <summary>
		/// Protected constructor prevents instantination from other classes.
		/// </summary>
		protected Singleton()
		{
		}
		
		/// <summary>
		/// The single instance of our template parameter class.
		/// </summary>
		private static T _instance = new T();
				
		/// <summary>
		/// The read-only instance property.
		/// </summary>
		public static T Instance
		{
			get
			{
				return _instance;
			}
		}	
	}
}
