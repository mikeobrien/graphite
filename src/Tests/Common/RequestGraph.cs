using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Routing;
using Graphite;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.DependencyInjection;
using Graphite.Routing;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Readers;
using Graphite.Reflection;
using Graphite.StructureMap;
using Graphite.Writers;
using Tests.Common.Fakes;
using HttpMethod = System.Net.Http.HttpMethod;

namespace Tests.Common
{
    public class RequestGraph
    {
        private readonly List<ActionParameter> _parameters = new List<ActionParameter>();
        private string _urlTemplate;

        public RequestGraph(ActionMethod actionMethod)
        {
            ActionMethod = actionMethod;
            ResponseType = !actionMethod.MethodDescriptor.HasResult ? 
                null : actionMethod.MethodDescriptor.ReturnType;
            CancellationToken = new CancellationToken();
            HttpConfiguration = new HttpConfiguration();
            ActionArguments = new object[actionMethod.MethodDescriptor.Parameters.Length];
            HttpMethod = "GET";
            Url = "http://fark.com";
            _urlTemplate = "";
            UnderlyingContainer = new StructureMap.Container();
            Configuration = new Configuration();
            Container = new Container(UnderlyingContainer);
            Container.Register(Configuration);
            TypeCache = new TypeCache();
        }

        public class Handler { public void Action() { } }

        public static RequestGraph Create()
        {
            return CreateFor<Handler>(x => x.Action());
        }

        public static RequestGraph CreateFor<T>(Expression<Action<T>> method)
        {
            return CreateFor(ActionMethod.From(method));
        }

        public static RequestGraph CreateFor<T>(Expression<Func<T, object>> method)
        {
            return CreateFor(ActionMethod.From(method));
        }

        public static RequestGraph CreateFor(LambdaExpression lambda)
        {
            return CreateFor(ActionMethod.From(lambda));
        }

        public static RequestGraph CreateFor(ActionMethod actionMethod)
        {
            return new RequestGraph(actionMethod);
        }

        public List<UrlParameter> UrlParameters { get; } = new List<UrlParameter>();
        public ITypeCache TypeCache { get; set; }
        public Configuration Configuration { get; }
        public object[] ActionArguments { get; }
        public TypeDescriptor ResponseType { get; }
        public ParameterDescriptor RequestParameter { get; private set; }
        public ActionMethod ActionMethod { get; }
        public string HttpMethod { get; set; }
        public HttpConfiguration HttpConfiguration { get; }
        public CancellationToken CancellationToken { get; }
        public StructureMap.IContainer UnderlyingContainer { get; }
        public IContainer Container { get; }
        public string ContentType { get; set; }
        public string AttachmentFilename { get; set; }
        public string Accept { get; set; }
        public byte[] RequestData { get; set; }
        public Dictionary<string, object> RequestProperties { get; } = new Dictionary<string, object>();
        public Dictionary<string, string> Cookies { get; } = new Dictionary<string, string>();
        public Dictionary<string, string> Headers { get; } = new Dictionary<string, string>();

        public string Url { get; set; }

        public string UrlTemplate
        {
            get { return _urlTemplate; }
            set
            {
                _urlTemplate = value;
                UrlParameters.Clear();
                var parameters = new Regex(@"\{(\*?\w*)\}").Matches(value)
                    .Cast<Match>().Select(x => new
                    {
                        Parameter = GetParameter(x.Groups[1].Value.TrimStart('*')),
                        IsWildcard = x.Groups[1].Value.StartsWith("*")
                    }).ToList();

                UrlParameters.AddRange(parameters.Select(x => new UrlParameter(x.Parameter, x.IsWildcard)));
            }
        }

        public RequestGraph WithRequestData(string data, Encoding encoding = null)
        {
            RequestData = (encoding ?? Encoding.UTF8).GetBytes(data);
            return this;
        }

        public RequestGraph WithContentType(string contentType)
        {
            ContentType = contentType;
            return this;
        }

        public RequestGraph WithAttachmentFilename(string filename)
        {
            AttachmentFilename = filename;
            return this;
        }

        public RequestGraph WithAccept(string accept)
        {
            Accept = accept;
            return this;
        }

        public RequestGraph WithUrl(string url)
        {
            Url = url;
            return this;
        }

        public RequestGraph WithUrlTemplate(string urlTemplate)
        {
            UrlTemplate = urlTemplate;
            return this;
        }

        public RequestGraph Configure(Action<ConfigurationDsl> config)
        {
            config(new ConfigurationDsl(Configuration));
            return this;
        }

        public RequestGraph ConfigureContainer(Action<IContainer> config)
        {
            config(Container);
            return this;
        }

