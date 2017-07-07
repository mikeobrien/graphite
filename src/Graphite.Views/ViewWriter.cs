using System.Net.Http;
using Graphite.Writers;

namespace Graphite.Views
{
    public class ViewWriter : StringWriterBase
    {
        private readonly ViewContent _viewContent;

        public ViewWriter(ViewContent viewContent, 
            HttpRequestMessage requestMessage, 
            HttpResponseMessage responseMessage) : 
            base(requestMessage, responseMessage, 
                viewContent.Encoding, viewContent.AcceptTypes)
        {
            _viewContent = viewContent;
        }
        
        protected override string GetResponse(ResponseWriterContext context)
        {
            return _viewContent.Render(context.Response);
        }
    }
}