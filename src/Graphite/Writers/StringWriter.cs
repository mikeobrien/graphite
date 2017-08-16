using System;
using System.Net.Http;
using System.Text;
using Graphite.Actions;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Writers
{
    public interface IStringOutputInfo : IOutputInfo
    {
        Encoding Encoding { get; }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OutputStringAttribute : OutputInfoAttribute, IStringOutputInfo
    {
        private readonly string _encoding;

        public OutputStringAttribute(string contentType = null,
            string filename = null, string encoding = null) : 
            base(contentType, filename)
        {
            _encoding = encoding;
        }

        public Encoding Encoding => _encoding.IsNotNullOrEmpty() 
            ? Encoding.GetEncoding(_encoding) : null;
    }

    public class OutputString : OutputBody<string>, IStringOutputInfo
    {
        public Encoding Encoding { get; set; }
    }

    public class StringWriter : BodyWriterBase<string, 
        OutputString, OutputStringAttribute, IStringOutputInfo>
    {
        private readonly Configuration _configuration;

        public StringWriter(ActionMethod actionMethod, 
            RouteDescriptor routeDescriptor,
            HttpResponseMessage responseMessage,
            Configuration configuration) : 
            base(actionMethod, routeDescriptor, 
                responseMessage, configuration)
        {
            _configuration = configuration;
        }

        protected override HttpContent GetContent(string data, 
            IStringOutputInfo outputInfo)
        {
            return new AsyncStringContent(data, outputInfo?.Encoding ??
                _configuration.DefaultEncoding);
        }

        protected override string GetContentType(string data)
        {
            return data.ContainsIgnoreCase("</") || 
                data.ContainsIgnoreCase("/>") 
                ? MimeTypes.TextHtml 
                : MimeTypes.TextPlain;
        }
    }
}