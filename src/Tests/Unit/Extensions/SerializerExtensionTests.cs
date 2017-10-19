using System;
using Graphite.Extensions;
using Newtonsoft.Json;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Extensions
{
    [TestFixture]
    public class SerializerExtensionTests
    {
        public class DateTimeModel
        {
            public DateTime DateTime { get; set; }
            public DateTime? NullableDateTime { get; set; }
        }

        [Test]
        public void Should_serialize_microsoft_datetime_format()
        {
            var datetime = new DateTime(1985, 10, 26, 5, 21, 0).SubtractUtcOffset();
            var settings = new JsonSerializerSettings();

            settings.WriteMicrosoftJsonDateTime(x => x.AdjustToUtcBeforeSerializing());

            var result = JsonConvert.SerializeObject(new DateTimeModel
            {
                DateTime = datetime,
                NullableDateTime = datetime
            }, settings);

            result.ShouldEqual(
                "{" +
                    "\"DateTime\":\"\\/Date(499152060000)\\/\"," +
                    "\"NullableDateTime\":\"\\/Date(499152060000)\\/\"" +
                "}");
        }

        [Test]
        public void Should_serialize_null_microsoft_datetime_format()
        {
            var settings = new JsonSerializerSettings();

            settings.WriteMicrosoftJsonDateTime(x => x.AdjustToUtcBeforeSerializing());

            var result = JsonConvert.SerializeObject(new DateTimeModel(), settings);

            result.ShouldEqual(
                "{" +
                    "\"DateTime\":\"\\/Date(-62135578800000)\\/\"," +
                    "\"NullableDateTime\":null" +
                "}");
        }

        [TestCase("\\/Date(499152060000)\\/", DateTimeKind.Utc)]
        [TestCase("10\\/26\\/1985 5:21:00 AM", DateTimeKind.Unspecified)]
        public void Should_deserialize_utc_microsoft_datetime_format(
            string value, DateTimeKind kind)
        {
            var datetime = new DateTime(1985, 10, 26, 5, 21, 0);
            var settings = new JsonSerializerSettings();

            settings.WriteMicrosoftJsonDateTime();

            var result = JsonConvert.DeserializeObject<DateTimeModel>(
                "{" +
                    $"\"DateTime\":\"{value}\"," +
                    $"\"NullableDateTime\":\"{value}\"" +
                "}", settings);

            result.DateTime.Kind.ShouldEqual(kind);
            result.DateTime.ShouldBeWithinSeconds(datetime);
            result.NullableDateTime.ShouldBeWithinSeconds(datetime);
        }

        [Test]
        public void Should_deserialize_local_microsoft_datetime_format(
            [Values("\\/Date(499152060000)\\/", "10\\/26\\/1985 5:21:00 AM")] string value)
        {
            var datetime = new DateTime(1985, 10, 26, 5, 21, 0).SubtractUtcOffset();
            var settings = new JsonSerializerSettings();

            settings.WriteMicrosoftJsonDateTime(x => x.AdjustToLocalAfterDeserializing());

            var result = JsonConvert.DeserializeObject<DateTimeModel>(
                "{" +
                    $"\"DateTime\":\"{value}\"," +
                    $"\"NullableDateTime\":\"{value}\"" +
                "}", settings);

            result.DateTime.Kind.ShouldEqual(DateTimeKind.Local);
            result.DateTime.ShouldBeWithinSeconds(datetime);
            result.NullableDateTime.ShouldBeWithinSeconds(datetime);
        }

        [Test]
        public void Should_deserialize_null_microsoft_datetime_format()
        {
            var settings = new JsonSerializerSettings();

            settings.WriteMicrosoftJsonDateTime();

            var result = JsonConvert.DeserializeObject<DateTimeModel>(
                "{" +
                    "\"NullableDateTime\":null" +
                "}", settings);
            
            result.NullableDateTime.ShouldBeNull();
        }
    }
}
