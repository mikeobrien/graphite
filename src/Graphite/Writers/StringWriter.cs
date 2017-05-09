using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Graphite.Extensions;
using Graphite.Http;

namespace Graphite.Writers
{
    public class StringWriter : IResponseWriter
    {
        public virtual bool AppliesTo(ResponseWriterContext context)
        {
            return context.RequestContext.Route.HasResponse && context
                .RequestContext.Route.ResponseType.Type == typeof(string);
        }

        public Task<HttpResponseMessage> Write(ResponseWriterContext context)
        {
            var response = new HttpResponseMessage();
            var data = context.Response?.ToString();
            if (data != null)
                response.Content = new StringContent(data,
                    Encoding.UTF8, data.ContainsIgnoreCase("</") ? 
                        MimeTypes.TextHtml : MimeTypes.TextPlain);
            return response.ToTaskResult();
        }
    }
}