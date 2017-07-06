using System.Net.Http;
using Graphite.Owin;
using Microsoft.Owin;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Owin
{
    [TestFixture]
    public class RequestPropertyProviderTests
    {
        [Test]
        public void Should_return_request_properties()
        {
            var requestMessage = new HttpRequestMessage();
            var provider = new OwinRequestPropertyProvider(requestMessage);
            var owinContext = new OwinContext
            {
                Request =
                {
                    RemoteIpAddress = "192.168.1.1",
                    RemotePort = 80
                }
            };

            requestMessage.Properties[OwinRequestPropertyProvider.OwinContextKey] = owinContext;

            var properties = provider.GetProperties();

            properties.ContainsKey("remoteaddress").ShouldBeTrue();
            properties["remoteaddress"].ShouldEqual("192.168.1.1");

            properties.ContainsKey("remoteport").ShouldBeTrue();
            properties["remoteport"].ShouldEqual(80);
        }

        [Test]
        public void Should_not_fail_if_endpoint_properties_not_found()
        {
            var requestMessage = new HttpRequestMessage();
            var provider = new OwinRequestPropertyProvider(requestMessage);
  
            provider.GetProperties().ShouldBeNull();
        }
    }
}
