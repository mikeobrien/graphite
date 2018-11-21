using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Extensions;
using Graphite.Linq;

namespace Graphite.Http
{
    public class HttpMethods : List<HttpMethod>
    {
        public HttpMethods Add(string method, bool allowRequestBody, bool allowResponseBody)
        {
            Add(new HttpMethod(method, allowRequestBody, allowResponseBody));
            return this;
        }
        
        public HttpMethods RemoveHttpMethod(params string[] methods)
        {
            methods.ForEach(method =>
            {
                var remove = this.FirstOrDefault(x => x.Method.EqualsUncase(method));
                if (remove != null) Remove(remove);
            });
            return this;
        }
        
        public HttpMethods RemoveHttpMethod(params HttpMethod[] methods)
        {
            methods.ForEach(method => Remove(method));
            return this;
        }

        public HttpMethods Get(Action<HttpMethodDsl> configure)
        {
            return Configure(HttpMethod.Get, configure);
        }

        public HttpMethods Post(Action<HttpMethodDsl> configure)
        {
            return Configure(HttpMethod.Post, configure);
        }

        public HttpMethods Put(Action<HttpMethodDsl> configure)
        {
            return Configure(HttpMethod.Put, configure);
        }

        public HttpMethods Patch(Action<HttpMethodDsl> configure)
        {
            return Configure(HttpMethod.Patch, configure);
        }

        public HttpMethods Delete(Action<HttpMethodDsl> configure)
        {
            return Configure(HttpMethod.Delete, configure);
        }

        public HttpMethods Options(Action<HttpMethodDsl> configure)
        {
            return Configure(HttpMethod.Options, configure);
        }

        public HttpMethods Head(Action<HttpMethodDsl> configure)
        {
            return Configure(HttpMethod.Head, configure);
        }

        public HttpMethods Trace(Action<HttpMethodDsl> configure)
        {
            return Configure(HttpMethod.Trace, configure);
        }

        public HttpMethods Connect(Action<HttpMethodDsl> configure)
        {
            return Configure(HttpMethod.Connect, configure);
        }

        public HttpMethod this[string method] => this.FirstOrDefault(
            x => method?.EqualsUncase(x.Method) ?? false);

        public bool Contains(string method)
        {
            return this.Any(x => method.EqualsUncase(x.Method));
        }

        private HttpMethods Configure(HttpMethod method, Action<HttpMethodDsl> configure)
        {
            var current = this.FirstOrDefault(x => x.Method.EqualsUncase(method.Method));
            if (current != null) Remove(current);
            Add(CreateFrom(current ?? method, configure));
            return this;
        }

        private HttpMethod CreateFrom(HttpMethod method, Action<HttpMethodDsl> configure)
        {
            var dsl = new HttpMethodDsl(method);
            configure(dsl);
            return dsl.Create();
        }
    }
}