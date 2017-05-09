using Graphite.Extensions;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Extensions
{
    [TestFixture]
    public class MemoizeTests
    {
        [Test]
        public void Should_memoize()
        {
            var memoized = Memoize.Func<string, object>(x => new object());

            memoized("a").ShouldEqual(memoized("a"));
            memoized("a").ShouldNotEqual(memoized("b"));
        }
    }
}
