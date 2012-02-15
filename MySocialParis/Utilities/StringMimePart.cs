
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
    public class StringMimePart : MimePart
    {
        Stream _data;

        public string StringData
        {
            set
            {
                _data = new MemoryStream(Encoding.UTF8.GetBytes(value));
            }
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
