﻿using System.Linq;
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
                Diagnostics = enabled
            };

            new DiagnosticsActionSource(configuration, null, null, null, null)
                .Applies().ShouldEqual(enabled);
        }

        [Test]
        public void Should_return_diagnostics_handler()
        {
            var diagnosticsHandler = typeof(DiagnosticsHandler);
            var configuration = new Configuration();
            var actionSource = new DiagnosticsActionSource(
                configuration, null, new TypeCache(), null, 
                    new ActionDescriptorFactory(configuration, null, new TypeCache()));

            var actions = actionSource.GetActions();

            actions.Count.ShouldEqual(3);

            var action = actions.FirstOrDefault(x => x.Route.Url == configuration.DiagnosticsUrl);

            action.ShouldNotBeNull();
            action.Action.HandlerTypeDescriptor.Type.ShouldEqual(diagnosticsHandler);
            action.Action.MethodDescriptor.MethodInfo.ShouldEqual(diagnosticsHandler
                .GetMethod(nameof(DiagnosticsHandler.Get)));

            actions.Any(x => x.Route.Url == $"{configuration.DiagnosticsUrl}/Favicon").ShouldBeTrue();
            actions.Any(x => x.Route.Url == $"{configuration.DiagnosticsUrl}/Logo").ShouldBeTrue();
        }
    }
}
