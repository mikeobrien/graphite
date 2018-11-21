using System;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using Graphite.AspNet;

namespace TestHarness.AspNet
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            RouteTable.Routes.RouteExistingFiles = true;

            GlobalConfiguration.Configure(c =>
            {
                c.MapHttpAttributeRoutes();

                c.InitializeGraphite(x =>
                    {
                        Bootstrap.Configure(x);
                        x.IncludeTypeAssembly<Global>();
                    });
            });
        }
    }
}