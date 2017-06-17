using NUnit.Framework;
using Should;

namespace Tests.Unit.Authentication
{
    [TestFixture]
    public class BasicAuthenticatorBaseTests
    {

        [TestCase(null, null, false)]
        [TestCase("", "", false)]
        [TestCase("fark", "", false)]
        [TestCase("", "farker", false)]
        [TestCase("fark", "farker", true)]
        public void Should_authenticate(string username, string password, bool expected)
        {
            var authenticator = new TestBasicAuthenticator("fark", "farker");

            authenticator.Authenticate(password == null ? username : 
                $"{username}:{password}").ShouldEqual(expected);
        }
    }
}
