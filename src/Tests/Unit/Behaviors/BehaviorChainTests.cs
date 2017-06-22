﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite;
using Graphite.Actions;
using Graphite.Behaviors;
using Graphite.DependencyInjection;
using Graphite.Extensions;
using Graphite.Reflection;
using Graphite.StructureMap;
using NUnit.Framework;
using Should;
using Tests.Common;
using Tests.Common.Fakes;

namespace Tests.Unit.Behaviors
{
    [TestFixture]
    public class BehaviorChainTests
    {
        private Logger _logger;
        private Configuration _configuration;
        private List<TypeDescriptor> _behaviors;
        private ActionDescriptor _actionDescriptor;
        private IContainer _container;

        [SetUp]
        public void Setup()
        {
            _logger = new Logger();
            _configuration = new Configuration();
            _behaviors = new List<TypeDescriptor>();
            _actionDescriptor = new ActionDescriptor(null, null, _behaviors);
            _container = new Container();
            _container.Register(_logger);
        }

        [Test]
        public async Task Should_invoke_behaviors_in_order()
        {
            _configuration.DefaultBehavior = typeof(TestDefaultBehavior);
            _behaviors.Add(typeof(TestBehavior1).ToTypeDescriptor());
            _behaviors.Add(typeof(TestBehavior2).ToTypeDescriptor());

            var behaviorChain = new BehaviorChain(_configuration, _actionDescriptor, _container);

            var result = await behaviorChain.InvokeNext();

            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Accepted);

            _logger.ShouldOnlyContain(
                typeof(TestBehavior1), 
                typeof(TestBehavior2), 
                typeof(TestDefaultBehavior));
        }

        [Test]
        public async Task Should_not_invoke_behaviors_that_dont_appy()
        {
            _configuration.DefaultBehavior = typeof(TestDefaultBehavior);
            _behaviors.Add(typeof(TestBehavior1).ToTypeDescriptor());
            _behaviors.Add(typeof(TestDoesentApplyBehavior).ToTypeDescriptor());
            _behaviors.Add(typeof(TestBehavior2).ToTypeDescriptor());

            var behaviorChain = new BehaviorChain(_configuration, _actionDescriptor, _container);

            var result = await behaviorChain.InvokeNext();

            result.ShouldNotBeNull();
            result.StatusCode.ShouldEqual(HttpStatusCode.Accepted);

            _logger.ShouldOnlyContain(
                typeof(TestBehavior1),
                typeof(TestBehavior2),
                typeof(TestDefaultBehavior));
        }

        public class TestDefaultBehavior : TestLoggingBehavior
        {
            public TestDefaultBehavior(Logger logger) : base(logger) { }

            public override async Task<HttpResponseMessage> Invoke()
            {
                base.Invoke();
                return new HttpResponseMessage(HttpStatusCode.Accepted);
            }
        }

        public abstract class TestBehaviorBase : TestLoggingBehavior
        {
            private readonly IBehaviorChain _behaviorChain;

            protected TestBehaviorBase(IBehaviorChain behaviorChain, Logger logger) : base(logger)
            {
                _behaviorChain = behaviorChain;
            }

            public override async Task<HttpResponseMessage> Invoke()
            {
                base.Invoke();
                return await _behaviorChain.InvokeNext();
            }
        }

        public class TestBehavior1 : TestBehaviorBase
        {
            public TestBehavior1(IBehaviorChain behaviorChain, Logger logger) : 
                base(behaviorChain, logger) { }
        }

        public class TestBehavior2 : TestBehaviorBase
        {
            public TestBehavior2(IBehaviorChain behaviorChain, Logger logger) : 
                base(behaviorChain, logger) { }
        }

        public class TestDoesentApplyBehavior : BehaviorBase
        {
            public TestDoesentApplyBehavior(IBehaviorChain behaviorChain) : base(behaviorChain) { }

            public override bool ShouldRun()
            {
                return false;
            }

            public override Task<HttpResponseMessage> Invoke()
            {
                throw new NotImplementedException();
            }
        }
    }
}