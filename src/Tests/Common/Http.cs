using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml.Serialization;
using Graphite.Extensions;
using Graphite.Http;
using Newtonsoft.Json;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Tests.Common
{
    public static class Http
    {
        public static RestClient ForHost(Host host)
        {
            return new RestClient(WebServers.EnsureHost(host));
        }

        public static RestClient ForIISExpress()
        {
            return new RestClient(WebServers.EnsureHost(Host.IISExpress));
        }

        public static RestClient ForOwin()
        {
            return new RestClient(WebServers.EnsureHost(Host.Owin));
        }
    }

    public class RestClient
    {
        private static readonly HttpMethod Patch = new HttpMethod("PATCH");
        private readonly HttpClient _httpClient;

        public class Result
        {
            public Result(HttpResponseMessage response)
            {
                Status = response.StatusCode;
                ReasonPhrase = response.ReasonPhrase;
                ContentType = response.Content?.Headers?.ContentType?.MediaType;
                Headers = response.Headers;
                Cookies = Headers.Where(x => x.Key.EqualsUncase("Set-Cookie"))
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
            public string ReasonPhrase { get; }
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

        public RestClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Result Options(string relativeUrl,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null)
        {
            return Execute<object>(HttpMethod.Options, relativeUrl, 
                cookies: cookies, requestHeaders: requestHeaders);
        }

        public Result Get(string relativeUrl,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            string accept = null)
        {
            return Get<object>(relativeUrl, accept, 
                x => null, cookies, requestHeaders);
        }

        public Result<string> GetText(string relativeUrl,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null)
        {
            return Get(relativeUrl, MimeTypes.TextPlain, x => new StreamReader(x)
                .ReadToEnd(), cookies, requestHeaders);
        }

        public Result<string> GetHtml(string relativeUrl,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null)
        {
            return Get(relativeUrl, MimeTypes.TextHtml, 
                x => new StreamReader(x).ReadToEnd());
        }

        public Result<TResponse> GetJson<TResponse>(string relativeUrl,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            return Get(relativeUrl, MimeTypes.ApplicationJson, 
                DeserializeJson<TResponse>, cookies, requestHeaders);
        }

        public Result<TResponse> GetXml<TResponse>(string relativeUrl) where TResponse : class
        {
            return Get(relativeUrl, MimeTypes.ApplicationXml, DeserializeXml<TResponse>);
        }

        public Result<string> GetString(string relativeUrl,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null)
        {
            return Get(relativeUrl, MimeTypes.TextPlain, 
                x => x.ReadAllText(), cookies, requestHeaders);
        }

        public Result PostJson<TRequest>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null,
            string accept = null)
        {
            return Send<object>(HttpMethod.Post, relativeUrl, MimeTypes.ApplicationJson,
                accept, x => SerializeJson(data, x), x => null,
                cookies, requestHeaders, contentHeaders);
        }

        public Result PutJson<TRequest>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null)
        {
            return Send<object>(HttpMethod.Put, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.TextPlain, x => SerializeJson(data, x), x => null,
                cookies, requestHeaders, contentHeaders); ;
        }

        public Result PatchJson<TRequest>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null)
        {
            return Send<object>(Patch, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.TextPlain, x => SerializeJson(data, x), x => null,
                cookies, requestHeaders, contentHeaders);
        }

        public Result DeleteJson<TRequest>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null)
        {
            return Send<object>(HttpMethod.Delete, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.TextPlain, x => SerializeJson(data, x), x => null, 
                cookies, requestHeaders, contentHeaders);
        }

        public Result<string> PostString(string relativeUrl, string data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null)
        {
            return Send(HttpMethod.Post, relativeUrl, MimeTypes.TextPlain, MimeTypes.TextPlain,
                x => x.WriteAllText(data), x => x.ReadAllText(), cookies, requestHeaders, contentHeaders);
        }

        public Result<string> PostStream(string relativeUrl, Stream stream,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null)
        {
            return Send(HttpMethod.Post, relativeUrl, MimeTypes.TextPlain, MimeTypes.TextPlain,
                stream.CopyTo, x => x.ReadAllText(), cookies, requestHeaders, contentHeaders);
        }

        public Result<string> Post(string relativeUrl,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null)
        {
            return Send(HttpMethod.Post, relativeUrl, MimeTypes.TextPlain, MimeTypes.TextPlain,
                x => {}, x => x.ReadAllText(), cookies, requestHeaders, contentHeaders);
        }

        public Result GetStream(string relativeUrl, Action<Stream> action,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null)
        {
            return Get<object>(relativeUrl, MimeTypes.ApplicationOctetStream, 
                x => { action(x); return null; }, cookies, requestHeaders);
        }

        public Result<TResponse> PostStream<TResponse>(string relativeUrl, 
            Stream stream, string mimeType, string filename,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            return Send(HttpMethod.Post, relativeUrl, mimeType, MimeTypes
                .ApplicationJson, stream.CopyTo, DeserializeJson<TResponse>, filename: filename,
                cookies: cookies, requestHeaders: requestHeaders, contentHeaders: contentHeaders);
        }

        public Result<TResponse> PostMultipartForm<TResponse>(string relativeUrl,
            Action<MultipartFormDataContent> multipart,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            return Execute(HttpMethod.Post, relativeUrl, MimeTypes.MultipartFormData, 
                MimeTypes.ApplicationJson, DeserializeJson<TResponse>, null, cookies, 
                requestHeaders: requestHeaders, contentHeaders: contentHeaders,
                multipart: multipart);
        }

        public Result<TResponse> PostXml<TRequest, TResponse>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            return Send(HttpMethod.Post, relativeUrl, MimeTypes.ApplicationXml, 
                MimeTypes.ApplicationXml, x => SerializeXml(data, x), 
                DeserializeXml<TResponse>, cookies, requestHeaders, contentHeaders);
        }

        public Result<TResponse> PostJson<TRequest, TResponse>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null,
            string accept = null) where TResponse : class
        {
            return Send(HttpMethod.Post, relativeUrl, MimeTypes.ApplicationJson,
                accept ?? MimeTypes.ApplicationJson, x => SerializeJson(data, x), 
                DeserializeJson<TResponse>, cookies, requestHeaders, contentHeaders);
        }

        public Result<TResponse> PutJson<TRequest, TResponse>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            return Send(HttpMethod.Put, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.ApplicationJson, x => SerializeJson(data, x),
                DeserializeJson<TResponse>, cookies, requestHeaders, contentHeaders);
        }

        public Result<TResponse> PatchJson<TRequest, TResponse>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            return Send(Patch, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.ApplicationJson, x => SerializeJson(data, x),
                DeserializeJson<TResponse>, cookies, requestHeaders, contentHeaders);
        }

        public Result<TResponse> DeleteJson<TRequest, TResponse>(string relativeUrl, TRequest data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            return Send(HttpMethod.Delete, relativeUrl, MimeTypes.ApplicationJson,
                MimeTypes.ApplicationJson, x => SerializeJson(data, x),
                DeserializeJson<TResponse>, cookies, requestHeaders, contentHeaders);
        }

        public Result<TResponse> PostForm<TResponse>(string relativeUrl, string data,
            IDictionary<string, string> cookies = null,
            Action<HttpRequestHeaders> requestHeaders = null,
            Action<HttpContentHeaders> contentHeaders = null) where TResponse : class
        {
            return Send(HttpMethod.Post, relativeUrl, MimeTypes.ApplicationFormUrlEncoded, 
                MimeTypes.ApplicationJson, x => x.WriteAllText(data), 
                DeserializeJson<TResponse>, cookies, requestHeaders, contentHeaders);
        }

        private Result<TResponse> Get<TResponse>(string relativeUrl,
            string accept, Func<Stream, TResponse> reader,
            IDictionary<string, string> cookies = null, 
            Action<HttpRequestHeaders> requestHeaders = null) where TResponse : class
        {
            return Execute(HttpMethod.Get, relativeUrl, accept: accept, 
                cookies: cookies, requestHeaders: requestHeaders, reader: reader);
        }

        private Result<TResponse> Send<TResponse>(HttpMethod method, 
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

        private Result<TResponse> Execute<TResponse>(
            HttpMethod method, string relativeUrl, string contentType = null, 
            string accept = null, Func<Stream, TResponse> reader = null,
            Action<Stream> writer = null, IDictionary<string, string> cookies = null,
            string filename = null, Action<HttpRequestHeaders> requestHeaders = null, 
            Action<HttpContentHeaders> contentHeaders = null,
            Action<MultipartFormDataContent> multipart = null) where TResponse : class
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
            else if (multipart != null)
            {
                var form = new MultipartFormDataContent();
                multipart(form);
                request.Content = form;
            }

            var response = _httpClient.SendAsync(request).Result;
            var responseStream = response.Content.ReadAsStreamAsync().Result;

            Console.WriteLine(response.ToString());

            if ((int)response.StatusCode >= 300)
            {
                var responseData = responseStream.ReadAllText();
                Console.WriteLine(responseData);
                return new Result<TResponse>(response, responseData);
            }
            
            return new Result<TResponse>(response, reader?.Invoke(responseStream));
        }

        private static T DeserializeJson<T>(Stream stream)
        {
            return new JsonSerializer().Deserialize<T>(
                new JsonTextReader(new StreamReader(stream)));
        }

        private static void SerializeJson<T>(T instance, Stream stream)
        {
            var writer = new JsonTextWriter(new StreamWriter(stream));
            new JsonSerializer().Serialize(writer, instance);
            writer.Flush();
        }

        private static T DeserializeXml<T>(Stream stream)
        {
            return (T)new XmlSerializer(typeof(T)).Deserialize(stream);
        }

        private static void SerializeXml<T>(T instance, Stream stream)
        {
            new XmlSerializer(typeof(T)).Serialize(stream, instance);
        }
    }
}
