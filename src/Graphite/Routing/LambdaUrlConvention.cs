using System;
using Graphite.Actions;

namespace Graphite.Routing
{
    public class LambdaUrlConvention : IUrlConvention
    {
        private readonly Func<ActionMethod, Url, string[]> _getUrls;
        private readonly Func<ActionMethod, bool> _appliesTo;

        public LambdaUrlConvention(
            Func<ActionMethod, Url, string[]> getUrls, 
            Func<ActionMethod, bool> appliesTo = null)
        {
            _getUrls = getUrls;
            _appliesTo = appliesTo;
        }

        public virtual bool AppliesTo(UrlContext context)
        {
            return _appliesTo?.Invoke(context.ActionMethod) ?? true;
        }

        public virtual string[] GetUrls(UrlContext context)
        {
            return _getUrls(context.ActionMethod, context.Url);
        }

    }
}