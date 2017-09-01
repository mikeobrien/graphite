using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Readers;

namespace Graphite.Binding
{
    public class ReaderBinder : IRequestBinder
    {
        private readonly ActionDescriptor _actionDescriptor;
        private readonly IEnumerable<IRequestReader> _readers;
        private readonly HttpRequestMessage _requestMessage;
        private readonly Configuration _configuration;

        public ReaderBinder(ActionDescriptor actionDescriptor,
            IEnumerable<IRequestReader> readers,
            HttpRequestMessage requestMessage,
            Configuration configuration)
        {
            _actionDescriptor = actionDescriptor;
            _readers = readers;
            _requestMessage = requestMessage;
            _configuration = configuration;
        }

        public virtual bool AppliesTo(RequestBinderContext context)
        {
            return _actionDescriptor.Route.HasRequest;
        }
        
        public virtual async Task<BindResult> Bind(RequestBinderContext context)
        {

            var position = _actionDescriptor.Route.RequestParameter.Position;

            if ((_requestMessage.Content?.Headers.ContentLength ?? 0) == 0)
            {
                if (_configuration.CreateEmptyRequestParameterValue)
                    context.ActionArguments[position] = _actionDescriptor
                        .Route.RequestParameter.ParameterType.TryCreate();
                return BindResult.Success();
            }

            var reader = _actionDescriptor.RequestReaders
                .ThatApplyOrDefault(_readers).FirstOrDefault();
            if (reader == null) return BindResult.NoReader();
            var result = await reader.Read();

            if (result.Status == ReadStatus.Failure)
                return BindResult.Failure(result.ErrorMessage);

            context.ActionArguments[position] = result.Value;
            return BindResult.Success();
        }
    }
}