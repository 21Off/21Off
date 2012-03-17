
using MonoTouch.UIKit;
using System;

namespace MSP.Client
{
	public class Application
	{
		static void Main (string[] args)
		{
			try
			{
				UIApplication.Main (args, null, "AppDelegateIPhone");
			}
			catch (Exception ex)
			{
				Util.LogException("Main", ex);
			}
		}
	}
}

