namespace Graphite.Http
{
    public class RequestHeaders
    {
        /// <summary>
        /// Content-Types that are acceptable for the response.
        /// </summary>
        public const string Accept = "Accept";

        /// <summary>
        /// Character sets that are acceptable.
        /// </summary>
        public const string AcceptCharset = "Accept-Charset";

        /// <summary>
        /// List of acceptable encodings.
        /// </summary>
        public const string AcceptEncoding = "Accept-Encoding";

        /// <summary>
        /// List of acceptable human languages for response.
        /// </summary>
        public const string AcceptLanguage = "Accept-Language";

        /// <summary>
        /// Acceptable version in time.
        /// </summary>
        public const string AcceptDatetime = "Accept-Datetime";

        /// <summary>
        /// Initiates a request for cross-origin resource sharing with Origin.
        /// </summary>
        public const string AccessControlRequestMethod = "Access-Control-Request-Method";

        /// <summary>
        /// Initiates a request for cross-origin resource sharing with Origin.
        /// </summary>
        public const string AccessControlRequestHeaders = "Access-Control-Request-Headers";

        /// <summary>
        /// Authentication credentials for HTTP authentication.
        /// </summary>
        public const string Authorization = "Authorization";

        /// <summary>
        /// Used to specify directives that must be obeyed by all caching mechanisms along the request-response chain.
        /// </summary>
        public const string CacheControl = "Cache-Control";

        /// <summary>
        /// Control options for the current connection and list of hop-by-hop request fields. Must not be used with HTTP/2.
        /// </summary>
        public const string Connection = "Connection";

        /// <summary>
        /// An HTTP cookie previously sent by the server with Set-Cookie.
        /// </summary>
        public const string Cookie = "Cookie";

        /// <summary>
        /// The length of the request body in octets (8-bit bytes).
        /// </summary>
        public const string ContentLength = "Content-Length";

        /// <summary>
        /// An opportunity to raise a "File Download" dialogue box for a known MIME type with binary 
        /// format or suggest a filename for dynamic content. Quotes are necessary with special characters.
        /// </summary>
        public const string ContentDisposition = "Content-Disposition";

        /// <summary>
        /// The type of encoding used on the data.
        /// </summary>
        public const string ContentEncoding = "Content-Encoding";

        /// <summary>
        /// The natural language or languages of the intended audience for the enclosed content.
        /// </summary>
        public const string ContentLanguage = "Content-Language";

        /// <summary>
        /// A Base64-encoded binary MD5 sum of the content of the request body.
        /// </summary>
        public const string ContentMD5 = "Content-MD5";

        /// <summary>
        /// The MIME type of the body of the request (used with POST and PUT requests).
        /// </summary>
        public const string ContentType = "Content-Type";

        /// <summary>
        /// The date and time that the message was originated (in "HTTP-date" format as defined by RFC 7231 Date/Time Formats).
        /// </summary>
        public const string Date = "Date";

        /// <summary>
        /// Indicates that particular server behaviors are required by the client.
        /// </summary>
        public const string Expect = "Expect";

        /// <summary>
        /// Disclose original information of a client connecting to a web server through an HTTP proxy.
        /// </summary>
        public const string Forwarded = "Forwarded";

        /// <summary>
        /// The email address of the user making the request.
        /// </summary>
        public const string From = "From";

        /// <summary>
        /// The domain name of the server (for virtual hosting), and the TCP port number on which the server is listening. 
        /// The port number may be omitted if the port is the standard port for the service requested.
        /// Mandatory since HTTP/1.1. If the request is generated directly in HTTP/2, it should not be used.
        /// </summary>
        public const string Host = "Host";

        /// <summary>
        /// Only perform the action if the client supplied entity matches the same entity on the server. 
        /// This is mainly for methods like PUT to only update a resource if it has not been modified since the user last updated it.
        /// </summary>
        public const string IfMatch = "If-Match";

        /// <summary>
        /// Allows a 304 Not Modified to be returned if content is unchanged.
        /// </summary>
        public const string IfModifiedSince = "If-Modified-Since";

        /// <summary>
        /// Allows a 304 Not Modified to be returned if content is unchanged.
        /// </summary>
        public const string IfNoneMatch = "If-None-Match";

        /// <summary>
        /// If the entity is unchanged, send me the part(s) that I am missing; otherwise, send me the entire new entity.
        /// </summary>
        public const string IfRange = "If-Range";

        /// <summary>
        /// Only send the response if the entity has not been modified since a specific time.
        /// </summary>
        public const string IfUnmodifiedSince = "If-Unmodified-Since";

        /// <summary>
        /// Limit the number of times the message can be forwarded through proxies or gateways.
        /// </summary>
        public const string MaxForwards = "Max-Forwards";

        /// <summary>
        /// Initiates a request for cross-origin resource sharing (asks server for Access-Control-* response fields).
        /// </summary>
        public const string Origin = "Origin";

