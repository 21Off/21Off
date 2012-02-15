using MSP.Client.DataContracts;

namespace MSP.Client
{
	public interface IPhotoUpdated
	{
		void UpdatedImage (string id);
	}
	
	public interface IDBImageUpdated
	{
		void UpdatedImage (Image image);
	}
}
