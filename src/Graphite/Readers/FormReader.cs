using System;
using System.Linq;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Readers
{
    public class FormReader : StringReaderBase
    {
        private readonly ParameterBinder<ReadResult> _parameterBinder;
        private readonly ActionMethod _actionMethod;

        public FormReader(
            ParameterBinder<ReadResult> parameterBinder,
            ActionMethod actionMethod) 
            : base(MimeTypes.ApplicationFormUrlEncoded)
        {
            _parameterBinder = parameterBinder;
            _actionMethod = actionMethod;
        }

        protected override ReadResult GetRequest(string data, ReaderContext context)
        {
            var instance = context.ReadType.TryCreate();

            if (instance == null)
                throw new RequestTypeCreationException(
                    context.ReadType, _actionMethod);

            var actionParameters = context.ReadType.Properties
                .Select(x => new ActionParameter(_actionMethod, x));
            var values = data.ParseQueryString();

            return _parameterBinder.Bind(values, actionParameters,
                (p, v) => p.BindProperty(instance, v),
                () => ReadResult.Success(instance),
                ReadResult.Failure);
        }
    }
}