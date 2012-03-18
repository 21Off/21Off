using System.Collections.Generic;
using MonoTouch.CoreLocation;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	public interface IMapLocationRequest
	{
		bool InitializeMap(RequestInfo request);
		LocalisationType LocType {get;set;}
		CLLocation Location{get;set;}
	 	IEnumerable<Image> GetDbImages(FilterType filterType, int start, int count);
		FilterType GetFilterType();
		List<ImageInfo> GetCurrentLoadedImages();
	}
}
