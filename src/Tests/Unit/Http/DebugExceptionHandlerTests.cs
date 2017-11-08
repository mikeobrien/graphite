using System;
using System.Net.Http;
using System.Web.Http.ExceptionHandling;
using Graphite;
using Graphite.Http;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Http
{
    [TestFixture]
    public class DebugExceptionHandlerTests
    {
        [Test]
        public void Should_only_return_error_when_diagnostics_are_enabled(
            [Values(true, false)] bool diagnostics)
        {
            var context = new ExceptionHandlerContext(new ExceptionContext(new Exception(), 
                new ExceptionContextCatchBlock("fark", true, true), new HttpRequestMessage()));
            var defaultResult = context.Result;

            new DebugExceptionHandler(new Configuration { Diagnostics = diagnostics })
                .Handle(context);

            (context.Result == defaultResult).ShouldEqual(!diagnostics);
        }
    }
}
