using System.Collections.Specialized;
using System.Net.Http;
using System.Web;
using Graphite.AspNet;
using NSubstitute;
using NUnit.Framework;
using Should;

namespace Tests.Unit.AspNet
{
    [TestFixture]
    public class RequestPropertyProviderTests
    {
        [Test]
        public void Should_return_request_properties()
        {
            var httpContext = Substitute.For<HttpContextBase>();
            var requestMessage = new HttpRequestMessage();
            var provider = new AspNetRequestPropertyProvider(requestMessage);

            requestMessage.Properties[AspNetRequestPropertyProvider.HttpContextKey] = httpContext;

            var serverVariables = new NameValueCollection
            {
                ["REMOTE_ADDR"] = "192.168.1.1",
                ["REMOTE_ADDR"] = "192.168.1.2",
                ["REMOTE_PORT"] = "80"
            };

            httpContext.Request.ServerVariables.Returns(serverVariables);

            var properties = provider.GetProperties();

            properties.Count.ShouldEqual(2);

            properties.ContainsKey("remoteaddress").ShouldBeTrue();
            properties["remoteaddress"].ShouldEqual("192.168.1.2");

            properties.ContainsKey("remoteport").ShouldBeTrue();
            properties["remoteport"].ShouldEqual("80");
        }

        [Test]
        public void Should_not_fail_if_context_not_found()
        {
            var requestMessage = new HttpRequestMessage();
            var provider = new AspNetRequestPropertyProvider(requestMessage);

            provider.GetProperties().ShouldBeNull();
        }

        [Test]
        public void Should_not_fail_if_server_variables_are_null()
        {
            var httpContext = Substitute.For<HttpContextBase>();
            var requestMessage = new HttpRequestMessage();
            var provider = new AspNetRequestPropertyProvider(requestMessage);

            requestMessage.Properties[AspNetRequestPropertyProvider.HttpContextKey] = httpContext;

            httpContext.Request.ServerVariables.Returns((object)null);

            provider.GetProperties().ShouldBeNull();
        }
    }
}