        /// <summary>
        /// Implementation-specific fields that may have various effects anywhere along the request-response chain.
        /// </summary>
        public const string Pragma = "Pragma";

        /// <summary>
        /// Authorization credentials for connecting to a proxy.
        /// </summary>
        public const string ProxyAuthorization = "Proxy-Authorization";

        /// <summary>
        /// Request only part of an entity. Bytes are numbered from 0.
        /// </summary>
        public const string Range = "Range";

        /// <summary>
        /// This is the address of the previous web page from which a link to the currently requested page was followed. 
        /// The word “referrer” has been misspelled in the RFC as well as in most implementations to the point that it has 
        /// become standard usage and is considered correct terminology)
        /// </summary>
        public const string Referer = "Referer";

        /// <summary>
        /// The transfer encodings the user agent is willing to accept: the same values as for the response header field 
        /// Transfer-Encoding can be used, plus the "trailers" value (related to the "chunked" transfer method) to notify 
        /// the server it expects to receive additional fields in the trailer after the last, zero-sized, chunk.
        /// Only trailers is supported in HTTP/2.
        /// </summary>
        public const string TE = "TE";

        /// <summary>
        /// The user agent string of the user agent.
        /// </summary>
        public const string UserAgent = "User-Agent";

        /// <summary>
        /// Ask the server to upgrade to another protocol. Must not be used to upgrade to HTTP/2.
        /// </summary>
        public const string Upgrade = "Upgrade";

        /// <summary>
        /// Informs the server of proxies through which the request was sent.
        /// </summary>
        public const string Via = "Via";

        /// <summary>
        /// A general warning about possible problems with the entity body.
        /// </summary>
        public const string Warning = "Warning";
    }

    public class ResponseHeaders
    {
        /// <summary>
        /// Specifying which web sites can participate in cross-origin resource sharing
        /// </summary>
        public const string AccessControlAllowOrigin = "Access-Control-Allow-Origin";

        /// <summary>
        /// Specifying which web sites can participate in cross-origin resource sharing
        /// </summary>
        public const string AccessControlAllowCredentials = "Access-Control-Allow-Credentials";

        /// <summary>
        /// Specifying which web sites can participate in cross-origin resource sharing
        /// </summary>
        public const string AccessControlExposeHeaders = "Access-Control-Expose-Headers";

        /// <summary>
        /// Specifying which web sites can participate in cross-origin resource sharing
        /// </summary>
        public const string AccessControlMaxAge = "Access-Control-Max-Age";

        /// <summary>
        /// Specifying which web sites can participate in cross-origin resource sharing
        /// </summary>
        public const string AccessControlAllowMethods = "Access-Control-Allow-Methods";

        /// <summary>
        /// Specifying which web sites can participate in cross-origin resource sharing
        /// </summary>
        public const string AccessControlAllowHeaders = "Access-Control-Allow-Headers";

        /// <summary>
        /// Specifies which patch document formats this server supports.
        /// </summary>
        public const string AcceptPatch = "Accept-Patch";

        /// <summary>
        /// What partial content range types this server supports via byte serving.
        /// </summary>
        public const string AcceptRanges = "Accept-Ranges";

        /// <summary>
        /// The age the object has been in a proxy cache in seconds.
        /// </summary>
        public const string Age = "Age";

        /// <summary>
        /// Valid methods for a specified resource. To be used for a 405 Method not allowed.
        /// </summary>
        public const string Allow = "Allow";

        /// <summary>
        /// A server uses "Alt-Svc" header (meaning Alternative Services) to indicate that its resources can also be 
        /// accessed at a different network location (host or port) or using a different protocol.
        /// When using HTTP/2, servers should instead send an ALTSVC frame.
        /// </summary>
        public const string AltSvc = "Alt-Svc";

        /// <summary>
        /// Tells all caching mechanisms from server to client whether they may cache this object. It is measured in seconds.
        /// </summary>
        public const string CacheControl = "Cache-Control";

        /// <summary>
        /// Control options for the current connection and list of hop-by-hop response fields. Must not be used with HTTP/2.
        /// </summary>
        public const string Connection = "Connection";

        /// <summary>
        /// An opportunity to raise a "File Download" dialogue box for a known MIME type with binary 
        /// format or suggest a filename for dynamic content. Quotes are necessary with special characters.
        /// </summary>
        public const string ContentDisposition = "Content-Disposition";

        /// <summary>
        /// The type of encoding used on the data.
        /// </summary>
        public const string ContentEncoding = "Content-Encoding";

        /// <summary>
        /// The natural language or languages of the intended audience for the enclosed content.
        /// </summary>
        public const string ContentLanguage = "Content-Language";

        /// <summary>
        /// The length of the response body in octets (8-bit bytes).
        /// </summary>
        public const string ContentLength = "Content-Length";

