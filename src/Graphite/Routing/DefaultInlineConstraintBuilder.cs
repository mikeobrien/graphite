using System;
using System.Collections.Generic;
using Graphite.Extensions;

namespace Graphite.Routing
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class RegexAttribute : Attribute
    {
        public RegexAttribute(string regex)
        {
            Regex = regex;
        }

        public string Regex { get; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class AlphaAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class LengthAttribute : Attribute
    {
        public LengthAttribute(int length)
        {
            Length = length;
        }

        public LengthAttribute(int min = -1, int max = -1)
        {
            if (min >= 0) Min = min;
            if (max >= 0) Max = max;
        }

        public int? Length { get; }
        public int? Min { get; }
        public int? Max { get; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class RangeAttribute : Attribute
    {
        public RangeAttribute(int min = -1, int max = -1)
        {
            if (min >= 0) Min = min;
            if (max >= 0) Max = max;
        }

        public int? Min { get; }
        public int? Max { get; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class MatchTypeAttribute : Attribute { }

    public class DefaultInlineConstraintBuilder : IInlineConstraintBuilder
    {
        private readonly Configuration _configuration;

        public DefaultInlineConstraintBuilder(Configuration configuration)
        {
            _configuration = configuration;
        }

        public List<string> Build(UrlParameter parameter)
        {
            var constraints = new List<string>();

            if (parameter.TypeDescriptor.Type == typeof(string))
            {
                if (parameter.HasAttribute<AlphaAttribute>()) constraints.Add("alpha");

                if (parameter.HasAttribute<LengthAttribute>())
                {
                    var length = parameter.GetAttribute<LengthAttribute>();
                    if (length.Length.HasValue) constraints.Add($"length({length.Length})");
                    else if (length.Min.HasValue && length.Max.HasValue)
                        constraints.Add($"length({length.Min},{length.Max})");
                    else if (length.Min.HasValue) constraints.Add($"minlength({length.Min})");
                    else if (length.Max.HasValue) constraints.Add($"maxlength({length.Max})");
                }
            }

            if ((parameter.TypeDescriptor.Type == typeof(int) ||
                 parameter.TypeDescriptor.Type == typeof(long)) &&
                parameter.HasAttribute<RangeAttribute>())
            {
                var range = parameter.GetAttribute<RangeAttribute>();
                if (range.Min.HasValue && range.Max.HasValue)
                    constraints.Add($"range({range.Min},{range.Max})");
                else if (range.Min.HasValue) constraints.Add($"min({range.Min})");
                else if (range.Max.HasValue) constraints.Add($"max({range.Max})");
            }

            if (parameter.HasAttribute<RegexAttribute>())
            {
                var regex = parameter.GetAttribute<RegexAttribute>()?.Regex;
                if (regex.IsNotNullOrEmpty()) constraints.Add($"regex({regex})");
            }

            if (parameter.HasAttribute<MatchTypeAttribute>() || 
                _configuration.AutomaticallyConstrainUrlParameterByType)
            {
                if (parameter.TypeDescriptor.Type == typeof(bool)) constraints.Add("bool");
                else if (parameter.TypeDescriptor.Type == typeof(DateTime)) constraints.Add("datetime");
                else if (parameter.TypeDescriptor.Type == typeof(decimal)) constraints.Add("decimal");
                else if (parameter.TypeDescriptor.Type == typeof(double)) constraints.Add("double");
                else if (parameter.TypeDescriptor.Type == typeof(float)) constraints.Add("float");
                else if (parameter.TypeDescriptor.Type == typeof(Guid)) constraints.Add("guid");
                else if (parameter.TypeDescriptor.Type == typeof(int)) constraints.Add("int");
                else if (parameter.TypeDescriptor.Type == typeof(long)) constraints.Add("long");
            }

            return constraints;
        }
    }
}