using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Bender.Extensions;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Linq;

namespace TestHarness.Multipart
{
    public class MultipartTestHandler
    {
        public class InputModel
        {
            public string Value1 { get; set; }
            public int Value2 { get; set; }
        }

        public class OutputModel
        {
            public string Value1 { get; set; }
            public int Value2 { get; set; }
            public string Value3 { get; set; }
            public int Value4 { get; set; }
        }

        public OutputModel PostSingleValueToParameter(string value)
        {
            return new OutputModel
            {
                Value1 = value
            };
        }

        public OutputModel PostMultipleValuesToParameters(string value1, int value2)
        {
            return new OutputModel
            {
                Value1 = value1,
                Value2 = value2
            };
        }

        public OutputModel PostMultipleValuesToModel(InputModel model)
        {
            return new OutputModel
            {
                Value1 = model.Value1,
                Value2 = model.Value2
            };
        }

        public OutputModel PostMultipleValuesToModelAndParams(
            InputModel model, string value3, int value4)
        {
            return new OutputModel
            {
                Value1 = model.Value1,
                Value2 = model.Value2,
                Value3 = value3,
                Value4 = value4
            };
        }

        public class InputBodyModel
        {
            public string Name { get; set; }
            public string Filename { get; set; }
            public string ContentType { get; set; }
            public string Data { get; set; }
            public List<string> Encoding { get; set; }
            public long? Length { get; set; }
        }

        public List<InputBodyModel> PostStreamsToParam(
            IEnumerable<InputStream> streams)
        {
            return streams.Select(x => new InputBodyModel
            {
                Name = x.Name,
                Filename = x.Filename,
                ContentType = x.ContentType,
                Encoding = x.Encoding.ToList(),
                Length = x.Length,
                Data = x.Data.ReadToEnd()
            }).ToList();
        }

        public class InputBodyResponse
        {
            public string Value1 { get; set; }
            public int Value2 { get; set; }
            public string Value3 { get; set; }
            public int Value4 { get; set; }
            public List<InputBodyModel> Files { get; set; }
        }

        public InputBodyResponse PostStreamsAndValuesToParams(
            IEnumerable<InputStream> streams, string value1, int value2)
        {
            return new InputBodyResponse
            {
                Value1 = value1,
                Value2 = value2,
                Files = streams.Select(x => new InputBodyModel
                {
                    Name = x.Name,
                    Filename = x.Filename,
                    ContentType = x.ContentType,
                    Encoding = x.Encoding.ToList(),
                    Length = x.Length,
                    Data = x.Data.ReadToEnd()
                }).ToList()
            };
        }

        public class FilesInputStreamModel
        {
            public string Value1 { get; set; }
            public int Value2 { get; set; }
            public string Value3 { get; set; }
            public int Value4 { get; set; }
            public IEnumerable<InputStream> Files { get; set; }
        }

        public InputBodyResponse PostInputStreamsAndValuesToModel(FilesInputStreamModel request)
        {
            return new InputBodyResponse
            {
                Value1 = request.Value1,
                Value2 = request.Value2,
                Value3 = request.Value3,
                Value4 = request.Value4,
                Files = request.Files.Select(x => new InputBodyModel
                {
                    Name = x.Name,
                    Filename = x.Filename,
                    ContentType = x.ContentType,
                    Encoding = x.Encoding.ToList(),
                    Length = x.Length,
                    Data = x.Data.ReadToEnd()
                }).ToList()
            };
        }

        public class FilesStreamModel
        {
            public string Value1 { get; set; }
            public int Value2 { get; set; }
            public string Value3 { get; set; }
            public int Value4 { get; set; }
            public IEnumerable<Stream> Files { get; set; }
        }

        public InputBodyResponse PostOnlyStreamsAndValuesToModel(FilesStreamModel request)
        {
            return new InputBodyResponse
            {
                Value1 = request.Value1,
                Value2 = request.Value2,
                Value3 = request.Value3,
                Value4 = request.Value4,
                Files = request.Files.Select(x => new InputBodyModel
                {
                    Data = x.ReadToEnd()
                }).ToList()
            };
        }

        public List<InputBodyModel> PostLargeStreams(IEnumerable<InputStream> files)
        {
            return files.Select(x =>
            {
                var stream = new DummyStream();
                x.Data.CopyTo(stream);
                
                return new InputBodyModel
                {
                    Name = x.Name,
                    Filename = x.Filename,
                    ContentType = x.ContentType,
                    Encoding = x.Encoding.ToList(),
                    Length = stream.Length,
                    Data = stream.Checksum.ToString()
                };
            }).ToList();
        }
    }
    
    public class MultipartController : ApiController
    {
        [HttpPost]
        [Route("webapi/multipart")]
        public async Task<List<MultipartTestHandler.InputBodyModel>> Post()
        {                    
            var streamProvider = new DummyStreamProvider();
            await Request.Content.ReadAsMultipartAsync(streamProvider);

            return streamProvider.Streams
                .Select(x => new MultipartTestHandler.InputBodyModel
                {
                    Name = x.Headers.ContentDisposition.Name,
                    Filename = x.Headers.ContentDisposition.FileName,
                    ContentType = x.Headers.ContentType.MediaType,
                    Data = x.Stream.Checksum.ToString(),
                    Encoding = x.Headers.ContentEncoding.ToList(),
                    Length = x.Stream.Length
                }).ToList();
        }

        public class DummyStreamProvider : MultipartStreamProvider
        {
            public List<ContentStream> Streams { get; } = new List<ContentStream>();

            public override Stream GetStream(HttpContent parent, HttpContentHeaders headers)
            {
                var stream = new DummyStream();
                Streams.Add(new ContentStream(headers, stream));
                return stream;
            }

            public class ContentStream
            {
                public ContentStream(HttpContentHeaders headers, DummyStream stream)
                {
                    Headers = headers;
                    Stream = stream;
                }

                public HttpContentHeaders Headers { get; }
                public DummyStream Stream { get; }
            }
        }
    }
}