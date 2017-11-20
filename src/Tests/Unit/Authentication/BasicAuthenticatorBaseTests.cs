using System.Text;
using Graphite.Authentication;
using Graphite.Extensions;
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
                $"{username}:{password}".ToBase64(Encoding.UTF8)).ShouldEqual(expected);
        }
    }

    public class TestBasicAuthenticator : BasicAuthenticatorBase
    {
        private readonly string _username;
        private readonly string _password;

        public TestBasicAuthenticator(string username, string password)
        {
            _username = username;
            _password = password;
        }

        public string RealmOverride { set => Realm = value; }
        public string StatusMessageOverride { set => UnauthorizedReasonPhrase = value; }

        public override bool Authenticate(string username, string password)
        {
            return username == _username && password == _password;
        }
    }
}