        /// <summary>
        /// An alternate location for the returned data.
        /// </summary>
        public const string ContentLocation = "Content-Location";

        /// <summary>
        /// A Base64-encoded binary MD5 sum of the content of the response.
        /// </summary>
        public const string ContentMD5 = "Content-MD5";

        /// <summary>
        /// Where in a full body message this partial message belongs.
        /// </summary>
        public const string ContentRange = "Content-Range";

        /// <summary>
        /// The MIME type of this content.
        /// </summary>
        public const string ContentType = "Content-Type";

        /// <summary>
        /// The date and time that the message was sent (in "HTTP-date" format as defined by RFC 7231).
        /// </summary>
        public const string Date = "Date";

        /// <summary>
        /// An identifier for a specific version of a resource, often a message digest.
        /// </summary>
        public const string ETag = "ETag";

        /// <summary>
        /// Gives the date/time after which the response is considered stale (in "HTTP-date" format as defined by RFC 7231).
        /// </summary>
        public const string Expires = "Expires";

        /// <summary>
        /// The last modified date for the requested object (in "HTTP-date" format as defined by RFC 7231).
        /// </summary>
        public const string LastModified = "Last-Modified";

        /// <summary>
        /// Used to express a typed relationship with another resource, where the relation type is defined by RFC 5988.
        /// </summary>
        public const string Link = "Link";

        /// <summary>
        /// Used in redirection, or when a new resource has been created.
        /// </summary>
        public const string Location = "Location";

        /// <summary>
        /// This field is supposed to set P3P policy, in the form of P3P:CP="your_compact_policy". However, 
        /// P3P did not take off,[40] most browsers have never fully implemented it, a lot of websites set this field
        /// with fake policy text, that was enough to fool browsers the existence of P3P policy and 
        /// grant permissions for third party cookies.
        /// </summary>
        public const string P3P = "P3P";

        /// <summary>
        /// Implementation-specific fields that may have various effects anywhere along the request-response chain.
        /// </summary>
        public const string Pragma = "Pragma";

        /// <summary>
        /// Request authentication to access the proxy.
        /// </summary>
        public const string ProxyAuthenticate = "Proxy-Authenticate";

        /// <summary>
        /// HTTP Public Key Pinning, announces hash of website's authentic TLS certificate.
        /// </summary>
        public const string PublicKeyPins = "Public-Key-Pins";

        /// <summary>
        /// If an entity is temporarily unavailable, this instructs the client to try again later. 
        /// Value could be a specified period of time (in seconds) or a HTTP-date.
        /// </summary>
        public const string RetryAfter = "Retry-After";

        /// <summary>
        /// A name for the server.
        /// </summary>
        public const string Server = "Server";

        /// <summary>
        /// An HTTP cookie.
        /// </summary>
        public const string SetCookie = "Set-Cookie";

        /// <summary>
        /// A HSTS Policy informing the HTTP client how long to cache the HTTPS only policy and whether this applies to subdomains.
        /// </summary>
        public const string StrictTransportSecurity = "Strict-Transport-Security";

        /// <summary>
        /// The Trailer general field value indicates that the given set of header fields is present in the trailer of a message encoded with chunked transfer coding.
        /// </summary>
        public const string Trailer = "Trailer";

        /// <summary>
        /// The form of encoding used to safely transfer the entity to the user. 
        /// Currently defined methods are: chunked, compress, deflate, gzip, identity.
        /// Must not be used with HTTP/2.
        /// </summary>
        public const string TransferEncoding = "Transfer-Encoding"; 

        /// <summary>
        /// Tracking Status header, value suggested to be sent in response to a DNT(do-not-track).
        /// </summary>
        public const string Tk = "Tk";

        /// <summary>
        /// Ask the client to upgrade to another protocol. Must not be used to upgrade to HTTP/2.
        /// </summary>
        public const string Upgrade = "Upgrade";

        /// <summary>
        /// Tells downstream proxies how to match future request headers to decide whether the 
        /// cached response can be used rather than requesting a fresh one from the origin server.
        /// </summary>
        public const string Vary = "Vary";

        /// <summary>
        /// Informs the client of proxies through which the response was sent.
        /// </summary>
        public const string Via = "Via";

        /// <summary>
        /// A general warning about possible problems with the entity body.
        /// </summary>
        public const string Warning = "Warning";

        /// <summary>
        /// Indicates the authentication scheme that should be used to access the requested entity.
        /// </summary>
        public const string WWWAuthenticate = "WWW-Authenticate";

        /// <summary>
        /// Clickjacking protection: deny - no rendering within a frame, sameorigin - no rendering if origin mismatch, 
        /// allow-from - allow from specified location, allowall - non-standard, allow from any location.
        /// </summary>
        public const string XFrameOptions = "X-Frame-Options";
    }
}
