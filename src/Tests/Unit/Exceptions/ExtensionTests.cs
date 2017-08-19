using System;
using Graphite.Exceptions;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Exceptions
{
    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void Should_friendly_format_exception()
        {
            Exception exception = null;
            try
            {
                ExceptionLevel1();
            }
            catch (Exception e)
            {
                exception = e;
            }

            var message = exception.ToFriendlyException();

            Console.WriteLine(message);

            message.ShouldContain(exception.GetType().FullName);
            message.ShouldContain(exception.Message);
            message.ShouldContain(exception.InnerException.Message);
            message.ShouldContain(exception.InnerException.InnerException.Message);
        }

        private void ExceptionLevel1()
        {
            try
            {
                ExceptionLevel2();
            }
            catch (Exception e)
            {
                throw new Exception("level 1", e);
            }
        }

        private void ExceptionLevel2()
        {
            try
            {
                ExceptionLevel3();
            }
            catch (Exception e)
            {
                throw new Exception("level 2", e);
            }
        }
        private void ExceptionLevel3() => throw new Exception("level 3");
    }
}
