using System;
using System.Linq;
using System.Text;
using Graphite.Extensions;
using Graphite.Linq;

namespace Graphite.Exceptions
{
    public static class Extensions
    {
        public static string ToFriendlyException(this Exception exception)
        {
            var message = new StringBuilder(1500);
            var lines = exception.ToString().Split(Environment.NewLine);

            if (!lines.Any()) return "";

            var levels = lines.First().Split("--->");
            message.AppendLine(levels.ToHierarchy());

            if (lines.Length == 1) return message.ToString();

            message.AppendLine();

            var indent = ' '.Repeat(levels.Length);

            lines.Skip(1).ForEach(x =>
            {
                var parts = x.Trim().Split(" in ");
                if (parts[0].StartsWith("-")) message.AppendLine();
                message.AppendLine(indent + parts[0]);
                if (parts.Length > 1)
                    message.AppendLine($"{indent}    in {parts[1]}");
                if (parts[0].EndsWith("-")) message.AppendLine();
            });

            return message.ToString();
        }
    }
}
