using MSP.Client;

namespace TweetStation
{
	public interface ISizeImageUpdated {
		void UpdatedImage (long id, long userid, SizeDB sizeDB);
	}
}
