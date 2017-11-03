using Graphite.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace Graphite.Routing
{
    public static class SegmentExtensions
    {
        public static string ToUrl(this IEnumerable<UrlSegment> segments)
        {
            return segments.Select(x => x.Content).Join("/");
        }

        public static string JoinUrls(this string left, string right)
        {
            if (left.IsNullOrEmpty()) return right;
            if (right.IsNullOrEmpty()) return left;
            return $"{left.TrimEnd('/')}/{right.TrimStart('/')}";
        }
    }

    public class UrlSegment
    {
        private readonly string _content;

        public UrlSegment(string content)
        {
            _content = content;
            Constraints = new List<string>();
        }

        public UrlSegment(UrlParameter parameter, List<string> constraints)
        {
            Parameter = parameter;
            Constraints = constraints;
            IsParameter = true;
        }

        public string Content => !IsParameter ? _content : BuildSegment();

        public bool IsParameter { get; }
        public UrlParameter Parameter { get; }
        public IEnumerable<string> Constraints { get; }

        private string BuildSegment()
        {
            return $"{{{(Parameter.IsWildcard ? "*" : "")}{Parameter.Name}" +
                   $"{Constraints.Select(x => $":{x}").Join()}}}";
        }
    }
}
