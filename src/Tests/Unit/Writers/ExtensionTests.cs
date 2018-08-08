using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite;
using Graphite.Actions;
using Graphite.Reflection;
using Graphite.Writers;
using NUnit.Framework;
using Tests.Common;

namespace Tests.Unit.Writers
{
    [TestFixture]
    public class ExtensionTests
    {
        [Test]
        public void Should_sort_writers_by_weighted_and_weight_and_order()
        {
            var configuration = new Configuration();
            var notWeightedWriter1 = new NotWeightedWriter1();
            var notWeightedWriter2 = new NotWeightedWriter2();
            var notWeightedWriter3 = new NotWeightedWriter3();
            var weightedWriter1 = new WeightedWriter1(.8);
            var weightedWriter2 = new WeightedWriter2(.5);
            var weightedWriter3 = new WeightedWriter3(.8);

            var writers = new List<IResponseWriter>
            {
                weightedWriter3, notWeightedWriter2, weightedWriter1,
                notWeightedWriter3, notWeightedWriter1, weightedWriter2
            };

            configuration.ResponseWriters.Configure(c => c
                .Append<WeightedWriter1>()
                .Append<NotWeightedWriter1>()
                .Append<WeightedWriter2>()
                .Append<NotWeightedWriter2>()
                .Append<WeightedWriter3>()
                .Append<NotWeightedWriter3>());

            var actionDescriptor = new ActionDescriptor(null, null, null, null, null, 
                configuration.ResponseWriters.CloneAllThatApplyTo(null), 
                null, null, null, new TypeCache());

            var results = writers.ThatApply(null, actionDescriptor);

            results.ShouldOnlyContain(
                notWeightedWriter1,
                notWeightedWriter2,
                notWeightedWriter3,
                weightedWriter1,
                weightedWriter3,
                weightedWriter2);
        }

        public class WriterBase : ResponseWriterBase
        {
            public override Task<HttpResponseMessage> 
                WriteResponse(ResponseWriterContext context)
            {
                throw new NotImplementedException();
            }
        }

        public class WeightedWriterBase : WriterBase
        {
            public WeightedWriterBase(double quality)
            {
                Weight = quality;
            }

            public override double Weight { get; }

            public override bool IsWeighted => true;
        }

        public class NotWeightedWriter1 : WriterBase { }
        public class NotWeightedWriter2 : WriterBase { }
        public class NotWeightedWriter3 : WriterBase { }

        public class WeightedWriter1 : WeightedWriterBase
        {
            public WeightedWriter1(double quality) : base(quality) { }
        }

        public class WeightedWriter2 : WeightedWriterBase
        {
            public WeightedWriter2(double quality) : base(quality) { }
        }

        public class WeightedWriter3 : WeightedWriterBase
        {
            public WeightedWriter3(double quality) : base(quality) { }
        }
    }
}
