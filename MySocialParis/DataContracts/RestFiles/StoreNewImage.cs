using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MSP.Client.DataContracts
{
    public class StoreNewImage
    {
        public Image Image { get; set; }
        public List<Keyword> Keywords { get; set; }
		public List<Comment> Comments { get; set; }
        public IOFile ImageFile { get; set; }
        public IOFile MapFile { get; set; }
		public int IdRelation {get;set;}
    }

    public class StoreNewImageResponse
    {
        public StoreNewImageResponse()
        {
            this.ResponseStatus = new ResponseStatus();
        }

        public int ImageId { get; set; }
        public int AlbumId { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
    }
	
    public class AddNewImageToAlbum
    {
        public StoreNewImage Image { get; set; }
        public int IdAlbum { get; set; }
    }

    public class CreateAlbumWithImage
    {
        public string Title { get; set; }
        public StoreNewImage Image { get; set; }
    }	


    [DataContract]
    public class IOFile
    {
        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public long ContentLength { get; set; }

        [DataMember]
        public string ContentType { get; set; }

        [DataMember]
        public byte[] InputBytes { get; set; }
    }
}

