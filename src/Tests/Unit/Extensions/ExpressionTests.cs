using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Graphite.Actions;
using Graphite.Extensions;
using Graphite.Reflection;
using NUnit.Framework;
using Should;
using Tests.Common;

namespace Tests.Unit.Extensions
{
    [TestFixture]
    public class ExpressionTests
    {
        [Test]
        public void Should_get_method_info_from_exprerssion()
        {
            Expression<Func<string, object>> methodExpression = x => x.Clone();

            var result = methodExpression.GetMethodInfo();

            result.ShouldEqual(typeof(string).GetMethod(nameof(string.Clone)));
        }
    }
}
