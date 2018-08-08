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
        /// Configures response status.
        /// </summary>
        public ConfigurationDsl ConfigureResponseStatus(Action<ConditionalPluginsDsl
            <IResponseStatus, ActionConfigurationContext>> configure)
        {
            _configuration.ResponseStatus.Configure(configure);
            return this;
        }
        /// <summary>
        /// Configures response headers.
        /// </summary>
        public ConfigurationDsl ConfigureResponseHeaders(Action<ConditionalPluginsDsl
            <IResponseHeaders, ActionConfigurationContext>> configure)
        {
            _configuration.ResponseHeaders.Configure(configure);
            return this;
        }

        /// <summary>
        /// Specifies the default buffer size in bytes. The default is 1MB.
        /// </summary>
        public ConfigurationDsl WithDefaultBufferSizeOf(int length)
        {
            _configuration.DefaultBufferSize = length;
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
            Func<string, string> reasonPhrase = null)
        {
            _configuration.DefaultBindingFailureStatusCode = statusCode;
            if (reasonPhrase != null)
                _configuration.DefaultBindingFailureReasonPhrase = reasonPhrase;
            return this;
        }

        /// <summary>
        /// Specifies the default status text when binding fails.
        /// </summary>
        public ConfigurationDsl WithDefaultBindingFailureReasonPhrase(
            Func<string, string> reasonPhrase)
        {
            _configuration.DefaultBindingFailureReasonPhrase = reasonPhrase;
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
            HttpStatusCode statusCode, string reasonPhrase)
        {
            _configuration.DefaultNoReaderStatusCode = statusCode;
            _configuration.DefaultNoReaderReasonPhrase = reasonPhrase;
            return this;
        }

        /// <summary>
        /// Specifies the default response status code and text.
        /// </summary>
        public ConfigurationDsl WithDefaultResponseStatus(
            HttpStatusCode statusCode, string reasonPhrase = null)
        {
            _configuration.DefaultHasResponseStatusCode = statusCode;
            if (reasonPhrase.IsNotNullOrEmpty())
                _configuration.DefaultHasResponseReasonPhrase = reasonPhrase;
            return this;
        }

        /// <summary>
        /// Specifies the default no response status code and text.
        /// </summary>
        public ConfigurationDsl WithDefaultNoResponseStatus(
            HttpStatusCode statusCode, string reasonPhrase = null)
        {
            _configuration.DefaultNoResponseStatusCode = statusCode;
            if (reasonPhrase.IsNotNullOrEmpty())
                _configuration.DefaultNoResponseReasonPhrase = reasonPhrase;
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
            HttpStatusCode statusCode, string reasonPhrase)
        {
            _configuration.DefaultNoWriterStatusCode = statusCode;
            _configuration.DefaultNoWriterReasonPhrase = reasonPhrase;
            return this;
        }
    }
}
