using System.Collections.Generic;
using System.Linq;
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

        public ReaderBinder(ActionDescriptor actionDescriptor,
            IEnumerable<IRequestReader> readers)
        {
            _actionDescriptor = actionDescriptor;
            _readers = readers;
        }

        public virtual bool AppliesTo(RequestBinderContext context)
        {
            return _actionDescriptor.Route.HasRequest;
        }

        public virtual async Task Bind(RequestBinderContext context)
        {
            var position = _actionDescriptor.Route.RequestParameter.Position;
            var reader = _actionDescriptor.RequestReaders
                .ThatApplyOrDefault(_readers).FirstOrDefault();
            if (reader != null) context.ActionArguments[position] = await reader.Read();
        }
    }
}