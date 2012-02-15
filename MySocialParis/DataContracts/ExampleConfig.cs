

namespace MSP.Client.DataContracts
{
	public class ExampleConfig
	{
		/// <summary>
		/// Would've preferred to use [assembly: ContractNamespace] attribute but it is not supported in Mono
		/// </summary>
		public const string DefaultNamespace = "http://schemas.servicestack.net/types";
	}
}
