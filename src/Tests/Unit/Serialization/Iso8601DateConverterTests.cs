using System;
using Graphite.Serialization;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Serialization
{
    [TestFixture]
    public class MicrosoftDateTimeFormetConverterTests
    {
        public class DateTimeModel
        {
            [JsonConverter(typeof(Iso8601DateConverter))]
            public DateTime DateTime { get; set; }
            [JsonConverter(typeof(Iso8601DateConverter))]
            public DateTime? NullableDateTime { get; set; }
        }

        [Test]
        public void Should_serialize_iso8601_date()
        {
            var datetime = new DateTime(1985, 10, 26, 5, 21, 0);

            var result = JsonConvert.SerializeObject(new DateTimeModel
            {
                DateTime = datetime,
                NullableDateTime = datetime
            });

            result.ShouldEqual(
                "{" +
                    "\"DateTime\":\"1985-10-26\"," +
                    "\"NullableDateTime\":\"1985-10-26\"" +
                "}");
        }

        [Test]
        public void Should_serialize_null_iso8601_date()
        {
            var settings = new JsonSerializerSettings();
            var datetime = new DateTime(1977, 3, 14, 9, 10, 11, DateTimeKind.Utc);
            
            var result = JsonConvert.SerializeObject(new DateTimeModel
            {
                DateTime = datetime
            }, settings);

            result.ShouldEqual(
                "{" +
                    $"\"DateTime\":\"1977-03-14\"," +
                    "\"NullableDateTime\":null" +
                "}");
        }
        
        [TestCase("1977-03-14T22:50:24-23:00", "3/15/1977")]
        [TestCase("1977-03-14T01:50:24Z", "3/14/1977")]
        [TestCase("03\\/14\\/1977 5:21:00 AM", "3/14/1977")]
        public void Should_deserialize_iso8601_date(string value, string expected)
        {
            var datetime = DateTime.Parse(expected).Date;

            var result = JsonConvert.DeserializeObject<DateTimeModel>(
                "{" +
                    $"\"DateTime\":\"{value}\"," +
                    $"\"NullableDateTime\":\"{value}\"" +
                "}");

            result.DateTime.Kind.ShouldEqual(DateTimeKind.Local);
            result.DateTime.ShouldBeWithinSeconds(datetime);
            result.NullableDateTime.ShouldBeWithinSeconds(datetime);
        }

        [Test]
        public void Should_deserialize_null_iso8601_date()
        {
            var result = JsonConvert.DeserializeObject<DateTimeModel>(
                "{" +
                    "\"NullableDateTime\":null" +
                "}");
            
            result.NullableDateTime.ShouldBeNull();
        }
    }
}
