using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MSP.Client.DataContracts
{
    public class StoreNewImage
    {
        [DataMember]
        public Image Image { get; set; }
        public List<Keyword> Keywords { get; set; }
        public IOFile ImageFile { get; set; }
        public IOFile MapFile { get; set; }
    }

    public class StoreNewImageResponse
    {
        public StoreNewImageResponse()
        {
            this.ResponseStatus = new ResponseStatus();
        }

        public int ImageId { get; set; }

        public ResponseStatus ResponseStatus { get; set; }
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

