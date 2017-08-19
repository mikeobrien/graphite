using System;
using System.Data.SqlClient;
using Graphite.DependencyInjection;
using Graphite.Reflection;
using NUnit.Framework;
using Should;

namespace Tests.Unit.DependencyInjection
{
    [TestFixture]
    public class RegistryTests
    {
        [Test]
        public void Should_generate_friendly_string_displaying_registrations()
        {
            var result = new Registry(new TypeCache())
                .Register<IDisposable>(typeof(SqlConnection))
                .Register(new object())
                .ToString();

            result.ShouldEqual(
                "Plugin Type······· Plugin Assembly· Singleton Instance Concrete Type······················ Concrete Assembly··\r\n" +
                "System.IDisposable mscorlib 4.0.0.0 False              System.Data.SqlClient.SqlConnection System.Data 4.0.0.0\r\n" +
                "object············ mscorlib 4.0.0.0 False···· object·· object····························· mscorlib 4.0.0.0···");
        }
    }
}
