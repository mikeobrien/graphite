using System.Linq;
using Graphite;
using Graphite.Extensions;
using NUnit.Framework;
using Should;

namespace Tests.Unit
{
    [TestFixture]
    public class DefaultConfigurationTests
    {
        [TestCase("", false)]
        [TestCase("Fark", false)]
        [TestCase("Handler", true)]
        [TestCase("FarkHandler", true)]
        public void Should_match_handler_name(
            string name, bool matches)
        {
            name.IsMatch(new Configuration().HandlerNameFilterRegex).ShouldEqual(matches);
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
        public void Should_match_method_name(
            string name, bool matches)
        {
            var configuration = new Configuration();
            name.IsMatch(configuration.ActionRegex(configuration))
                .ShouldEqual(matches);
        }

        [TestCase("", null)]
        [TestCase("Fark", null)]
        [TestCase("GetFark", "Get")]
        [TestCase("Get", "Get")]
        [TestCase("Post", "Post")]
        [TestCase("Put", "Put")]
        [TestCase("Delete", "Delete")]
        [TestCase("Options", "Options")]
        [TestCase("Head", "Head")]
        [TestCase("Trace", "Trace")]
        [TestCase("Connect", "Connect")]
        public void Should_return_method_name(
            string name, string expected)
        {
            var configuration = new Configuration();
            name.MatchGroups(configuration.ActionRegex(configuration))
                .FirstOrDefault().ShouldEqual(expected);
        }
    }
}
