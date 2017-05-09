using System;
using System.Collections.Generic;
using Graphite.Actions;

namespace Graphite.Routing
{
    public class LambdaUrlConvention : IUrlConvention
    {
        private readonly Func<ActionMethod, string[], string[]> _getUrls;
        private readonly Func<ActionMethod, bool> _appliesTo;

        public LambdaUrlConvention(
            Func<ActionMethod, string[], string[]> getUrls, 
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
            return _getUrls(context.ActionMethod, context.UrlSegments);
        }

    }
}