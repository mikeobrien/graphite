using System.Linq;
using Graphite.Extensions;
using Graphite.Monitoring;
using Graphite.Reflection;
using Newtonsoft.Json;

namespace Graphite.Diagnostics
{
    public class ConfigurationSection : DiagnosticsSectionBase
    {
        private static readonly Configuration DefaultConfiguration = new Configuration();
        private readonly Configuration _configuration;
        private readonly JsonSerializerSettings _jsonSettings;
        private readonly Metrics _metrics;
        private readonly ITypeCache _typeCache;

        public ConfigurationSection(
            Configuration configuration, JsonSerializerSettings jsonSettings,
            Metrics metrics, ITypeCache typeCache) : 
            base("Configuration")
        {
            _configuration = configuration;
            _jsonSettings = jsonSettings;
            _metrics = metrics;
            _typeCache = typeCache;
        }

        public override string Render()
        {
            return _typeCache.GetTypeAssemblyDescriptor<ConfigurationSection>()
                .GetResourceString<DiagnosticsHandler>("Configuration.html")
                .RenderMustache(new
                    {
                        startupTime = _metrics.StartupTime,
                        totalRequests = _metrics.TotalRequests,
                        metrics = _configuration.Metrics.ToYesNoModel(),
                        container = (_configuration.Container != null
                            ? _typeCache.GetTypeDescriptor(_configuration.Container.GetType()).FriendlyFullName
                            : null).ToOptionalModel(),
                        defaultEncoding = _configuration.DefaultEncoding?.ToString().ToOptionalModel(),
                        defaultBufferSize = _configuration.DefaultBufferSize.ToSizeString(),
                        behaviorChain = _typeCache.GetTypeDescriptor(_configuration.BehaviorChain).FriendlyFullName,
                        defaultBehavior = _configuration.DefaultBehavior,

                        handlerNameConvention = _configuration.HandlerNameConvention.ToOptionalModel(DefaultConfiguration.HandlerNameConvention),
                        handlerFilter = _configuration.HandlerFilter.ToOptionalModel(DefaultConfiguration.HandlerFilter),
                        actionNameConvention = _configuration.ActionNameConvention.ToOptionalModel(DefaultConfiguration.ActionNameConvention),
                        actionFilter = _configuration.ActionFilter.ToOptionalModel(DefaultConfiguration.ActionFilter),

                        urlPrefix = _configuration.UrlPrefix.ToOptionalModel(),
                        urlAliases = _configuration.UrlAliases.ToListModel(),
                        actionSegmentsConvention = _configuration.ActionSegmentsConvention.ToOptionalModel(DefaultConfiguration.ActionSegmentsConvention),
                        automaticallyConstrainUrlParameterByType = _configuration.AutomaticallyConstrainUrlParameterByType.ToYesNoModel(),
                        querystringParameterDelimiters = _configuration.QuerystringParameterDelimiters.ToListModel(),
                        namespaceUrlMappings = _configuration.NamespaceUrlMappings.ToListModel(),

                        headersBindingMode = _configuration.HeadersBindingMode,
                        cookiesBindingMode = _configuration.CookiesBindingMode,
                        requestInfoBindingMode = _configuration.RequestInfoBindingMode,
                        continerBindingMode = _configuration.ContinerBindingMode,
                        bindComplexTypeProperties = _configuration.BindComplexTypeProperties.ToYesNoModel(),
                        createEmptyRequestParameterValue = _configuration.CreateEmptyRequestParameterValue.ToYesNoModel(),
                        failIfNoMapperFound = _configuration.FailIfNoMapperFound.ToYesNoModel(),

                        defaultBindingFailureStatusCode = _configuration.DefaultBindingFailureStatusCode,
                        defaultBindingFailureReasonPhrase = _configuration.DefaultBindingFailureReasonPhrase
                            .ToOptionalModel(DefaultConfiguration.DefaultBindingFailureReasonPhrase),

                        defaultNoReaderStatusCode = _configuration.DefaultNoReaderStatusCode,
                        defaultNoReaderReasonPhrase = _configuration.DefaultNoReaderReasonPhrase.ToOptionalModel(),

                        defaultHasResponseStatusCode = _configuration.DefaultHasResponseStatusCode,
                        defaultHasResponseReasonPhrase = _configuration.DefaultHasResponseReasonPhrase.ToOptionalModel(),

                        defaultNoResponseStatusCode = _configuration.DefaultNoResponseStatusCode,
                        defaultNoResponseReasonPhrase = _configuration.DefaultNoResponseReasonPhrase.ToOptionalModel(),

                        defaultNoWriterStatusCode = _configuration.DefaultNoWriterStatusCode,
                        defaultNoWriterReasonPhrase = _configuration.DefaultNoWriterReasonPhrase.ToOptionalModel(),

                        httpMethods = _configuration.SupportedHttpMethods.Select(x => new
                        {
                            method = x.Method,
                            allowRequestBody = x.AllowRequestBody.ToYesNoModel(),
                            allowResponseBody = x.AllowResponseBody.ToYesNoModel()
                        }),

                        serializerBufferSize = _configuration.SerializerBufferSize.ToSizeString().ToOptionalModel(),
                        disposeSerializedObjects = _configuration.DisposeSerializedObjects.ToYesNoModel(),

                        jsonSerializer = new
                        {
                            checkAdditionalContent = _jsonSettings.CheckAdditionalContent.ToYesNoModel(),
                            constructorHandling = _jsonSettings.ConstructorHandling,
                            converters = _jsonSettings.Converters.ToListModel(),  
                            culture = _jsonSettings.Culture.Name, 
                            dateFormatHandling = _jsonSettings.DateFormatHandling,  
                            dateFormatString = _jsonSettings.DateFormatString,    
                            dateParseHandling = _jsonSettings.DateParseHandling,   
                            dateTimeZoneHandling = _jsonSettings.DateTimeZoneHandling,    
                            defaultValueHandling = _jsonSettings.DefaultValueHandling,    
                            floatFormatHandling = _jsonSettings.FloatFormatHandling, 
                            floatParseHandling = _jsonSettings.FloatParseHandling,  
                            formatting = _jsonSettings.Formatting,  
                            maxDepth = _jsonSettings.MaxDepth.ToOptionalModel(),    
                            metadataPropertyHandling = _jsonSettings.MetadataPropertyHandling,    
                            missingMemberHandling = _jsonSettings.MissingMemberHandling,   
                            nullValueHandling = _jsonSettings.NullValueHandling,   
                            objectCreationHandling = _jsonSettings.ObjectCreationHandling,  
                            preserveReferencesHandling = _jsonSettings.PreserveReferencesHandling,  
                            referenceLoopHandling = _jsonSettings.ReferenceLoopHandling,   
                            stringEscapeHandling = _jsonSettings.StringEscapeHandling,    
                            typeNameHandling = _jsonSettings.TypeNameHandling

                        },

                        failIfNoAuthenticatorsApplyToAction = _configuration.FailIfNoAuthenticatorsApplyToAction.ToYesNoModel(),
                        excludeDiagnosticsFromAuthentication = _configuration.ExcludeDiagnosticsFromAuthentication.ToYesNoModel(),
                        defaultAuthenticationRealm = _configuration.DefaultAuthenticationRealm.ToOptionalModel(),
                        defaultUnauthorizedReasonPhrase = _configuration.DefaultUnauthorizedReasonPhrase.ToOptionalModel()

                    }, _typeCache);
        }
    }
}