        public RequestGraph AddRequestProperty(string name, object value)
        {
            RequestProperties[name] = value;
            return this;
        }

        public HttpResponseMessage GetHttpResponseMessage()
        {
            return GetHttpRequestMessage().CreateResponse();
        }

        public RouteDescriptor GetRouteDescriptor()
        {
            return new RouteDescriptor(HttpMethod, Url, UrlParameters.ToArray(),
                _parameters.ToArray(), RequestParameter, ResponseType);
        }

        public ActionParameter[] GetActionParameters()
        {
            return _parameters.ToArray();
        }

        public QuerystringParameters GetQuerystringParameters()
        {
            return new QuerystringParameters(
                GetHttpRequestMessage().GetQueryNameValuePairs()
                    .ToLookup(x => x.Key, x => (object)x.Value));
        }

        public UrlParameters GetUrlParameters()
        {
            return new UrlParameters(GetHttpRequestMessage()
                .GetRequestContext().RouteData.Values
                .ToDictionary(x => x.Key, x => x.Value));
        }

        public ActionDescriptor GetActionDescriptor()
        {
            return new ActionDescriptor(ActionMethod, GetRouteDescriptor(), GetBehaviors());
        }

        public HttpRequestMessage GetHttpRequestMessage()
        {
            var message = new HttpRequestMessage(new HttpMethod(HttpMethod), Url);
            message.SetRequestContext(GetHttpRequestContext(message));
            if (RequestData != null) message.Content = new ByteArrayContent(RequestData);
            else message.Content = new HttpMessageContent(message);
            if (ContentType.IsNotNullOrEmpty()) message.Content.Headers
                .ContentType = new MediaTypeHeaderValue(ContentType);
            if (Accept.IsNotNullOrEmpty()) message.Headers.Add("Accept", Accept);
            if (AttachmentFilename.IsNotNullOrEmpty())
                message.Content.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attachment")
                    {
                        FileName = AttachmentFilename
                    };
            RequestProperties.ForEach(x => message.Properties[x.Key] = x.Value);
            if (Cookies.Any()) message.Headers.Add("Cookie", 
                Cookies.Select(x => $"{x.Key}={x.Value}").Join("; "));
            Headers.ForEach(x => message.Headers.Add(x.Key, x.Value));
            return message;
        }

        public HttpRequestContext GetHttpRequestContext(HttpRequestMessage httpRequestMessage)
        {
            return new HttpRequestContext
            {
                RouteData = new HttpRoute(UrlTemplate).GetRouteData("", httpRequestMessage)
            };
        }

        public RequestGraph WithRequestParameter(string name)
        {
            RequestParameter = GetParameter(name);
            return this;
        }

        public RequestGraph AddUrlParameter(string name, bool wildcard = false)
        {
            UrlParameters.Add(new UrlParameter(GetParameter(name), wildcard));
            return this;
        }

        public RequestGraph AddCookie(string name, string value)
        {
            Cookies[name] = value;
            return this;
        }

        public RequestGraph AddHeader(string name, string value)
        {
            Headers[name] = value;
            return this;
        }

        public RequestGraph AddAllActionParameters(bool includeComplexTypeProperties = false)
        {
            _parameters.AddRange(ActionMethod.MethodDescriptor.Parameters
                .Select(x => new ActionParameter(x)));
            if (includeComplexTypeProperties)
                _parameters
                    .AddRange(ActionMethod.MethodDescriptor.Parameters
                        .Where(x => x.ParameterType.IsComplexType)
                        .SelectMany(x => x.ParameterType.Properties
                            .Select(y => new ActionParameter(x, y))));
            return this;
        }

        public RequestGraph AddParameters(params string[] names)
        {
            _parameters.AddRange(names.Select(name => new ActionParameter(GetParameter(name))));
            return this;
        }

        public bool RemoveParameter(string name)
        {
            var parameter = _parameters.FirstOrDefault(x => x.Name.EqualsIgnoreCase(name));
            if (parameter != null) _parameters.Remove(parameter);
            return parameter != null;
        }

        private ParameterDescriptor GetParameter(string name)
        {
            var parameter = ActionMethod.MethodDescriptor.Parameters
                .FirstOrDefault(x => x.Name.EqualsIgnoreCase(name));
            if (parameter == null) throw new Exception($"Could not find parameter {name} " +
                $@"should be {ActionMethod.MethodDescriptor.Parameters.Select(x => 
                    x.Name).Join(", ")}.");
            return parameter;
        }

        public RequestGraph AddModelParameters(string parameterName, params string[] names)
        {
            var parameter = GetParameter(parameterName);
            _parameters.AddRange(names.Select(name => new ActionParameter(
                parameter, GetProperty(parameter, name))));
            return this;
        }

