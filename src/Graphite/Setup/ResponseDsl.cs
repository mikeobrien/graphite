using System;
using System.Net;
using System.Text;
using Graphite.Actions;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Writers;

namespace Graphite.Setup
{
    public partial class ConfigurationDsl
    {
        /// <summary>
        /// Configures response writers.
        /// </summary>
        public ConfigurationDsl ConfigureResponseWriters(Action<ConditionalPluginsDsl
            <IResponseWriter, ActionConfigurationContext>> configure)
        {
            _configuration.ResponseWriters.Configure(configure);
            return this;
        }

        /// <summary>
        /// Indicates that responses should be disposed 
        /// if they implement IDisposable.
        /// </summary>
        public ConfigurationDsl DisposeResponses()
        {
            _configuration.DisposeResponses = true;
            return this;
        }

        /// <summary>
        /// Specifies the download buffer size in bytes. The default is 1MB.
        /// </summary>
        public ConfigurationDsl WithDownloadBufferSizeOf(int length)
        {
            _configuration.DownloadBufferSize = length;
            return this;
        }

        /// <summary>
        /// Specifies the default encoding.
        /// </summary>
        public ConfigurationDsl WithDefaultEncoding<T>(T encoding) where T : Encoding
        {
            _configuration.DefaultEncoding = encoding;
            return this;
        }

        /// <summary>
        /// Specifies the default response status code and text.
        /// </summary>
        public ConfigurationDsl WithDefaultResponseStatus(
            HttpStatusCode statusCode, string statusText = null)
        {
            _configuration.DefaultResponseStatusCode = statusCode;
            if (statusText.IsNotNullOrEmpty())
                _configuration.DefaultResponseStatusText = statusText;
            return this;
        }

        /// <summary>
        /// Specifies the default no response status code and text.
        /// </summary>
        public ConfigurationDsl WithDefaultNoResponseStatus(
            HttpStatusCode statusCode, string statusText = null)
        {
            _configuration.DefaultNoResponseStatusCode = statusCode;
            if (statusText.IsNotNullOrEmpty())
                _configuration.DefaultNoResponseStatusText = statusText;
            return this;
        }

        /// <summary>
        /// Specifies the default status code when no writer applies.
        /// </summary>
        public ConfigurationDsl WithDefaultNoWriterStatus(HttpStatusCode statusCode)
        {
            _configuration.DefaultNoWriterStatusCode = statusCode;
            return this;
        }

        /// <summary>
        /// Specifies the default status code and text when no writer applies.
        /// </summary>
        public ConfigurationDsl WithDefaultNoWriterStatus(
            HttpStatusCode statusCode, string statusText)
        {
            _configuration.DefaultNoWriterStatusCode = statusCode;
            _configuration.DefaultNoWriterStatusText = statusText;
            return this;
        }
    }
}
