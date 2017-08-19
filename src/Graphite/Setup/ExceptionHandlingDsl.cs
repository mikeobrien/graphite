using System;
using System.Net.Http;
using System.Reflection;
using Graphite.Exceptions;
using Graphite.Reflection;

namespace Graphite.Setup
{
    public partial class ConfigurationDsl
    {
        /// <summary>
        /// Specifies the status text returned by an unhandled exception.
        /// </summary>
        public ConfigurationDsl WithUnhandledExceptionStatusText(string statusText)
        {
            _configuration.UnhandledExceptionStatusText = statusText;
            return this;
        }

        /// <summary>
        /// Specifies the exception handler. This handler is not 
        /// for general use and enables some Graphite features.
        /// Logging and exception handling should be handled 
        /// through the Web Api API.
        /// </summary>
        public ConfigurationDsl WithExceptionHandler<T>() 
            where T : IExceptionHandler
        {
            _configuration.ExceptionHandler.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the exception handler. This handler is not 
        /// for general use and enables some Graphite features.
        /// Logging and exception handling should be handled 
        /// through the Web Api API.
        /// </summary>
        public ConfigurationDsl WithExceptionHandler<T>(T instance) 
            where T : IExceptionHandler
        {
            _configuration.ExceptionHandler.Set(instance);
            return this;
        }

        /// <summary>
        /// Specifies the debug response generator. This response is 
        /// returned when debug response messages are enabled.
        /// </summary>
        public ConfigurationDsl WithExceptionDebugResponse<T>()
            where T : IExceptionDebugResponse
        {
            _configuration.ExceptionDebugResponse.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the debug response generator. This response is 
        /// returned when debug response messages are enabled.
        /// </summary>
        public ConfigurationDsl WithExceptionDebugResponse<T>(T instance)
            where T : IExceptionDebugResponse
        {
            _configuration.ExceptionDebugResponse.Set(instance);
            return this;
        }

        /// <summary>
        /// Returns the request headers, stack trace and 
        /// container configuration of unhandled exceptions.
        /// </summary>
        public ConfigurationDsl ReturnErrorMessages()
        {
            ReturnErrorMessagesWhen(x => true);
            return this;
        }

        /// <summary>
        /// Returns the request headers, stack trace and 
        /// container configuration of unhandled exceptions
        /// when the predicate is true.
        /// </summary>
        public ConfigurationDsl ReturnErrorMessagesWhen(
            Func<HttpRequestMessage, bool> predicate)
        {
            _configuration.ReturnErrorMessage = predicate;
            return this;
        }

        /// <summary>
        /// Returns the request headers, stack trace and 
        /// container configuration of unhandled exceptions
        /// when calling assembly is in debug mode.
        /// </summary>
        public ConfigurationDsl ReturnErrorMessagesInDebugMode()
        {
            var debugMode = Assembly.GetCallingAssembly().IsInDebugMode();
            ReturnErrorMessagesWhen(x => debugMode);
            return this;
        }

        /// <summary>
        /// Returns the request headers, stack trace and 
        /// container configuration of unhandled exceptions
        /// when calling assembly is in debug mode 
        /// or the predicate is true.
        /// </summary>
        public ConfigurationDsl ReturnErrorMessagesInDebugModeOrWhen(
            Func<HttpRequestMessage, bool> predicate)
        {
            var debugMode = Assembly.GetCallingAssembly().IsInDebugMode();
            ReturnErrorMessagesWhen(x => debugMode || predicate(x));
            return this;
        }

        /// <summary>
        /// Returns the request headers, stack trace and 
        /// container configuration of unhandled exceptions
        /// when type assembly is in debug mode.
        /// </summary>
        public ConfigurationDsl ReturnErrorMessagesInDebugMode<T>()
        {
            var debugMode = typeof(T).Assembly.IsInDebugMode();
            ReturnErrorMessagesWhen(x => debugMode);
            return this;
        }

        /// <summary>
        /// Returns the request headers, stack trace and 
        /// container configuration of unhandled exceptions
        /// when type assembly is in debug mode
        /// or the predicate is true.
        /// </summary>
        public ConfigurationDsl ReturnErrorMessagesInDebugModeOrWhen<T>(
            Func<HttpRequestMessage, bool> predicate)
        {
            var debugMode = typeof(T).Assembly.IsInDebugMode();
            ReturnErrorMessagesWhen(x => debugMode || predicate(x));
            return this;
        }
    }
}
