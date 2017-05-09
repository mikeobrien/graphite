using Graphite.Extensions;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Extensions
{
    [TestFixture]
    public class StringExtensionTests
    {
        [Test]
        public void Should_remove_by_regex()
        {
            "OneTwoOneThreeThree".Remove("^One", "Three").ShouldEqual("TwoOne");
        }

        [TestCase("", "", null)]
        [TestCase("FarkFarker", "^(Fark)", "Fark")]
        [TestCase("Fark", "^(Farker)", null)]
        public void Should_return_matched_groups(
            string source, string regex, string expected)
        {
            source.MatchGroups(regex).ShouldOnlyContain(
                expected?.Split(',') ?? new string[] {});
        }
    }
}
