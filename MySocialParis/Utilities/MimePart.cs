
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
   public abstract class MimePart
    {
        NameValueCollection _headers = new NameValueCollection();
        byte[] _header;

        public NameValueCollection Headers
        {
            get { return _headers; }
        }

        public byte[] Header
        {
            get { return _header; }
        }

        public long GenerateHeaderFooterData(string boundary)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("--");
            sb.Append(boundary);
            sb.AppendLine();
            foreach (string key in _headers.AllKeys)
            {
                sb.Append(key);
                sb.Append(": ");
                sb.AppendLine(_headers[key]);
            }
            sb.AppendLine();

            _header = Encoding.UTF8.GetBytes(sb.ToString());

            return _header.Length + Data.Length + 2;
        }

        public abstract Stream Data { get; }
    }
}
