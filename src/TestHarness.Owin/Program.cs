using System;
using Graphite.Owin;
using Microsoft.Owin.Hosting;
using Owin;

namespace TestHarness.Owin
{
    class Program
    {
        static void Main(string[] args)
        {
            WebApp.Start<Startup>(args[0]);

            Console.WriteLine($"Server running at {args[0]}, press enter to exit.");
            Console.ReadLine();
        }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder appBuilder)
        {
            appBuilder.InitializeGraphite(x =>
            {
                Bootstrap.Configure(x);
                x.IncludeTypeAssembly<Startup>();
            });

            // Manually setup Web Api
            //var httpConfiguration = new HttpConfiguration();
            //httpConfiguration.Routes.MapHttpRoute(...);
            //appBuilder.UseWebApi(httpConfiguration);
            //appBuilder.InitializeGraphite(httpConfiguration);
        }
    }
}
