using System;
using System.Collections.Generic;
using Graphite.Routing;

namespace Tests.Common.Fakes
{
    public class TestRouteConvention : IRouteConvention
    {
        public Func<RouteContext, bool> AppliesToPredicate { get; set; }
        public Func<RouteContext, List<RouteDescriptor>> GetDescriptors { get; set; }
        public RouteContext AppliesToContext { get; set; }
        public RouteContext GetRouteDescriptorsContext { get; set; }
        public bool AppliesToCalled { get; set; }
        public bool GetRouteDescriptorsCalled { get; set; }

        public bool AppliesTo(RouteContext context)
        {
            AppliesToCalled = true;
            AppliesToContext = context;
            return AppliesToPredicate?.Invoke(context) ?? true;
        }

        public List<RouteDescriptor> GetRouteDescriptors(RouteContext context)
        {
            GetRouteDescriptorsCalled = true;
            GetRouteDescriptorsContext = context;
            return GetDescriptors(context);
        }
    }
}