        private PropertyDescriptor GetProperty(ParameterDescriptor parameter, string name)
        {
            var property = parameter.ParameterType.Properties
                .FirstOrDefault(x => x.Name.EqualsIgnoreCase(name));
            if (property == null) throw new Exception($"Could not find property {name} " +
                $@"should be {parameter.ParameterType.Properties.Select(x =>
                    x.Name).Join(", ")}.");
            return property;
        }

        public ParameterBinder GetParameterBinder()
        {
            return new ParameterBinder(
                GetConfigurationContext(),
                ActionMethod,
                GetRouteDescriptor(),
                ValueMappers);
        }

        public ConfigurationContext GetConfigurationContext()
        {
            return new ConfigurationContext(Configuration, HttpConfiguration);
        }

        public ActionConfigurationContext GetActionConfigurationContext()
        {
            return new ActionConfigurationContext(GetConfigurationContext(), 
                ActionMethod, GetRouteDescriptor());
        }

        private TypeDescriptor[] GetBehaviors()
        {
            return Configuration.Behaviors.ThatApplyTo(new ActionConfigurationContext(
                    GetConfigurationContext(), ActionMethod, GetRouteDescriptor()))
                .Select(x => x.Type).Select(x => TypeCache.GetTypeDescriptor(x)).ToArray();
        }

        /* ---------------- Parameter Mappers --------------- */

        public ValueMapperContext GetParameterMapperContext(ActionParameter parameter, 
            Type destinationType, object[] value)
        {
            return new ValueMapperContext(parameter, value);
        }

        public List<IValueMapper> ValueMappers { get; } = new List<IValueMapper>();
        public TestValueMapper1 ValueMapper1 { get; private set; }
        public TestValueMapper2 ValueMapper2 { get; private set; }

        public RequestGraph AddValueMapper<T>(T mapper,
            Func<ValueMapperContext, bool> configAppliesTo = null) 
            where T : IValueMapper
        {
            Configuration.ValueMappers.Append<T>(configAppliesTo);
            ValueMappers.Add(mapper);
            return this;
        }

        public RequestGraph AddValueMapper1(Func<ValueMapperContext, object> map, 
            Func<ValueMapperContext, bool> configAppliesTo = null, 
            Func<ValueMapperContext, bool> instanceAppliesTo = null)
        {
            if (ValueMapper1 != null) throw new Exception("Parameter mapper 1 already added.");
            ValueMapper1 = AddValueMapper<TestValueMapper1>(map, configAppliesTo, instanceAppliesTo);
            return this;
        }

        public RequestGraph AddValueMapper2(Func<ValueMapperContext, object> map,
            Func<ValueMapperContext, bool> configAppliesTo = null,
            Func<ValueMapperContext, bool> instanceAppliesTo = null)
        {
            if (ValueMapper2 != null) throw new Exception("Parameter mapper 2 already added.");
            ValueMapper2 = AddValueMapper<TestValueMapper2>(map, configAppliesTo, instanceAppliesTo);
            return this;
        }

        private T AddValueMapper<T>(Func<ValueMapperContext, object> map,
            Func<ValueMapperContext, bool> configAppliesTo,
            Func<ValueMapperContext, bool> instanceAppliesTo)
            where T : TestValueMapper, new()
        {
            Configuration.ValueMappers.Append<T>(configAppliesTo);
            var mapper = new T
            {
                AppliesToFunc = instanceAppliesTo,
                MapFunc = map
            };
            ValueMappers.Add(mapper);
            return mapper;
        }

        public RequestGraph AppendValueMapper<T>(Func<ValueMapperContext, bool> configAppliesTo = null)
            where T : IValueMapper, new()
        {
            Configuration.ValueMappers.Append<T>(configAppliesTo);
            var mapper = new T();
            ValueMappers.Add(mapper);
            return this;
        }

        /* ---------------- Request Readers --------------- */

        public List<IRequestReader> RequestReaders { get; } = new List<IRequestReader>();
        public TestRequestReader1 RequestReader1 { get; private set; }
        public TestRequestReader2 RequestReader2 { get; private set; }

        public RequestGraph AddRequestReader1(Func<Task<object>> read,
            Func<ActionConfigurationContext, bool> configAppliesTo = null,
            Func<bool> instanceAppliesTo = null)
        {
            if (RequestReader1 != null) throw new Exception("Request reader 1 already added.");
            RequestReader1 = AddRequestReader<TestRequestReader1>(read, configAppliesTo, instanceAppliesTo);
            return this;
        }

        public RequestGraph AddRequestReader2(Func<Task<object>> read,
            Func<ActionConfigurationContext, bool> configAppliesTo = null,
            Func<bool> instanceAppliesTo = null)
        {
            if (RequestReader2 != null) throw new Exception("Request reader 2 already added.");
            RequestReader2 = AddRequestReader<TestRequestReader2>(read, configAppliesTo, instanceAppliesTo);
            return this;
        }

        private T AddRequestReader<T>(Func<Task<object>> read,
            Func<ActionConfigurationContext, bool> configAppliesTo,
            Func<bool> instanceAppliesTo)
            where T : TestRequestReader, new()
        {
            Configuration.RequestReaders.Append<T>(configAppliesTo);
            var reader = new T
            {
                AppliesFunc = instanceAppliesTo,
                ReadFunc = read
            };
            RequestReaders.Add(reader);
            return reader;
        }

        /* ---------------- Request Binders --------------- */

        public RequestBinderContext GetRequestBinderContext()
        {
            return new RequestBinderContext(ActionArguments);
        }

        public List<IRequestBinder> RequestBinders { get; } = new List<IRequestBinder>();
        public TestRequestBinder1 RequestBinder1 { get; private set; }
        public TestRequestBinder2 RequestBinder2 { get; private set; }

        public RequestGraph AddRequestBinder1(Func<RequestBinderContext, Task> bind,
            Func<ActionConfigurationContext, bool> configAppliesTo = null,
            Func<RequestBinderContext, bool> instanceAppliesTo = null)
        {
            if (RequestBinder1 != null) throw new Exception("Request binder 1 already added.");
            RequestBinder1 = AddRequestBinder<TestRequestBinder1>(bind, configAppliesTo, instanceAppliesTo);
            return this;
        }

        public RequestGraph AddRequestBinder2(Func<RequestBinderContext, Task> bind,
            Func<ActionConfigurationContext, bool> configAppliesTo = null,
            Func<RequestBinderContext, bool> instanceAppliesTo = null)
        {
            if (RequestBinder2 != null) throw new Exception("Request binder 2 already added.");
            RequestBinder2 = AddRequestBinder<TestRequestBinder2>(bind, configAppliesTo, instanceAppliesTo);
            return this;
        }

        private T AddRequestBinder<T>(Func<RequestBinderContext, Task> bind,
            Func<ActionConfigurationContext, bool> configAppliesTo,
            Func<RequestBinderContext, bool> instanceAppliesTo)
            where T : TestRequestBinder, new()
        {
            Configuration.RequestBinders.Append<T>(configAppliesTo);
            var binder = new T
            {
                AppliesToFunc = instanceAppliesTo,
                BindFunc = bind
            };
            RequestBinders.Add(binder);
            return binder;
        }

        /* ---------------- Response Writers --------------- */

        public ResponseWriterContext GetResponseWriterContext(object response)
        {
            return new ResponseWriterContext(response);
        }

        public List<IResponseWriter> ResponseWriters { get; } = new List<IResponseWriter>();
        public TestResponseWriter1 ResponseWriter1 { get; private set; }
        public TestResponseWriter2 ResponseWriter2 { get; private set; }

        public RequestGraph AddResponseWriter1(Func<ResponseWriterContext, Task<HttpResponseMessage>> write,
            Func<ActionConfigurationContext, bool> configAppliesTo = null,
            Func<ResponseWriterContext, bool> instanceAppliesTo = null)
        {
            if (ResponseWriter1 != null) throw new Exception("Response writer 1 already added.");
            ResponseWriter1 = AddResponseWriter<TestResponseWriter1>(write, configAppliesTo, instanceAppliesTo);
            return this;
        }

        public RequestGraph AddResponseWriter2(Func<ResponseWriterContext, Task<HttpResponseMessage>> write,
            Func<ActionConfigurationContext, bool> configAppliesTo = null,
            Func<ResponseWriterContext, bool> instanceAppliesTo = null)
        {
            if (ResponseWriter2 != null) throw new Exception("Response writer 2 already added.");
            ResponseWriter2 = AddResponseWriter<TestResponseWriter2>(write, configAppliesTo, instanceAppliesTo);
            return this;
        }

        private T AddResponseWriter<T>(Func<ResponseWriterContext, Task<HttpResponseMessage>> write,
            Func<ActionConfigurationContext, bool> configAppliesTo,
            Func<ResponseWriterContext, bool> instanceAppliesTo)
            where T : TestResponseWriter, new()
        {
            Configuration.ResponseWriters.Append<T>(configAppliesTo);
            var writer = new T
            {
                AppliesToFunc = instanceAppliesTo,
                WriteFunc = write
            };
            ResponseWriters.Add(writer);
            return writer;
        }
    }
}
