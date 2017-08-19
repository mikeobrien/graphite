using System;
using System.Collections.Specialized;
using Graphite.Linq;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Linq
{
    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void Should_convert_name_value_collection_to_string_lookup()
        {
            var lookup = new NameValueCollection
            {
                { "key1", "value1" },
                { "key1", "value2" },
                { "key2", "value3" }
            }.ToLookup();

            lookup.Count.ShouldEqual(2);

            lookup["key1"].ShouldOnlyContain("value1", "value2");
            lookup["key2"].ShouldOnlyContain("value3");
        }

        [Test]
        public void Should_convert_name_value_collection_to_object_lookup()
        {
            var lookup = new NameValueCollection
            {
                { "key1", "value1" },
                { "key1", "value2" },
                { "key2", "value3" }
            }.ToLookup();

            lookup.Count.ShouldEqual(2);

            lookup["key1"].ShouldOnlyContain("value1", "value2");
            lookup["key2"].ShouldOnlyContain("value3");
        }

        [Test]
        public void Should_enumerate_nested_objects()
        {
            var innerInstance = new Exception();
            var instance = new Exception("fark", innerInstance);

            instance.Enumerate(x => x.InnerException)
                .ShouldOnlyContain(instance, innerInstance);
        }
    }
}
