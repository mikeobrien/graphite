using System;
using Graphite.Routing;

namespace TestHarness.Routing
{
    namespace SomeNamespace
    {
        public class Handler
        {
            public TestHarness.Handler.OutputModel GetSomeMethod()
            {
                return new TestHarness.Handler.OutputModel { Value = "fark" };
            }
        }
    }

    public class RoutingTestHandler
    {
        public class UrlAliasModel
        {
            public string Value { get; set; }
        }

        [UrlAlias("Fark/Alias")]
        public UrlAliasModel GetNonAlias()
        {
            return new UrlAliasModel { Value = "fark" };
        }

        public string GetWithAmbiguousUrl_Id(Guid id)
        {
            return id.ToString();
        }

        public string GetWithAmbiguousUrl_Segment()
        {
            return "no id";
        }

        public string GetWithAmbiguousTypeUrl_Id(string id)
        {
            return "string:" + id;
        }

        public string GetWithAmbiguousTypeUrl_Id([MatchType] Guid id)
        {
            return "guid:" + id;
        }

        public string GetWithRouteDecorator()
        {
            return "fark";
        }

        public class HttpRouteDecorator : IHttpRouteDecorator
        {
            public bool AppliesTo(HttpRouteDecoratorContext context)
            {
                return true;
            }

            public void Decorate(HttpRouteDecoratorContext route)
            {
                route.Route.MethodConstraints.Add("POST");
            }
        }
    }
}