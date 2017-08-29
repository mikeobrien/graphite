using System.IO;
using System.Text;
using System.Web.Http;
using Graphite.Extensions;
using Graphite.Readers;

namespace TestHarness.Reader
{
    public class ReaderTestHandler
    {
        public class XmlModel
        {
            public string Value { get; set; }
        }

        public XmlModel PostXml(XmlModel request)
        {
            return request;
        }

        public class OutputInfoModel
        {
            public string Data { get; set; }
            public string Filename { get; set; }
            public string MimeType { get; set; }
            public long? Length { get; set; }
        }

        public OutputInfoModel PostString1([FromBody] string value)
        {
            return new OutputInfoModel
            {
                Data = value
            };
        }

        public OutputInfoModel PostString2(InputString stringInfo)
        {
            return new OutputInfoModel
            {
                Filename = stringInfo.Filename,
                MimeType = stringInfo.MimeType,
                Length = stringInfo.Length,
                Data = stringInfo.Data
            };
        }

        public OutputInfoModel PostStream1(Stream stream)
        {
            return new OutputInfoModel
            {
                Data = stream.ReadToEnd(Encoding.UTF8)
            };
        }

        public OutputInfoModel PostStream2(InputStream stream)
        {
            return new OutputInfoModel
            {
                Filename = stream.Filename,
                MimeType = stream.MimeType,
                Length = stream.Length,
                Data = stream.Data.ReadToEnd(Encoding.UTF8)
            };
        }

        public OutputInfoModel PostBytes1([FromBody] byte[] bytes)
        {
            return new OutputInfoModel
            {
                Data = Encoding.UTF8.GetString(bytes)
            };
        }

        public OutputInfoModel PostBytes2(InputBytes byteInfo)
        {
            return new OutputInfoModel
            {
                Filename = byteInfo.Filename,
                MimeType = byteInfo.MimeType,
                Length = byteInfo.Length,
                Data = Encoding.UTF8.GetString(byteInfo.Data)
            };
        }

        public class NoReaderModel { }

        public void PostNoReader(NoReaderModel model)
        {
        }
    }
}