using System.Net;
using System.Reflection;
using Graphite.Reflection;
using NUnit.Framework;
using Should;
using TestHarness.Host;
using Tests.Common;

namespace Tests.Acceptance
{
    [TestFixture]
    public class HostTests
    {
        private const string BaseUrl = "Host/";

        [TestCase(Host.IISExpress, @"\src\TestHarness.AspNet\")]
        [TestCase(Host.Owin, @"\src\TestHarness.Owin\bin\{config}\")]
        public void Should_return_application_paths(Host host, string path)
        {
            path = path.Replace("{config}", Assembly.GetExecutingAssembly()
                .IsInDebugMode() ? "debug" : "release");
            var result = Http.ForHost(host).GetJson<HostTestHandler.PathsModel>(
                $"{BaseUrl}Paths/~/fark/farker.txt");

            result.Status.ShouldEqual(HttpStatusCode.OK);
            result.Data.ApplicationPath.ShouldEndWith(path);
            result.Data.MapPath.ShouldEndWith(path + @"fark\farker.txt");
        }
    }
}
