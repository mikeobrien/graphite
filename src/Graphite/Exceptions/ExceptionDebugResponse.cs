using System.Text;
using Graphite.DependencyInjection;
using Graphite.Extensions;

namespace Graphite.Exceptions
{
    public class ExceptionDebugResponse : IExceptionDebugResponse
    {
        public string GetResponse(ExceptionContext context)
        {
            string WriteHeader(string name) => $" {name} ".PadCenter(100, '█');

            var message = new StringBuilder(100.KB());

            message
                .AppendLine(WriteHeader("Action"))
                .AppendLine()
                .AppendLine($"Method: {context.ActionDescriptor.Action}")
                .AppendLine($"Url Template: {context.ActionDescriptor.Route.Url}")
                .AppendLine()
                .AppendLine(WriteHeader("Request"))
                .AppendLine()
                .AppendLine($"{context.RequestMessage.RequestUri}")
                .AppendLine()
                .AppendLine(context.RequestMessage.RawHeaders())
                .AppendLine()
                .AppendLine(WriteHeader("Exception"))
                .AppendLine()
                .AppendLine(context.Exception.ToFriendlyException());

            if (context.Container.Parent != null)
            {
                var rootTrackingContainer = context.Container.Parent as TrackingContainer;
                var requestTrackingContainer = context.Container as TrackingContainer;

                if (rootTrackingContainer != null)
                    message
                        .AppendLine()
                        .AppendLine(WriteHeader("Root Container Registrations"))
                        .AppendLine()
                        .AppendLine(rootTrackingContainer.Registry);

                if (requestTrackingContainer != null)
                    message
                        .AppendLine()
                        .AppendLine(WriteHeader("Request Container Registrations"))
                        .AppendLine()
                        .AppendLine(requestTrackingContainer.Registry);

                message 
                    .AppendLine()
                    .AppendLine(WriteHeader("Root Container"))
                    .AppendLine()
                    .AppendLine(context.Container.Parent.GetConfiguration()?.Trim())
                    .AppendLine()
                    .AppendLine(WriteHeader("Request Container"))
                    .AppendLine()
                    .AppendLine(context.Container.GetConfiguration()?.Trim());
            }
            else
            {
                var rootTrackingContainer = context.Container as TrackingContainer;

                if (rootTrackingContainer != null)
                    message
                        .AppendLine()
                        .AppendLine(WriteHeader("Root Container Registrations"))
                        .AppendLine()
                        .AppendLine(rootTrackingContainer.Registry);

                message
                    .AppendLine()
                    .AppendLine(WriteHeader("Root Container")) 
                    .AppendLine()
                    .AppendLine(context.Container.GetConfiguration()?.Trim());
            }
            return message.ToString();
        }

    }
}