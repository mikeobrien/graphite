using System.Collections.Generic;
using System.Web;
using Graphite.Binding;
using Graphite.Extensions;
using Graphite.Http;

namespace Graphite.Readers
{
    public class FormReader : StringReaderBase
    {
        private readonly IEnumerable<IValueMapper> _mappers;
        private readonly Configuration _configuration;

        public FormReader(IEnumerable<IValueMapper> mappers, Configuration configuration) 
            : base(MimeTypes.ApplicationFormUrlEncoded)
        {
            _mappers = mappers;
            _configuration = configuration;
        }

        public override bool AppliesTo(RequestReaderContext context)
        {
            return context.RequestContext.Route.HasRequest && base.AppliesTo(context);
        }

        protected override object GetRequest(string data, 
            RequestReaderContext context)
        {
            return context.RequestContext.Route.RequestParameter.ParameterType
                .CreateAndBind(HttpUtility.ParseQueryString(data).ToObjectLookup(),
                    (p, v) => _mappers.Map(v, p, context.RequestContext, _configuration));
        }
    }
}