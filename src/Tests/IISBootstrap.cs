using IISExpressBootstrapper;
using NUnit.Framework;
using Tests.Common;

namespace Tests
{
    [SetUpFixture]
    public class IISBootstrap
    {
        private IISExpressHost _host;

        [OneTimeSetUp]
        public void StartIIS()
        {
            _host = new IISExpressHost("TestHarness", Http.Port);
        }

        [OneTimeTearDown]
        public void StopIIS()
        {
            _host.Dispose();
        }
    }
}
