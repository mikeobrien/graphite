using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using Bender;
using Graphite.Extensions;
using Graphite.Http;

namespace Tests.Common
{
    public static class WebClient
    {
        public const int Port = 3091;

        public class Result
        {
            public Result(HttpStatusCode status, string statusText, string contentType, string filename)
            {
                Status = status;
                StatusText = statusText;
                ContentType = contentType;
                Filename = filename;
            }

            public Result(HttpStatusCode status, string statusText, string error)
            {
                Status = status;
                StatusText = statusText;
                Error = error;
            }

            public string Error { get; }
            public HttpStatusCode Status { get; }
            public string StatusText { get; }
            public string ContentType { get; }
            public string Filename { get; }
        }

        public class Result<T> : Result
        {
            public Result(HttpStatusCode status, string statusText, string error) : 
                base(status, statusText, error) { }

            public Result(HttpStatusCode status, string statusText, 
                string contentType, string filename, T data) : 
                base(status, statusText, contentType, filename)
            {
                Data = data;
            }
            
            public T Data { get; }
        }

        public static Result<string> GetText(string relativeUrl)
        {
            return Get(relativeUrl, MimeTypes.TextPlain, x => new StreamReader(x).ReadToEnd());
        }

        public static Result<string> GetHtml(string relativeUrl)
        {
            return Get(relativeUrl, MimeTypes.TextHtml, x => new StreamReader(x).ReadToEnd());
        }

        public static Result<TResponse> GetJson<TResponse>(string relativeUrl)
        {
            return Get(relativeUrl, MimeTypes.ApplicationJson, x => Deserialize.Json<TResponse>(x));
        }

        public static Result<TResponse> GetXml<TResponse>(string relativeUrl)
        {
            return Get(relativeUrl, MimeTypes.ApplicationXml, x => Deserialize.Xml<TResponse>(x));
        }

        public static Result<string> GetString(string relativeUrl)
        {
            return Get<string>(relativeUrl, MimeTypes.ApplicationJson, x => x.ReadAllText());
        }

        private static Result<TResponse> Get<TResponse>(string relativeUrl,
            string accept, Func<Stream, TResponse> deserialize)
        {
            var request = (HttpWebRequest)WebRequest.Create(BuildUrl(relativeUrl));
            request.Method = HttpMethod.Get.Method;
            request.Accept = accept;
            return Execute(request, deserialize);
        }

        public static Result PostJson<TRequest>(string relativeUrl, TRequest data)
        {
            return Send<object>(HttpMethod.Post.Method, relativeUrl, MimeTypes.ApplicationJson, 
                MimeTypes.TextPlain, x => Serialize.JsonStream(data, x), x => null);
        }

        public static Result PutJson<TRequest>(string relativeUrl, TRequest data)
        {
            return Send<object>(HttpMethod.Put.Method, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.TextPlain, x => Serialize.JsonStream(data, x), x => null);
        }

        public static Result PatchJson<TRequest>(string relativeUrl, TRequest data)
        {
            return Send<object>(HttpMethod.Patch.Method, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.TextPlain, x => Serialize.JsonStream(data, x), x => null);
        }

        public static Result DeleteJson<TRequest>(string relativeUrl, TRequest data)
        {
            return Send<object>(HttpMethod.Delete.Method, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.TextPlain, x => Serialize.JsonStream(data, x), x => null);
        }

        public static Result<string> PostString(string relativeUrl, string data)
        {
            return Send(HttpMethod.Post.Method, relativeUrl, MimeTypes.TextPlain, MimeTypes.TextPlain,
                x => x.WriteAllText(data), x => x.ReadAllText());
        }

        public static Result<string> PostStream(string relativeUrl, Stream stream)
        {
            return Send(HttpMethod.Post.Method, relativeUrl, MimeTypes.TextPlain, MimeTypes.TextPlain,
                stream.CopyTo, x => x.ReadAllText());
        }

        public static Result GetStream(string relativeUrl, Action<Stream> action)
        {
            return Get<object>(relativeUrl, MimeTypes.ApplicationOctetStream, 
                x => { action(x); return null; });
        }

        public static Result<TResponse> PostStream<TResponse>(string relativeUrl, 
            Stream stream, string mimeType, string filename)
        {
            return Send< TResponse>(HttpMethod.Post.Method, relativeUrl, mimeType, MimeTypes
                .ApplicationJson, stream.CopyTo, x => Deserialize.Json<TResponse>(x), filename);
        }

