using System;
using System.Linq;
using System.Linq.Expressions;
using Graphite.Routing;
using NUnit.Framework;
using Should;
using Tests.Common;
using RangeAttribute = Graphite.Routing.RangeAttribute;

namespace Tests.Unit.Routing
{
    [TestFixture]
    public class DefaultInlineConstraintBuilderTests
    {
        public class Handler
        {
            public void Regex([Regex(".*")] string param) { }
            public void Alpha([Alpha] string param) { }

            public void Length([Length(5)] string param) { }
            public void MinLength([Length(min: 5)] string param) { }
            public void MaxLength([Length(max: 5)] string param) { }

            public void IntRange([Range(5, 10)] int param) { }
            public void IntStartRange([Range(min: 5)] int param) { }
            public void IntEndRange([Range(max: 5)] int param) { }

            public void LongRange([Range(5, 10)] long param) { }
            public void LongStartRange([Range(min: 5)] long param) { }
            public void LongEndRange([Range(max: 5)] long param) { }

            public void MatchBool([MatchType] bool param) { }
            public void MatchDateTime([MatchType] DateTime param) { }
            public void MatchDecimal([MatchType] decimal param) { }
            public void MatchDouble([MatchType] double param) { }
            public void MatchFloat([MatchType] float param) { }
            public void MatchGuid([MatchType] Guid param) { }
            public void MatchInt([MatchType] int param) { }
            public void MatchLong([MatchType] long param) { }
        }

        public static object[][] ConstraintCases = TestCaseSource
            .CreateWithExpression<Handler, string>(x => x
                .Add(h => h.Regex(null), "regex(.*)")
                .Add(h => h.Alpha(null), "alpha")

                .Add(h => h.Length(null), "length(5)")
                .Add(h => h.MinLength(null), "minlength(5)")
                .Add(h => h.MaxLength(null), "maxlength(5)")

                .Add(h => h.IntRange(0), "range(5,10)")
                .Add(h => h.IntStartRange(0), "min(5)")
                .Add(h => h.IntEndRange(0), "max(5)")

                .Add(h => h.LongRange(0), "range(5,10)")
                .Add(h => h.LongStartRange(0), "min(5)")
                .Add(h => h.LongEndRange(0), "max(5)")

                .Add(h => h.MatchBool(false), "bool")
                .Add(h => h.MatchDateTime(DateTime.MinValue), "datetime")
                .Add(h => h.MatchDecimal(0), "decimal")
                .Add(h => h.MatchDouble(0), "double")
                .Add(h => h.MatchFloat(0), "float")
                .Add(h => h.MatchGuid(Guid.Empty), "guid")
                .Add(h => h.MatchInt(0), "int")
                .Add(h => h.MatchLong(0), "long")
            );

        [TestCaseSource(nameof(ConstraintCases))]
        public void Should_build_regex_constraint(
            Expression<Action<Handler>> action, string constraint)
        {
            var requestGraph = RequestGraph.CreateFor(action)
                .AddUrlParameter("param");

            CreateBuilder(requestGraph)
                .Build(requestGraph.UrlParameters.First())
                .ShouldOnlyContain(constraint);
        }

        public class TypeMatchHandler
        {
            public void MatchBool(bool param) { }
            public void MatchDateTime(DateTime param) { }
            public void MatchDecimal(decimal param) { }
            public void MatchDouble(double param) { }
            public void MatchFloat(float param) { }
            public void MatchGuid(Guid param) { }
            public void MatchInt(int param) { }
            public void MatchLong(long param) { }
        }

        public static object[][] TypeConstraintCases = TestCaseSource
            .CreateWithExpression<TypeMatchHandler, bool, string>(x => x
                .Add(h => h.MatchBool(false), true, "bool")
                .Add(h => h.MatchDateTime(DateTime.MinValue), true, "datetime")
                .Add(h => h.MatchDecimal(0), true, "decimal")
                .Add(h => h.MatchDouble(0), true, "double")
                .Add(h => h.MatchFloat(0), true, "float")
                .Add(h => h.MatchGuid(Guid.Empty), true, "guid")
                .Add(h => h.MatchInt(0), true, "int")
                .Add(h => h.MatchLong(0), true, "long")

                .Add(h => h.MatchBool(false), false, null)
                .Add(h => h.MatchDateTime(DateTime.MinValue), false, null)
                .Add(h => h.MatchDecimal(0), false, null)
                .Add(h => h.MatchDouble(0), false, null)
                .Add(h => h.MatchFloat(0), false, null)
                .Add(h => h.MatchGuid(Guid.Empty), false, null)
                .Add(h => h.MatchInt(0), false, null)
                .Add(h => h.MatchLong(0), false, null)
            );

        [TestCaseSource(nameof(TypeConstraintCases))]
        public void Should_automatically_build_constraints_for_types_when_cnfigured(
            Expression<Action<TypeMatchHandler>> action, bool auto, string constraint)
        {
            var requestGraph = RequestGraph.CreateFor(action)
                .AddUrlParameter("param");

            requestGraph.Configuration.AutomaticallyConstrainUrlParameterByType = auto;

            var constraints = CreateBuilder(requestGraph)
                .Build(requestGraph.UrlParameters.First());

            if (constraint == null) constraints.ShouldBeEmpty();
            else constraints.ShouldOnlyContain(constraint);
        }

        private DefaultInlineConstraintBuilder CreateBuilder(RequestGraph requestGraph)
        {
            return new DefaultInlineConstraintBuilder(requestGraph.Configuration);
        }
    }
}
