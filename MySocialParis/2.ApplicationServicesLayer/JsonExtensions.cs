using System;
using System.Json;
using System.Threading;
using MSP.Client.DataContracts;
using System.Collections.Generic;
using System.Reflection;
using MonoTouch.Foundation;

namespace MSP.Client
{
	
	public static class JsonExtensions
	{
		public static int TryGetInt(this JsonValue jv, string key)
		{
			return jv.ContainsKey(key) ? Convert.ToInt32(jv[key].ToString()) : 0;
		}
		public static string TryGetString(this JsonValue jv, string key)
		{
			return jv.ContainsKey(key) ? jv[key].ToString().Replace("\"", "") : null;
		}	
	}	
}
