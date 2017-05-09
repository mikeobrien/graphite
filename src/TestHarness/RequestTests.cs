using System.IO;
using System.Text;
using System.Web.Http;
using Bender.Extensions;
using Graphite.Readers;
using Graphite.Routing;
using Graphite.Writers;

namespace TestHarness
{
    namespace SomeNamespace
    {
        public class Handler
        {
            public TestHarness.Handler.OutputModel GetSomeMethod()
            {
                return new TestHarness.Handler.OutputModel { Value = "fark" };
            }
        }
    }

    public class Handler
    {
        public class OutputModel
        {
            public string Value { get; set; }
            public string Url1 { get; set; }
            public int Url2 { get; set; }
            public string UrlWildcard { get; set; }
            public int[] UrlMultiWildcard { get; set; }
            public string Query1 { get; set; }
            public int Query2 { get; set; }
            public string[] MultiQuery1 { get; set; }
            public int[] MultiQuery2 { get; set; }
            public string Form1 { get; set; }
            public int Form2 { get; set; }
            public string[] MultiForm1 { get; set; }
            public int[] MultiForm2 { get; set; }
        }

        public class InputModel
        {
            public string Value { get; set; }
            public string Form1 { get; set; }
            public int Form2 { get; set; }
            public string[] MultiForm1 { get; set; }
            public int[] MultiForm2 { get; set; }
        }

        public void GetWithNoResponse() { }

        [UrlAlias("SomeAlias")]
        public OutputModel GetWithResponse()
        {
            return new OutputModel { Value = "fark" };
        }

        public void GetWithQueryParam(int query) { }

        public OutputModel GetWithResponseUrlAndQueryParams_Url1_Segment_Url2(
            string url1, int url2, string query1, int query2)
        {
            return new OutputModel
            {
                Value = "fark",
                Url1 = url1,
                Url2 = url2,
                Query1 = query1,
                Query2 = query2
            };
        }

        public OutputModel GetWithMultiQueryParams(string[] query1, int[] query2)
        {
            return new OutputModel
            {
                MultiQuery1 = query1,
                MultiQuery2 = query2
            };
        }

        public OutputModel GetWithParamsWildcard_Ids(params int[] ids)
        {
            return new OutputModel
            {
                UrlMultiWildcard = ids
            };
        }

        public OutputModel GetWithAttriubteWildcard_Ids([Wildcard] int[] ids)
        {
            return new OutputModel
            {
                UrlMultiWildcard = ids
            };
        }

        public OutputModel GetWithSingleValueWildcard_Ids([Wildcard] string ids)
        {
            return new OutputModel
            {
                UrlWildcard = ids
            };
        }

        public OutputModel GetWithWildcard_Ids(params int[] ids)
        {
            return new OutputModel
            {
                UrlMultiWildcard = ids
            };
        }

        public void PostWithNoResponse(InputModel request) { }

        public OutputModel PostWithResponseUrlAndQueryParams_Url1_Segment_Url2(
            InputModel request, string url1, int url2, string query1, int query2)
        {
            return new OutputModel
            {
                Value = request.Value,
                Url1 = url1,
                Url2 = url2,
                Query1 = query1,
                Query2 = query2
            };
        }

        public OutputModel PostWithResponse(InputModel request)
        {
            return new OutputModel
            {
                Value = request.Value,
                Form1 = request.Form1,
                Form2 = request.Form2,
                MultiForm1 = request.MultiForm1,
                MultiForm2 = request.MultiForm2
            };
        }

        public string PostString(string request)
        {
            return request;
        }

        public OutputModel PostWithQueryParams([FromUri]string query1, int query2)
        {
            return new OutputModel
            {
                Query1 = query1,
                Query2 = query2
            };
        }

        public OutputModel PostWithMultiQueryParams([FromUri]string[] query1, int[] query2)
        {
            return new OutputModel
            {
                MultiQuery1 = query1,
                MultiQuery2 = query2
            };
        }

        public void PutWithNoResponse(InputModel request) { }

        public OutputModel PutWithResponseUrlAndQueryParams_Url1_Segment_Url2(
            InputModel request, string url1, int url2, string query1, int query2)
        {
            return new OutputModel
            {
                Value = request.Value,
                Url1 = url1,
                Url2 = url2,
                Query1 = query1,
                Query2 = query2
            };
        }

        public void PatchWithNoResponse(InputModel request) { }

        public OutputModel PatchWithResponseUrlAndQueryParams_Url1_Segment_Url2(
            InputModel request, string url1, int url2, string query1, int query2)
        {
            return new OutputModel
            {
                Value = request.Value,
                Url1 = url1,
                Url2 = url2,
                Query1 = query1,
                Query2 = query2
            };
        }

        public void DeleteWithNoResponse(InputModel request) { }

        public OutputModel DeleteWithResponseUrlAndQueryParams_Url1_Segment_Url2(
            InputModel request, string url1, int url2, string query1, int query2)
        {
            return new OutputModel
            {
                Value = request.Value,
                Url1 = url1,
                Url2 = url2,
                Query1 = query1,
                Query2 = query2
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
                Stream = new MemoryStream(Encoding.UTF8.GetBytes("fark"))
            };
        }

        public class StreamInfoModel
        {
            public string Data { get; set; }
            public string Filename { get; set; }
            public string MimeType { get; set; }
            public long? Length { get; set; }
        }

        public StreamInfoModel PostStream1(Stream stream)
        {
            return new StreamInfoModel
            {
                Data = stream.ReadToEnd()
            };
        }

        public StreamInfoModel PostStream2(InputStream stream)
        {
            return new StreamInfoModel
            {
                Filename = stream.Filename,
                MimeType = stream.MimeType,
                Length = stream.Length,
                Data = stream.Stream.ReadToEnd()
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