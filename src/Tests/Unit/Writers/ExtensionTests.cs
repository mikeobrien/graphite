using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Graphite;
using Graphite.Actions;
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
            var notWeightedWriter1 = new NotWeightedWriter1(configuration);
            var notWeightedWriter2 = new NotWeightedWriter2(configuration);
            var notWeightedWriter3 = new NotWeightedWriter3(configuration);
            var weightedWriter1 = new WeightedWriter1(.8, configuration);
            var weightedWriter2 = new WeightedWriter2(.5, configuration);
            var weightedWriter3 = new WeightedWriter3(.8, configuration);

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
                configuration.ResponseWriters.CloneAllThatApplyTo(null), null, null);

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
            public WriterBase(Configuration configuration) : base(configuration) { }

            public override Task<HttpResponseMessage> 
                WriteResponse(ResponseWriterContext context)
            {
                throw new NotImplementedException();
            }
        }

        public class WeightedWriterBase : WriterBase
        {
            public WeightedWriterBase(double quality, 
                Configuration configuration) : 
                base(configuration)
            {
                Weight = quality;
            }

            public override double Weight { get; }

            public override bool IsWeighted => true;
        }

        public class NotWeightedWriter1 : WriterBase
        {
            public NotWeightedWriter1(Configuration configuration) : base(configuration) { }
        }

        public class NotWeightedWriter2 : WriterBase
        {
            public NotWeightedWriter2(Configuration configuration) : base(configuration) { }
        }

        public class NotWeightedWriter3 : WriterBase
        {
            public NotWeightedWriter3(Configuration configuration) : base(configuration) { }
        }

        public class WeightedWriter1 : WeightedWriterBase
        {
            public WeightedWriter1(double quality, Configuration configuration) : 
                base(quality, configuration) { }
        }

        public class WeightedWriter2 : WeightedWriterBase
        {
            public WeightedWriter2(double quality, Configuration configuration) : 
                base(quality, configuration) { }
        }

        public class WeightedWriter3 : WeightedWriterBase
        {
            public WeightedWriter3(double quality, Configuration configuration) : 
                base(quality, configuration) { }
        }
    }
}
