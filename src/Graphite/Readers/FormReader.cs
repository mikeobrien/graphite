using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Routing;

namespace Graphite.Readers
{
    public class FormReader : StringReaderBase
    {
        private readonly IEnumerable<IValueMapper> _mappers;

        public FormReader(IEnumerable<IValueMapper> mappers) 
            : base(MimeTypes.ApplicationFormUrlEncoded)
        {
            _mappers = mappers;
        }

        public override bool AppliesTo(RequestReaderContext context)
        {
            return context.RequestContext.Route.HasRequest && base.AppliesTo(context);
        }

        protected override object GetRequest(string data, RequestReaderContext context)
        {
            var requestParameter = context.RequestContext.Route.RequestParameter;
            var parameterType = requestParameter.ParameterType;
            var instance = parameterType.TryCreate();

            if (instance == null) return null;

            HttpUtility.ParseQueryString(data).ToLookup()
                .Where(x => x.Any())
                .JoinIgnoreCase(parameterType.Properties, x => x.Key, x => x.Name, 
                    (param, prop) => new
                    {
                        Parameter = new ActionParameter(requestParameter, prop),
                        Values = param.ToArray()
                    })
                .ForEach(x =>
                {
                    var result = _mappers.Map(x.Values, x.Parameter,
                        context.RequestContext, context.Configuration);

                    if (result.Mapped) x.Parameter.BindProperty(instance, result.Value);
                });

            return instance;
        }
    }
}