using System;
using Graphite.Routing;

namespace Tests.Common.Fakes
{
    public class TestUrlConvention : IUrlConvention
    {
        public Func<UrlContext, bool> AppliesToFunc { get; set; }
        public Func<UrlContext, string[]> GetUrlsFunc { get; set; }
        public UrlContext AppliesToContext { get; set; }
        public UrlContext GetUrlsContext { get; set; }
        public bool AppliesToCalled { get; set; }
        public bool GetUrlsCalled { get; set; }

        public bool AppliesTo(UrlContext context)
        {
            AppliesToContext = context;
            AppliesToCalled = true;
            return AppliesToFunc?.Invoke(context) ?? true;
        }

        public string[] GetUrls(UrlContext context)
        {
            GetUrlsContext = context;
            GetUrlsCalled = true;
            return GetUrlsFunc(context);
        }
    }
}
