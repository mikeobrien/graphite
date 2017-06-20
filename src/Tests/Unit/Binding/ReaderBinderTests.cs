using System.Linq;
using System.Threading.Tasks;
using Graphite.Binding;
using Graphite.Extensions;
using NUnit.Framework;
using Should;
using Tests.Common;
using Tests.Common.Fakes;

namespace Tests.Unit.Binding
{
    [TestFixture]
    public class ReaderBinderTests
    {
        public class Handler
        {
            public void Get(string request) { }
        }

        [Test]
        public async Task Should_bind_reader_result_with_the_first_matching_reader()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Get(null))
                .WithRequestParameter("request")
                .AddRequestReader1(() => "reader1".ToTaskResult<object>())
                .AddRequestReader2(() => "reader2".ToTaskResult<object>());
            var requestReaderContext = requestGraph.GetRequestBinderContext();

            await CreateBinder(requestGraph).Bind(requestReaderContext);

            requestGraph.ActionArguments.ShouldOnlyContain("reader1");

            requestGraph.RequestReader1.AppliesCalled.ShouldBeTrue();
            requestGraph.RequestReader1.ReadCalled.ShouldBeTrue();

            requestGraph.RequestReader2.AppliesCalled.ShouldBeTrue();
            requestGraph.RequestReader2.ReadCalled.ShouldBeFalse();
        }

        [Test]
        public void Should_not_bind_reader_if_route_does_not_have_a_request()
        {
            var requestGraph = RequestGraph.CreateFor<Handler>(h => h.Get(null));

            CreateBinder(requestGraph).AppliesTo(requestGraph
                .GetRequestBinderContext()).ShouldBeFalse();
        }

        [Test]
        public async Task Should_not_bind_with_reader_that_does_not_apply_in_configuration()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Get(null))
                .WithRequestParameter("request")
                .AddRequestReader1(() => "reader1".ToTaskResult<object>(),
                    configAppliesTo: x => false)
                .AddRequestReader2(() => "reader2".ToTaskResult<object>())
                .AddValueMapper1(x => x.Values.First());

            await CreateBinder(requestGraph).Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain("reader2");

            requestGraph.RequestReader1.AppliesCalled.ShouldBeFalse();
            requestGraph.RequestReader1.ReadCalled.ShouldBeFalse();

            requestGraph.RequestReader2.AppliesCalled.ShouldBeTrue();
            requestGraph.RequestReader2.ReadCalled.ShouldBeTrue();
        }

        [Test]
        public async Task Should_not_bind_with_reader_instance_that_does_not_apply()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Get(null))
                .WithRequestParameter("request")
                .AddRequestReader1(() => "reader1".ToTaskResult<object>(),
                    instanceAppliesTo: () => false)
                .AddRequestReader2(() => "reader2".ToTaskResult<object>())
                .AddValueMapper1(x => x.Values.First());

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain("reader2");

            requestGraph.RequestReader1.AppliesCalled.ShouldBeTrue();
            requestGraph.RequestReader1.ReadCalled.ShouldBeFalse();

            requestGraph.RequestReader2.AppliesCalled.ShouldBeTrue();
            requestGraph.RequestReader2.ReadCalled.ShouldBeTrue();
        }

        [Test]
        public async Task Should_bind_default_reader_if_configured_and_no_reader_applies()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Get(null))
                .WithRequestParameter("request")
                .AddRequestReader1(() => "reader1".ToTaskResult<object>(),
                    instanceAppliesTo: () => false)
                .AddRequestReader2(() => "reader2".ToTaskResult<object>(),
                    instanceAppliesTo: () => false)
                .AddValueMapper1(x => x.Values.First());
            requestGraph.Configuration.RequestReaders.DefaultIs<TestRequestReader2>();

            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain("reader2");

            requestGraph.RequestReader1.AppliesCalled.ShouldBeTrue();
            requestGraph.RequestReader1.ReadCalled.ShouldBeFalse();

            requestGraph.RequestReader2.AppliesCalled.ShouldBeTrue();
            requestGraph.RequestReader2.ReadCalled.ShouldBeTrue();
        }

        [Test]
        public async Task Should_not_bind_if_no_reader_applies()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Get(null))
                .WithRequestParameter("request")
                .AddValueMapper1(x => $"{x.Values.First()}mapper");
            var binder = CreateBinder(requestGraph);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null);

            requestGraph.ValueMapper1.MapCalled.ShouldBeFalse();
        }

        public ReaderBinder CreateBinder(RequestGraph requestGraph)
        {
            return new ReaderBinder(requestGraph.GetActionConfigurationContext(),
                requestGraph.GetRouteDescriptor(), requestGraph.RequestReaders);
        }
    }
}
