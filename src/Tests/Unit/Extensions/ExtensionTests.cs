using NUnit.Framework;
using Graphite.Extensions;
using Should;

namespace Tests.Unit.Extensions
{
    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void Should_generate_hash_code_based_on_values()
        {
            new object().GetHashCode(1, "a")
                .ShouldEqual(new object().GetHashCode(1, "a"));

            new object().GetHashCode(1, "a")
                .ShouldNotEqual(new object().GetHashCode(1, "b"));
        }
    }
}
