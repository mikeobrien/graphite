using System.IO;
using System.Text;
using Graphite.Writers;

namespace TestHarness.Writer
{
    public class WriterTestHandler
    {
        public class XmlModel
        {
            public string Value { get; set; }
        }

        public XmlModel GetXml()
        {
            return new XmlModel { Value = "fark" };
        }

        [OutputString("application/video", "weddingsinger.mp4", "ascii")]
        public string GetString1()
        {
            return "fark";
        }

        public OutputString GetString2()
        {
            return new OutputString
            {
                ContentType = "application/video",
                Filename = "weddingsinger.mp4",
                Data = "fark"
            };
        }

        [OutputStream("application/video", "weddingsinger.mp4", 1048576)]
        public Stream GetStream1()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes("fark"));
        }

        public OutputStream GetStream2()
        {
            return new OutputStream
            {
                ContentType = "application/video",
                Filename = "weddingsinger.mp4",
                BufferSize = 1048576,
                Data = new MemoryStream(Encoding.UTF8.GetBytes("fark"))
            };
        }

        [OutputBytes("application/video", "weddingsinger.mp4")]
        public byte[] GetBytes1()
        {
            return Encoding.UTF8.GetBytes("fark");
        }

        public OutputBytes GetBytes2()
        {
            return new OutputBytes
            {
                ContentType = "application/video",
                Filename = "weddingsinger.mp4",
                Data = Encoding.UTF8.GetBytes("fark")
            };
        }

        public Redirect GetRedirect()
        {
            return new Redirect
            {
                Type = RedirectType.Found,
                Url = "http://www.google.com"
            };
        }

        public class RedirectModel : IRedirectable
        {
            private readonly RedirectType _type;
            private readonly string _url;

            public RedirectModel()
            {
                _type = RedirectType.None;
            }

            public RedirectModel(RedirectType type, string url)
            {
                _type = type;
                _url = url;
            }

            RedirectType IRedirectable.Ridirect => _type;
            string IRedirectable.RidirectUrl => _url;

            public string Value { get; set; }
        }

        public RedirectModel GetRedirectModel(bool redirect)
        {
            return redirect ? new RedirectModel(RedirectType
                    .MovedPermanently, "http://www.google.com") :
                new RedirectModel { Value = "value" };
        }
    }
}