
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using MonoTouch.Foundation;
using System.Threading;
using System.Collections.Specialized;
using System.Text;
namespace MSP.Client
{
}
namespace Krystalware.UploadHelper
{
    public class StreamMimePart : MimePart
    {
        Stream _data;

        public void SetStream(Stream stream)
        {
            _data = stream;
        }

        public override Stream Data
        {
            get
            {
                return _data;
            }
        }
    }
}
