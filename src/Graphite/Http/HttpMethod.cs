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

        public HttpMethod WithActionRegex(string actionRegex)
        {
            return new HttpMethod(actionRegex, Method, AllowRequestBody, AllowResponseBody);
        }

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
}
