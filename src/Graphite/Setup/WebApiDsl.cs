using System;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.Http.Hosting;
using System.Web.Routing;
using Graphite.Exceptions;
using Graphite.Extensions;
using Graphite.Http;

namespace Graphite.Setup
{
    public partial class ConfigurationDsl
    {
        /// <summary>
        /// Allows you to configure Web Api.
        /// </summary>
        public ConfigurationDsl ConfigureWebApi(Action<WebApiDsl> configure)
        {
            configure(new WebApiDsl(_httpConfiguration));
            return this;
        }
    }

    public class WebApiDsl
    {
        public WebApiDsl(HttpConfiguration configuration)
        {
            Configuration = configuration;
        }

        public HttpConfiguration Configuration { get; }

        /// <summary>
        /// Creates and applies a new buffer policy selector.
        /// </summary>
        public WebApiDsl CreateBufferPolicy(Action<BufferPolicyDsl> configure)
        {
            var policy = new BufferPolicySelector();
            configure(new BufferPolicyDsl(policy));
            Configuration.Services.Replace<IHostBufferPolicySelector>(policy);
            return this;
        }

        /// <summary>
        /// Adds a request scoped exception logger.
        /// </summary>
        public WebApiDsl AddExceptionLogger<T>() where T : class, IExceptionLogger
        {
            Configuration.Services.Add<IExceptionLogger, ExceptionLoggerWrapper<T>>();
            return this;
        }

        /// <summary>
        /// Sets a request scoped exception handler.
        /// </summary>
        public WebApiDsl SetExceptionHandler<T>() where T : class, IExceptionHandler
        {
            Configuration.Services.Replace<IExceptionHandler, ExceptionHandlerWrapper<T>>();
            return this;
        }

        /// <summary>
        /// Allows you to have a url that is the same as a physical path.
        /// </summary>
        public WebApiDsl RouteExistingFiles()
        {
            RouteTable.Routes.RouteExistingFiles = true;
            return this;
        }
    }

    public class BufferPolicyDsl
    {
        private readonly BufferPolicySelector _policy;

        public BufferPolicyDsl(BufferPolicySelector policy)
        {
            _policy = policy;
        }

        /// <summary>
        /// Prevents buffering of input.
        /// </summary>
        public BufferPolicyDsl DoNotBufferInput()
        {
            _policy.BufferAllInput = false;
            return this;
        }

        /// <summary>
        /// Prevents buffering of output.
        /// </summary>
        public BufferPolicyDsl DoNotBufferOutput()
        {
            _policy.BufferAllOutput = false;
            return this;
        }

        /// <summary>
        /// Includes a specific type of content to buffer.
        /// </summary>
        public BufferPolicyDsl BufferOutputOf<T>() where T : HttpContent
        {
            _policy.BufferAllOutput = false;
            _policy.IncludeForOutput.Add<T>();
            return this;
        }

        /// <summary>
        /// Excludes a specific type of content from buffering.
        /// </summary>
        public BufferPolicyDsl DoNotBufferOutputOf<T>() where T : HttpContent
        {
            _policy.ExcludeForOutput.Add<T>();
            return this;
        }

        /// <summary>
        /// Applies the WebApi buffering defaults. 
        /// Excludes StreamContent and PushStreamContent from buffering.
        /// </summary>
        public BufferPolicyDsl ApplyWebApiDefaults()
        {
            _policy.BufferAllInput = true;
            _policy.BufferAllOutput = true;
            _policy.ExcludeForOutput.Add<StreamContent>().Add<PushStreamContent>();
            return this;
        }

        /// <summary>
        /// Applies the Graphite buffering defaults. 
        /// Excludes AsyncContent from buffering.
        /// </summary>
        public BufferPolicyDsl ApplyGraphiteDefaults()
        {
            _policy.BufferAllInput = true;
            _policy.BufferAllOutput = true;
            _policy.ExcludeForOutput.Add<AsyncContent>();
            return this;
        }
    }
}