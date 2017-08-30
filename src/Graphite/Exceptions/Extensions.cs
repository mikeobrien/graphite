using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using Graphite.DependencyInjection;
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

        public static string GetDebugResponse(this Exception exception, HttpRequestMessage requestMessage)
        {
            if (exception is UnhandledGraphiteException)
                return exception.As<UnhandledGraphiteException>()
                    .GetDebugResponse(requestMessage);

            var message = new StringBuilder(10.KB());

            message
                .AppendLine("Action".WriteHeader())
                .AppendLine()
                .AppendLine($"Url Template: {requestMessage.RequestUri}")
                .AppendLine()
                .AppendLine("Request".WriteHeader())
                .AppendLine()
                .AppendLine($"{requestMessage.RequestUri}")
                .AppendLine()
                .AppendLine(requestMessage.RawHeaders())
                .AppendLine()
                .AppendLine("Exception".WriteHeader())
                .AppendLine()
                .AppendLine(exception.ToFriendlyException());

            return message.ToString();
        }

        private static string GetDebugResponse(this UnhandledGraphiteException exception, HttpRequestMessage requestMessage)
        {
            var message = new StringBuilder(100.KB());

            message
                .AppendLine("Action".WriteHeader())
                .AppendLine()
                .AppendLine($"Method: {exception.ActionDescriptor.Action}")
                .AppendLine($"Url Template: {exception.ActionDescriptor.Route.Url}")
                .AppendLine()
                .AppendLine("Request".WriteHeader())
                .AppendLine()
                .AppendLine($"{requestMessage.RequestUri}")
                .AppendLine()
                .AppendLine(requestMessage.RawHeaders())
                .AppendLine()
                .AppendLine("Exception".WriteHeader())
                .AppendLine()
                .AppendLine(exception.InnerException.ToFriendlyException());

            if (exception.Container.Parent != null)
            {
                var rootTrackingContainer = exception.Container.Parent as TrackingContainer;
                var requestTrackingContainer = exception.Container as TrackingContainer;

                if (rootTrackingContainer != null)
                    message
                        .AppendLine()
                        .AppendLine("Root Container Registrations".WriteHeader())
                        .AppendLine()
                        .AppendLine(rootTrackingContainer.Registry);

                if (requestTrackingContainer != null)
                    message
                        .AppendLine()
                        .AppendLine("Request Container Registrations".WriteHeader())
                        .AppendLine()
                        .AppendLine(requestTrackingContainer.Registry);

                message
                    .AppendLine()
                    .AppendLine("Root Container".WriteHeader())
                    .AppendLine()
                    .AppendLine(exception.Container.Parent.GetConfiguration()?.Trim())
                    .AppendLine()
                    .AppendLine("Request Container".WriteHeader())
                    .AppendLine()
                    .AppendLine(exception.Container.GetConfiguration()?.Trim());
            }
            else
            {
                var rootTrackingContainer = exception.Container as TrackingContainer;

                if (rootTrackingContainer != null)
                    message
                        .AppendLine()
                        .AppendLine("Root Container Registrations".WriteHeader())
                        .AppendLine()
                        .AppendLine(rootTrackingContainer.Registry);

                message
                    .AppendLine()
                    .AppendLine("Root Container".WriteHeader())
                    .AppendLine()
                    .AppendLine(exception.Container.GetConfiguration()?.Trim());
            }
            return message.ToString();
        }

        private static string WriteHeader(this string name) => $" {name} ".PadCenter(100, '█');
    }
}
