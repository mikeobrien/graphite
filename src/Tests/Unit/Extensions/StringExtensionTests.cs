using System.Linq;
using System.Text;
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

        [Test]
        public void Should_create_table()
        {
            1.To(3).Select(x => new
            {
                FirstName = $"Fark{x * 100}",
                LastName = $"Farker{x}"
            }).ToTable(x => new
            {
                First = x.FirstName,
                Last_Name = x.LastName
            }).ShouldEqual(
                "First·· Last Name\r\n" +
                "Fark100 Farker1  \r\n" +
                "Fark200 Farker2··\r\n" +
                "Fark300 Farker3  ");
        }

        [TestCase(null, null)]
        [TestCase("", "")]
        [TestCase("f", "F")]
        [TestCase("fark", "Fark")]
        [TestCase("FARK", "Fark")]
        public void Should_initial_cap(string source, string expected)
        {
            source.InitialCap().ShouldEqual(expected);
        }

        [TestCase(null, "")]
        [TestCase("", "")]
        [TestCase("fark", "ZmFyaw==")]
        public void Should_encode_base64(string text, string expected)
        {
            text.ToBase64(Encoding.UTF8).ShouldEqual(expected);
        }

        [TestCase(null, "")]
        [TestCase("", "")]
        [TestCase("farker", "")]
        [TestCase("ZmFyaw==", "fark")]
        public void Should_decode_base64(string base64, string expected)
        {
            base64.FromBase64(Encoding.UTF8).ShouldEqual(expected);
        }

        [TestCase(null, 5, null)]
        [TestCase("", 5, "")]
        [TestCase("fark", 3, "fark")]
        [TestCase("fark", 4, "fark")]
        [TestCase("fark", 5, "*fark")]
        [TestCase("fark", 6, "*fark*")]
        [TestCase("fark", 7, "**fark*")]
        public void Should_pad_center(string source, int width, string expected)
        {
            source.PadCenter(width, '*').ShouldEqual(expected);
        }

        [TestCase(null, null)]
        [TestCase("fark", "fark")]
        [TestCase("fark,farker", "fark\r\n└farker")]
        [TestCase("fark,farker,mcfarker", "fark\r\n└farker\r\n └mcfarker")]
        public void Should_create_hierarchy(string hierarchy, string expected)
        {
            (hierarchy?.Split(",")).ToHierarchy().ShouldEqual(expected);
        }
    }
}
