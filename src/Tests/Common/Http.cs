using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using Bender;
using Graphite.Extensions;
using Graphite.Http;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Tests.Common
{
    public static class Http
    {
        public const int Port = 3091;

        private static readonly HttpClient HttpClient = 
            new HttpClient(new HttpClientHandler
            {
                UseCookies = false, AllowAutoRedirect = false
            })
            {
                // Fiddler can't hook into localhost so when its running 
                // you can use localhost.fiddler
                BaseAddress = new Uri($@"http://{(
                    Process.GetProcessesByName("Fiddler").Any() ?
                        "localhost.fiddler" : "localhost")}:{Port}/")
            };

        private static readonly HttpMethod Patch = new HttpMethod("PATCH");

        public class Result
        {
            public Result(HttpResponseMessage response)
            {
                Status = response.StatusCode;
                StatusText = response.ReasonPhrase;
                ContentType = response.Content?.Headers?.ContentType?.MediaType;
                Headers = response.Headers;
                Cookies = Headers.Where(x => x.Key.EqualsIgnoreCase("Set-Cookie"))
                    .SelectMany(x => x.Value)
                    .Select(x => x.Split('='))
                    .ToDictionary(x => x[0].Trim(), x => x[1].Trim(),
                        StringComparer.OrdinalIgnoreCase); ;
                Filename = response.Content?.Headers?.ContentDisposition?.FileName;
                WasRedirected = Status.IsRedirect();
                RedirectUrl = response.Headers.Location?.ToString();
            }

            public Result(HttpResponseMessage response, 
                string error) : this(response)
            {
                Error = error;
            }

            public string Error { get; }
            public HttpStatusCode Status { get; }
            public string StatusText { get; }
            public string ContentType { get; }
            public string Filename { get; }
            public bool WasRedirected { get; }
            public string RedirectUrl { get; }
            public HttpResponseHeaders Headers { get; }
            public Dictionary<string, string> Cookies { get; }
        }

        public class Result<T> : Result
        {
            public Result(HttpResponseMessage response, string error) :
                base(response, error) { }

            public Result(HttpResponseMessage response, T data) : 
                base(response)
            {
                Data = data;
            }

            public T Data { get; }
        }

        public static Result Get(string relativeUrl,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null)
        {
            return Get<object>(relativeUrl, MimeTypes.ApplicationJson, x => null, cookies, requestHeaders);
        }

        public static Result<string> GetText(string relativeUrl,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null)
        {
            return Get(relativeUrl, MimeTypes.TextPlain, x => new StreamReader(x)
                .ReadToEnd(), cookies, requestHeaders);
        }

        public static Result<string> GetHtml(string relativeUrl,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null)
        {
            return Get(relativeUrl, MimeTypes.TextHtml, x => new StreamReader(x).ReadToEnd());
        }

        public static Result<TResponse> GetJson<TResponse>(string relativeUrl,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            return Get(relativeUrl, MimeTypes.ApplicationJson, x => 
                Deserialize.Json<TResponse>(x), cookies, requestHeaders);
        }

        public static Result<TResponse> GetXml<TResponse>(string relativeUrl) where TResponse : class
        {
            return Get(relativeUrl, MimeTypes.ApplicationXml, x => Deserialize.Xml<TResponse>(x));
        }

        public static Result<string> GetString(string relativeUrl,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null)
        {
            return Get<string>(relativeUrl, MimeTypes.ApplicationJson, 
                x => x.ReadAllText(), cookies, requestHeaders);
        }

        public static Result PostJson<TRequest>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null)
        {
            return Send<object>(HttpMethod.Post, relativeUrl, MimeTypes.ApplicationJson, 
                MimeTypes.TextPlain, x => Serialize.JsonStream(data, x), x => null,
                cookies, requestHeaders, contentHeaders);
        }

        public static Result PutJson<TRequest>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null)
        {
            return Send<object>(HttpMethod.Put, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.TextPlain, x => Serialize.JsonStream(data, x), x => null,
                cookies, requestHeaders, contentHeaders); ;
        }

        public static Result PatchJson<TRequest>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null)
        {
            return Send<object>(Patch, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.TextPlain, x => Serialize.JsonStream(data, x), x => null,
                cookies, requestHeaders, contentHeaders);
        }

        public static Result DeleteJson<TRequest>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null)
        {
            return Send<object>(HttpMethod.Delete, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.TextPlain, x => Serialize.JsonStream(data, x), x => null, 
                cookies, requestHeaders, contentHeaders);
        }

        public static Result<string> PostString(string relativeUrl, string data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null)
        {
            return Send(HttpMethod.Post, relativeUrl, MimeTypes.TextPlain, MimeTypes.TextPlain,
                x => x.WriteAllText(data), x => x.ReadAllText(), cookies, requestHeaders, contentHeaders);
        }

        public static Result<string> PostStream(string relativeUrl, Stream stream,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null)
        {
            return Send(HttpMethod.Post, relativeUrl, MimeTypes.TextPlain, MimeTypes.TextPlain,
                stream.CopyTo, x => x.ReadAllText(), cookies, requestHeaders, contentHeaders);
        }

        public static Result<string> Post(string relativeUrl,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null)
        {
            return Send(HttpMethod.Post, relativeUrl, MimeTypes.TextPlain, MimeTypes.TextPlain,
                x => {}, x => x.ReadAllText(), cookies, requestHeaders, contentHeaders);
        }

        public static Result GetStream(string relativeUrl, Action<Stream> action,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null)
        {
            return Get<object>(relativeUrl, MimeTypes.ApplicationOctetStream, 
                x => { action(x); return null; }, cookies, requestHeaders);
        }

        public static Result<TResponse> PostStream<TResponse>(string relativeUrl, 
            Stream stream, string mimeType, string filename,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            return Send(HttpMethod.Post, relativeUrl, mimeType, MimeTypes
                .ApplicationJson, stream.CopyTo, x => Deserialize.Json<TResponse>(x), filename: filename,
                cookies: cookies, requestHeaders: requestHeaders, contentHeaders: contentHeaders);
        }

        public static Result<TResponse> PostXml<TRequest, TResponse>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            return Send(HttpMethod.Post, relativeUrl, MimeTypes.ApplicationXml, 
                MimeTypes.ApplicationXml, x => Serialize.XmlStream(data, x), 
                x => Deserialize.Xml<TResponse>(x), cookies, requestHeaders, contentHeaders);
        }

        public static Result<TResponse> PostJson<TRequest, TResponse>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            return Send(HttpMethod.Post, relativeUrl, MimeTypes.ApplicationJson, 
                MimeTypes.ApplicationJson, x => Serialize.JsonStream(data, x), 
                x => Deserialize.Json<TResponse>(x), cookies, requestHeaders, contentHeaders);
        }

        public static Result<TResponse> PutJson<TRequest, TResponse>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            return Send(HttpMethod.Put, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.ApplicationJson, x => Serialize.JsonStream(data, x),
                x => Deserialize.Json<TResponse>(x), cookies, requestHeaders, contentHeaders);
        }

        public static Result<TResponse> PatchJson<TRequest, TResponse>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            return Send(Patch, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.ApplicationJson, x => Serialize.JsonStream(data, x),
                x => Deserialize.Json<TResponse>(x), cookies, requestHeaders, contentHeaders);
        }

        public static Result<TResponse> DeleteJson<TRequest, TResponse>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            return Send(HttpMethod.Delete, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.ApplicationJson, x => Serialize.JsonStream(data, x),
                x => Deserialize.Json<TResponse>(x), cookies, requestHeaders, contentHeaders);
        }

        public static Result<TResponse> PostForm<TResponse>(string relativeUrl, string data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            return Send(HttpMethod.Post, relativeUrl, MimeTypes.ApplicationFormUrlEncoded, 
                MimeTypes.ApplicationJson, x => x.WriteAllText(data), 
                x => Deserialize.Json<TResponse>(x), cookies, requestHeaders, contentHeaders);
        }

        private static Result<TResponse> Get<TResponse>(string relativeUrl,
            string accept, Func<Stream, TResponse> reader,
            IDictionary<string, string> cookies = null, 
            Action<HttpRequestHeaders> requestHeaders = null) where TResponse : class
        {
            return Execute(HttpMethod.Get, relativeUrl, accept: accept, 
                cookies: cookies, requestHeaders: requestHeaders, reader: reader);
        }

        private static Result<TResponse> Send<TResponse>(HttpMethod method, 
            string relativeUrl, string contentType, string accept,
            Action<Stream> writer, Func<Stream, TResponse> reader,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null,
            string filename = null) where TResponse : class
        {
            return Execute(method, relativeUrl, contentType, accept, reader, writer,
                cookies, filename, requestHeaders, contentHeaders);
        }

        private static Result<TResponse> Execute<TResponse>(
            HttpMethod method, string relativeUrl, string contentType = null, 
            string accept = null, Func<Stream, TResponse> reader = null,
            Action<Stream> writer = null, IDictionary<string, string> cookies = null,
            string filename = null, Action<HttpRequestHeaders> requestHeaders = null, 
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            var request = new HttpRequestMessage(method, relativeUrl);

            if (accept.IsNotNullOrEmpty())
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(accept));

            if (cookies != null && cookies.Any())
                request.Headers.Add("Cookie", cookies.Select(x => $"{x.Key}={x.Value}").Join("; "));

            requestHeaders?.Invoke(request.Headers);

            if (writer != null)
            {
                var requestStream = new MemoryStream();
                writer(requestStream);
                requestStream.Position = 0;
                request.Content = new StreamContent(requestStream);
                if (contentType.IsNotNullOrEmpty())
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                if (filename.IsNotNullOrEmpty())
                    request.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue
                        ("attachment") { FileName = filename };

                contentHeaders?.Invoke(request.Content.Headers);
            }
            
            var response = HttpClient.SendAsync(request).Result;
            var responseStream = response.Content.ReadAsStreamAsync().Result;

            if ((int)response.StatusCode >= 300)
            {
                var responseData = responseStream.ReadAllText();
                Console.WriteLine(responseData);
                return new Result<TResponse>(response, responseData);
            }
            
            return new Result<TResponse>(response, reader?.Invoke(responseStream));
        }
    }
}
