using System;
using Graphite.Binding;

namespace Tests.Common.Fakes
{
    public class TestValueMapper1 : TestValueMapper { }
    public class TestValueMapper2 : TestValueMapper { }

    public class TestValueMapper : IValueMapper
    {
        public Func<ValueMapperContext, bool> AppliesToFunc { get; set; }
        public Func<ValueMapperContext, MapResult> MapFunc { get; set; }
        public ValueMapperContext AppliesToContext { get; set; }
        public ValueMapperContext MapContext { get; set; }
        public bool AppliesToCalled { get; set; }
        public bool MapCalled { get; set; }

        public bool AppliesTo(ValueMapperContext context)
        {
            AppliesToCalled = true;
            AppliesToContext = context;
            return AppliesToFunc?.Invoke(context) ?? true;
        }

        public MapResult Map(ValueMapperContext context)
        {
            MapCalled = true;
            MapContext = context;
            return MapFunc(context);
        }
    }
}
