namespace MSP.Client.DataContracts
{
	public class StoreNewUser
	{
		public string UserName { get; set; }
		public string Email { get; set; }
		public string Password { get; set; }
        public IOFile ImageFile { get; set; }
		public string Version { get; set; }
	}
}
