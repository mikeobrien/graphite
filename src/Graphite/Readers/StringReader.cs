namespace Graphite.Readers
{
    public class StringReader : StringReaderBase
    {
        public override bool AppliesTo(RequestReaderContext context)
        {
            return context.RequestContext.Route.HasRequest && context.RequestContext
                .Route.RequestParameter.ParameterType.Type == typeof(string);
        }

        protected override object GetRequest(string data, RequestReaderContext context)
        {
            return data;
        }
    }
}