namespace Graphite.Http
{
    public class HttpMethod
    {
        public HttpMethod(string actionRegex, string method, bool allowRequestBody, bool allowResponseBody)
        {
            ActionRegex = actionRegex;
            Method = method;
            AllowRequestBody = allowRequestBody;
            AllowResponseBody = allowResponseBody;
        }

        public string ActionRegex { get; }
        public string Method { get; }
        public bool AllowRequestBody { get; }
        public bool AllowResponseBody { get; }

        public static readonly HttpMethod Get = new HttpMethod("^Get", "GET", false, true);
        public static readonly HttpMethod Post = new HttpMethod("^Post", "POST", true, true);
        public static readonly HttpMethod Put = new HttpMethod("^Put", "PUT", true, true);
        public static readonly HttpMethod Patch = new HttpMethod("^Patch", "PATCH", true, true);
        public static readonly HttpMethod Delete = new HttpMethod("^Delete", "DELETE", true, true);
        public static readonly HttpMethod Options = new HttpMethod("^Options", "OPTIONS", false, false);
        public static readonly HttpMethod Head = new HttpMethod("^Head", "HEAD", false, false);
        public static readonly HttpMethod Trace = new HttpMethod("^Trace", "TRACE", true, true);
        public static readonly HttpMethod Connect = new HttpMethod("^Connect", "CONNECT", true, true);
    }

    public class HttpMethodDsl
    {
        private string _actionRegex;
        private bool? _allowRequestBody;
        private bool? _allowResponseBody;
        private readonly HttpMethod _method;

        public HttpMethodDsl(HttpMethod method)
        {
            _method = method;
        }

        public HttpMethod Create()
        {
            return new HttpMethod(_actionRegex ?? _method.ActionRegex, _method.Method,
                _allowRequestBody ?? _method.AllowRequestBody,
                _allowResponseBody ?? _method.AllowResponseBody);
        }

        public HttpMethodDsl WithActionRegex(string actionRegex)
        {
            _actionRegex = actionRegex;
            return this;
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
