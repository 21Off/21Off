using System;
using System.Collections.Generic;
using MSP.Client.DataContracts;

namespace MSP.Client
{
	/// <summary>
	/// 
	/// </summary>
	public interface IBusinessImagesService
	{
		void DeleteImage(Image image);
		Image GetImage(int id);
		IEnumerable<Image> GetImageList(FilterType filterType, GeoLoc geoLoc, int startIndex, int maximumImages, int userId);
	}
}

