namespace Graphite.Http
{
    public class HttpMethod
    {
        public HttpMethod(string method, bool allowRequestBody, bool allowResponseBody)
        {
            Method = method;
            AllowRequestBody = allowRequestBody;
            AllowResponseBody = allowResponseBody;
        }
        
        public string Method { get; }
        public bool AllowRequestBody { get; }
        public bool AllowResponseBody { get; }

        public static readonly HttpMethod Get = new HttpMethod("GET", false, true);
        public static readonly HttpMethod Post = new HttpMethod("POST", true, true);
        public static readonly HttpMethod Put = new HttpMethod("PUT", true, true);
        public static readonly HttpMethod Patch = new HttpMethod( "PATCH", true, true);
        public static readonly HttpMethod Delete = new HttpMethod("DELETE", true, true);
        public static readonly HttpMethod Options = new HttpMethod("OPTIONS", false, false);
        public static readonly HttpMethod Head = new HttpMethod("HEAD", false, false);
        public static readonly HttpMethod Trace = new HttpMethod("TRACE", true, true);
        public static readonly HttpMethod Connect = new HttpMethod("CONNECT", true, true);
    }

    public class HttpMethodDsl
    {
        private bool? _allowRequestBody;
        private bool? _allowResponseBody;
        private readonly HttpMethod _method;

        public HttpMethodDsl(HttpMethod method)
        {
            _method = method;
        }

        public HttpMethod Create()
        {
            return new HttpMethod(_method.Method,
                _allowRequestBody ?? _method.AllowRequestBody,
                _allowResponseBody ?? _method.AllowResponseBody);
        }
        
        public HttpMethodDsl AllowRequestBody(bool allow = true)
        {
            _allowRequestBody = allow;
            return this;
        }

        public HttpMethodDsl AllowResponseBody(bool allow = true)
        {
            _allowResponseBody = allow;
            return this;
        }
    }
}
