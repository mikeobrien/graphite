using System.Text.RegularExpressions;
using Graphite;
using Graphite.Actions;
using Graphite.Extensions;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit
{
    [TestFixture]
    public class DefaultConfigurationTests
    {
        private Configuration _configuration;

        [SetUp]
        public void Setup()
        {
            _configuration = new Configuration();
        }

        [Test]
        public void Should_match_handler_namespace_regex()
        {
            "fark".MatchGroupValue(_configuration.HandlerNamespaceConvention,
                Configuration.HandlerNamespaceGroupName)
                .ShouldEqual("fark");
        }

        public class ParseNamespace
        {
            public void Post() { }
        }
        
        [Test]
        public void Should_parse_default_handler_namespace()
        {
            _configuration.HandlerNamespaceParser(_configuration, 
                ActionMethod.From<ParseNamespace>(x => x.Post()))
                .ShouldEqual("Tests.Unit");
        }

        [Test]
        public void Should_parse_custom_handler_namespace()
        {
            _configuration.HandlerNamespaceConvention = new Regex(@"Tests\." + 
                Configuration.DefaultHandlerNamespaceConventionRegex);
            _configuration.HandlerNamespaceParser(_configuration,
                    ActionMethod.From<ParseNamespace>(x => x.Post()))
                .ShouldEqual("Unit");
        }

        [TestCase("", false)]
        [TestCase("Fark", false)]
        [TestCase("Handler", true)]
        [TestCase("FarkHandler", true)]
        public void Should_match_handler_name(string name, bool matches)
        {
            _configuration.HandlerNameConvention.IsMatch(name).ShouldEqual(matches);
        }

        [TestCase("", false)]
        [TestCase("Fark", false)]
        [TestCase("GetFark", true)]
        [TestCase("Get", true)]
        [TestCase("Post", true)]
        [TestCase("Put", true)]
        [TestCase("Delete", true)]
        [TestCase("Options", true)]
        [TestCase("Head", true)]
        [TestCase("Trace", true)]
        [TestCase("Connect", true)]
        public void Should_match_method_name(string name, bool matches)
        {
            _configuration.ActionNameConvention(_configuration).IsMatch(name)
                .ShouldEqual(matches);
        }
        
        [TestCase("", null)]
        [TestCase("GetFark", "Get")]
        [TestCase("Get", "Get")]
        [TestCase("Post", "Post")]
        [TestCase("Put", "Put")]
        [TestCase("Delete", "Delete")]
        [TestCase("Options", "Options")]
        [TestCase("Head", "Head")]
        [TestCase("Trace", "Trace")]
        [TestCase("Connect", "Connect")]
        public void Should_return_method_name(string name, string expected)
        {
            name.MatchGroupValue(_configuration.ActionNameConvention(_configuration),
                    Configuration.HttpMethodGroupName)
                .ShouldEqual(expected);
        }

        public class ParseHttpMethod
        {
            public void GetFark() { }
            public void Get() { }
            public void Post() { }
            public void Put() { }
            public void Delete() { }
            public void Options() { }
            public void Head() { }
            public void Trace() { }
            public void Connect() { }
        }
        
        [TestCase(nameof(ParseHttpMethod.GetFark), "GET")]
        [TestCase(nameof(ParseHttpMethod.Get), "GET")]
        [TestCase(nameof(ParseHttpMethod.Post), "POST")]
        [TestCase(nameof(ParseHttpMethod.Put), "PUT")]
        [TestCase(nameof(ParseHttpMethod.Delete), "DELETE")]
        [TestCase(nameof(ParseHttpMethod.Options), "OPTIONS")]
        [TestCase(nameof(ParseHttpMethod.Head), "HEAD")]
        [TestCase(nameof(ParseHttpMethod.Trace), "TRACE")]
        [TestCase(nameof(ParseHttpMethod.Connect), "CONNECT")]
        public void Should_return_method_name_from_method_name_convention(string name, string expected)
        {
            _configuration.HttpMethodConvention(_configuration,
                    ActionMethod.From<ParseHttpMethod>(name))
                .ShouldEqual(expected);
        }

        public class ParseSegments
        {
            public void Get() { }
            public void GetFark() { }
            public void GetFark_Farker() { }
        }

        [TestCase(nameof(ParseSegments.GetFark), "Fark")]
        [TestCase(nameof(ParseSegments.Get), null)]
        [TestCase(nameof(ParseSegments.GetFark_Farker), "Fark,Farker")]
        public void Should_return_segments_from_segment_convention(
            string name, string expected)
        {
            var result = _configuration.ActionSegmentsConvention(_configuration,
                ActionMethod.From<ParseSegments>(name));

            if (expected == null) result.ShouldBeNull();
            else result.ShouldOnlyContain(expected.Split(','));
        }
    }
}
