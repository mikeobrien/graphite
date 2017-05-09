using System.Linq;
using System.Threading.Tasks;
using Graphite.Binding;
using Graphite.Extensions;
using NUnit.Framework;
using Should;
using Tests.Common;

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
                .AddRequestReader1(x => "reader1".ToTaskResult<object>())
                .AddRequestReader2(x => "reader2".ToTaskResult<object>());
            var binder = new ReaderBinder(requestGraph.RequestReaders, requestGraph.Configuration);
            var requestReaderContext = requestGraph.GetRequestBinderContext();

            await binder.Bind(requestReaderContext);

            requestGraph.ActionArguments.ShouldOnlyContain("reader1");

            requestGraph.RequestReader1.AppliesToCalled.ShouldBeTrue();
            requestGraph.RequestReader1.AppliesToContext.RequestContext
                .ShouldEqual(requestReaderContext.RequestContext);
            requestGraph.RequestReader1.ReadCalled.ShouldBeTrue();
            requestGraph.RequestReader1.ReadContext.RequestContext
                .ShouldEqual(requestReaderContext.RequestContext);

            requestGraph.RequestReader2.AppliesToCalled.ShouldBeTrue();
            requestGraph.RequestReader2.AppliesToContext.RequestContext
                .ShouldEqual(requestReaderContext.RequestContext);
            requestGraph.RequestReader2.ReadCalled.ShouldBeFalse();
        }

        [Test]
        public void Should_not_bind_reader_if_route_does_not_have_a_request()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Get(null));
            var binder = new ReaderBinder(requestGraph.RequestReaders, requestGraph.Configuration);

            binder.AppliesTo(requestGraph.GetRequestBinderContext()).ShouldBeFalse();
        }

        [Test]
        public async Task Should_not_bind_with_reader_that_does_not_apply_in_configuration()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Get(null))
                .WithRequestParameter("request")
                .AddRequestReader1(x => "reader1".ToTaskResult<object>(),
                    configAppliesTo: x => false)
                .AddRequestReader2(x => "reader2".ToTaskResult<object>())
                .AddValueMapper1(x => x.Values.First());

            var binder = new ReaderBinder(requestGraph.RequestReaders, requestGraph.Configuration);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain("reader2");

            requestGraph.RequestReader1.AppliesToCalled.ShouldBeFalse();
            requestGraph.RequestReader1.ReadCalled.ShouldBeFalse();

            requestGraph.RequestReader2.AppliesToCalled.ShouldBeTrue();
            requestGraph.RequestReader2.ReadCalled.ShouldBeTrue();
        }

        [Test]
        public async Task Should_not_bind_with_reader_instance_that_does_not_apply()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Get(null))
                .WithRequestParameter("request")
                .AddRequestReader1(x => "reader1".ToTaskResult<object>(),
                    instanceAppliesTo: x => false)
                .AddRequestReader2(x => "reader2".ToTaskResult<object>())
                .AddValueMapper1(x => x.Values.First());

            var binder = new ReaderBinder(requestGraph.RequestReaders, requestGraph.Configuration);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain("reader2");

            requestGraph.RequestReader1.AppliesToCalled.ShouldBeTrue();
            requestGraph.RequestReader1.ReadCalled.ShouldBeFalse();

            requestGraph.RequestReader2.AppliesToCalled.ShouldBeTrue();
            requestGraph.RequestReader2.ReadCalled.ShouldBeTrue();
        }

        [Test]
        public async Task Should_not_bind_if_no_reader_found()
        {
            var requestGraph = RequestGraph
                .CreateFor<Handler>(h => h.Get(null))
                .WithRequestParameter("request")
                .AddValueMapper1(x => $"{x.Values.First()}mapper");
            var binder = new ReaderBinder(requestGraph.RequestReaders, requestGraph.Configuration);

            await binder.Bind(requestGraph.GetRequestBinderContext());

            requestGraph.ActionArguments.ShouldOnlyContain(null);

            requestGraph.ValueMapper1.MapCalled.ShouldBeFalse();
        }
    }
}
