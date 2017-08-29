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
        /// Specifies the default status code when binding fails.
        /// </summary>
        public ConfigurationDsl WithDefaultBindingFailureStatus(HttpStatusCode statusCode,
            Func<string, string> statusText = null)
        {
            _configuration.DefaultBindingFailureStatusCode = statusCode;
            if (statusText != null)
                _configuration.DefaultBindingFailureStatusText = statusText;
            return this;
        }

        /// <summary>
        /// Specifies the default status text when binding fails.
        /// </summary>
        public ConfigurationDsl WithDefaultBindingFailureStatusText(
            Func<string, string> statusText)
        {
            _configuration.DefaultBindingFailureStatusText = statusText;
            return this;
        }

        /// <summary>
        /// Specifies the default status code when no reader applies.
        /// </summary>
        public ConfigurationDsl WithDefaultNoReaderStatus(HttpStatusCode statusCode)
        {
            _configuration.DefaultNoReaderStatusCode = statusCode;
            return this;
        }

        /// <summary>
        /// Specifies the default status code and text when no reader applies.
        /// </summary>
        public ConfigurationDsl WithDefaultNoReaderStatus(
            HttpStatusCode statusCode, string statusText)
        {
            _configuration.DefaultNoReaderStatusCode = statusCode;
            _configuration.DefaultNoReaderStatusText = statusText;
            return this;
        }

        /// <summary>
        /// Specifies the default response status code and text.
        /// </summary>
        public ConfigurationDsl WithDefaultResponseStatus(
            HttpStatusCode statusCode, string statusText = null)
        {
            _configuration.DefaultHasResponseStatusCode = statusCode;
            if (statusText.IsNotNullOrEmpty())
                _configuration.DefaultHasResponseStatusText = statusText;
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
