using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Results;
using Graphite.Actions;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Actions
{
    [TestFixture]
    public class AttributeResponseHeadersTests
    {
        public interface IHandler
        {
            [ResponseHeader("fark", "farker"),
             ResponseHeader("fark2", "farker2")]
            void WithAttribute();
            void NoAttribute();
        }

        [Test]
        public void Should_not_apply_to_actions_without_attribute()
        {
            var requestGraph = RequestGraph
                .CreateFor<IHandler>(x => x.NoAttribute());

            AttributeResponseHeaders.AppliesTo(requestGraph.GetActionConfigurationContext())
                .ShouldBeFalse();
        }

        [Test]
        public void Should_apply_to_actions_with_attribute()
        {
            var requestGraph = RequestGraph
                .CreateFor<IHandler>(x => x.WithAttribute());

            AttributeResponseHeaders.AppliesTo(requestGraph.GetActionConfigurationContext())
                .ShouldBeTrue();
        }

        [Test]
        public void Should_set_headers()
        {
            var requestGraph = RequestGraph
                .CreateFor<IHandler>(x => x.WithAttribute());
            var responseMessage = new HttpResponseMessage();

            new AttributeResponseHeaders(requestGraph.ActionMethod)
                .SetHeaders(new ResponseHeadersContext(responseMessage));

            responseMessage.Headers.GetValues("fark").ShouldOnlyContain("farker");
            responseMessage.Headers.GetValues("fark2").ShouldOnlyContain("farker2");
        }
    }
}
