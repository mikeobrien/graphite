using System;
using System.Net.Http;
using System.Reflection;
using Graphite.Actions;
using Graphite.Reflection;

namespace Graphite.Setup
{
    public partial class ConfigurationDsl
    {
        /// <summary>
        /// Disables the default error handler.
        /// </summary>
        public ConfigurationDsl DisableDefaultErrorHandler()
        {
            _configuration.DefaultErrorHandlerEnabled = false;
            return this;
        }

        /// <summary>
        /// Specifies the status text returned by an unhandled exception.
        /// </summary>
        public ConfigurationDsl WithUnhandledExceptionStatusText(string statusText)
        {
            _configuration.UnhandledExceptionStatusText = statusText;
            return this;
        }

        /// <summary>
        /// Specifies the catch all exception handler. This is NOT something 
        /// you would generally override and is NOT the appropriate place for 
        /// app and request error handling and logging. That should be done 
        /// at the app level (e.g. Global.asax) and with a behavior respectively.
        /// </summary>
        public ConfigurationDsl WithUnhandledExceptionHandler<T>() 
            where T : IUnhandledExceptionHandler
        {
            _configuration.UnhandledExceptionHandler.Set<T>();
            return this;
        }

        /// <summary>
        /// Specifies the catch all exception handler. This is NOT something 
        /// you would generally override and is NOT the appropriate place for 
        /// app and request error handling and logging. That should be done 
        /// at the app level (e.g. Global.asax) and with a behavior respectively.
        /// </summary>
        public ConfigurationDsl WithUnhandledExceptionHandler<T>(T instance) 
            where T : IUnhandledExceptionHandler
        {
            _configuration.UnhandledExceptionHandler.Set(instance);
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