        public static Result<TResponse> PostXml<TRequest, TResponse>(string relativeUrl, TRequest data)
        {
            return Send(HttpMethod.Post.Method, relativeUrl, MimeTypes.ApplicationXml, 
                MimeTypes.ApplicationXml, x => Serialize.XmlStream(data, x), 
                x => Deserialize.Xml<TResponse>(x));
        }

        public static Result<TResponse> PostJson<TRequest, TResponse>(string relativeUrl, TRequest data)
        {
            return Send(HttpMethod.Post.Method, relativeUrl, MimeTypes.ApplicationJson, 
                MimeTypes.ApplicationJson, x => Serialize.JsonStream(data, x), 
                x => Deserialize.Json<TResponse>(x));
        }

        public static Result<TResponse> PutJson<TRequest, TResponse>(string relativeUrl, TRequest data)
        {
            return Send(HttpMethod.Put.Method, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.ApplicationJson, x => Serialize.JsonStream(data, x),
                x => Deserialize.Json<TResponse>(x));
        }

        public static Result<TResponse> PatchJson<TRequest, TResponse>(string relativeUrl, TRequest data)
        {
            return Send(HttpMethod.Patch.Method, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.ApplicationJson, x => Serialize.JsonStream(data, x),
                x => Deserialize.Json<TResponse>(x));
        }

        public static Result<TResponse> DeleteJson<TRequest, TResponse>(string relativeUrl, TRequest data)
        {
            return Send(HttpMethod.Delete.Method, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.ApplicationJson, x => Serialize.JsonStream(data, x),
                x => Deserialize.Json<TResponse>(x));
        }

        public static Result<TResponse> PostForm<TResponse>(string relativeUrl, string data)
        {
            return Send(HttpMethod.Post.Method, relativeUrl, MimeTypes.ApplicationFormUrlEncoded, 
                MimeTypes.ApplicationJson, x => x.WriteAllText(data), 
                x => Deserialize.Json<TResponse>(x));
        }

        private static Result<TResponse> Send<TResponse>(string method, 
            string relativeUrl, string contentType, string accept,
            Action<Stream> serialize, Func<Stream, TResponse> deserialize,
            string filename = null)
        {
            var request = (HttpWebRequest)WebRequest.Create(BuildUrl(relativeUrl));
            request.Method = method;
            request.ContentType = contentType;
            request.Accept = accept;
            if (filename.IsNotNullOrEmpty())
                request.Headers.Add("Content-Disposition", $"attachment; filename=\"{filename}\"");
            using (var requestStream = request.GetRequestStream())
            {
                serialize(requestStream);
                return Execute(request, deserialize);
            }
        }

        private static Result<TResponse> Execute<TResponse>(HttpWebRequest request, 
            Func<Stream, TResponse> deserialize)
        {
            //Console.WriteLine($"{request.Method}:{request.RequestUri}");
            using (var response = GetResponse(request))
            {
                using (var responseStream = response.GetResponseStream())
                {
                    if ((int)response.StatusCode >= 300)
                    {
                        var responseData = responseStream.ReadAllText();
                        Console.WriteLine(responseData);
                        return new Result<TResponse>(response.StatusCode, 
                            response.StatusDescription, responseData);
                    }
                    return new Result<TResponse>(response.StatusCode,
                            response.StatusDescription, 
                            response.Headers["content-type"],
                            response.Headers["content-disposition"]?
                                .GetHeaderValues()["filename"].FirstOrDefault(),
                            deserialize(responseStream));
                }
            }
        }

        private static HttpWebResponse GetResponse(HttpWebRequest request)
        {
            try
            {
                return (HttpWebResponse)request.GetResponse();
            }
            catch (WebException e)
            {
                var response = e.Response as HttpWebResponse;
                if (response == null) throw;
                return response;
            }
        }

        private static string BuildUrl(string relativeUrl)
        {
            // Fiddler can't hook into localhost so when its running 
            // you can use localhost.fiddler
            var host = Process.GetProcessesByName("Fiddler").Any() ?
                    "localhost.fiddler" : "localhost";
            return $"http://{host}:{Port}/{relativeUrl}";
        }
    }
}
