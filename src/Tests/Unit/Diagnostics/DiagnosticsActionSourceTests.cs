using System.Linq;
using Graphite;
using Graphite.Actions;
using Graphite.Diagnostics;
using Graphite.Reflection;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Diagnostics
{
    [TestFixture]
    public class DiagnosticsActionSourceTests
    {
        [Test]
        public void Should_enable_diagnostics_from_configuration(
            [Values(true, false)] bool enabled)
        {
            var configuration = new Configuration
            {
                EnableDiagnostics = enabled
            };

            new DiagnosticsActionSource(configuration, null)
                .AppliesTo(null).ShouldEqual(enabled);
        }

        [Test]
        public void Should_return_diagnostics_handler()
        {
            var diagnosticsHandler = typeof(DiagnosticsHandler);
            var configuration = new Configuration();
            var actionSource = new DiagnosticsActionSource(
                configuration, new TypeCache());

            var actions = actionSource.GetActions(new ActionSourceContext(null, null));

            actions.Count.ShouldEqual(1);

            var action = actions.First();

            action.Route.Url.ShouldEqual(configuration.DiagnosticsUrl);
            action.Action.HandlerType.Type.ShouldEqual(diagnosticsHandler);
            action.Action.Method.MethodInfo.ShouldEqual(diagnosticsHandler
                .GetMethod(nameof(DiagnosticsHandler.Get)));
        }
    }
}
