using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using Graphite.Reflection;
using IISExpressBootstrapper;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Tests.Common;

[assembly: KillServers]

namespace Tests.Common
{
    public enum Host { Owin, IISExpress }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class KillServersAttribute : Attribute, ITestAction
    {
        public void BeforeTest(ITest test) { }

        public void AfterTest(ITest test)
        {
            WebServers.KillHosts();
        }

        public ActionTargets Targets => ActionTargets.Suite;
    }

    public static class WebServers
    {
        private const int IISExpressPort = 3091;
        private const int OwinPort = 3092;
        
        private static readonly HttpClient IISExpressHttpClient =
            CreateHttpClient(IISExpressPort);
        private static readonly HttpClient OwinHttpClient =
            CreateHttpClient(OwinPort);

        private static IISExpressHost _iisExpress;
        private static Process _owin;

        public static HttpClient EnsureHost(Host host)
        {
            switch (host)
            {
                case Host.Owin: return EnsureOwin();
                default: return EnsureIISExpress();
            }
        }

        public static void KillHosts()
        {
            _iisExpress?.Dispose();
            _owin?.Kill();
        }

        private static HttpClient EnsureIISExpress()
        {
            if (_iisExpress != null) return IISExpressHttpClient;

            _iisExpress = new IISExpressHost("TestHarness.AspNet", IISExpressPort);

            return IISExpressHttpClient;
        }

        private static HttpClient EnsureOwin()
        {
            if (_owin != null) return OwinHttpClient;

            var serverPath = Path.GetFullPath(Path.Combine(
                TestContext.CurrentContext.TestDirectory, @"..\..\..\TestHarness.Owin\bin",
                Assembly.GetExecutingAssembly().IsInDebugMode() ? "debug" : "release",
                "TestHarness.Owin.exe"));
            _owin = Process.Start(serverPath, $"http://localhost:{OwinPort}");
            Thread.Sleep(3000);
            return OwinHttpClient;
        }

        private static HttpClient CreateHttpClient(int port)
        {
            return new HttpClient(new HttpClientHandler
            {
                UseCookies = false,
                AllowAutoRedirect = false
            })
            {
                // Fiddler can't hook into localhost so when its running 
                // you can use localhost.fiddler
                BaseAddress = new Uri($@"http://{(
                    Process.GetProcessesByName("Fiddler").Any() ?
                        "localhost.fiddler" : "localhost")}:{port}/")
            };
        }
    }
}