---
layout: documentation
---

## Configuration

Graphite is configured by the configuration DSL during initialization. For example:

```csharp
public class Global : HttpApplication{    protected void Application_Start(object sender, EventArgs e)    {
        GlobalConfiguration.Configure(c => c            .InitializeGraphite(g => g
                // Required
                .IncludeTypeAssembly<Global>()                .UseStructureMapContainer()
                // Optional                .EnableDiagnosticsInDebugMode<Global>()                .ConfigureNamespaceUrlMapping(u => u                    .MapNamespaceAfter<Global>())));    }}
```
The only required configuration is specifying the IoC container to use and the assemblies to scan for handlers and action methods.

The configuration DSL is covered briefly below but in more detail in following sections. 

#### General Configuration

| Configuration | Description |
| --- | ----------- |
| `DisableMetrics` | Disables the built in metrics. |
| `WithInitializer` | Specifies the initializer to use. |
| `WithTypeCache` | Specifies the type cache to use. |
| `ConfigureRequestReaders` | Allows you to configure request readers. |
| `ConfigureValueMappers` | Allows you to configure value mappers. |

#### Handler/Action Configuration

| Configuration | Description |
| --- | ----------- |
| `IncludeTypeAssembly` | Includes the assembly of the specified type. This call is additive, so you can specify multiple assemblies. |
| `IncludeAssemblies` | Includes the assemblies of the specified type. |
| `ConfigureActionSources` | Allows you to configure action sources. |
| `ConfigureActionMethodSources` | Allows you to configure action method sources. |
| `FilterHandlersBy` | Configures a predicate that filters handlers. |
| `OnlyIncludeHandlersUnder<T>` | Only includes handlers under the specified type's namespace. |
| `FilterActionsBy ` | Configures a predicate that filters actions. |
| `ConfigureActionDecorators` | Allows you to configure action decorators. |
| `WithActionInvoker` | Sets the action invoker. |

#### Authentication Configuration

| Configuration | Description |
| --- | ----------- |
| `ConfigureAuthenticators` | Allows you to configure authentication. |
| `WithDefaultAuthenticationRealm` | Sets the default authentication realm. |
| `WithDefaultUnauthorizedStatusMessage` | Sets the default unauthorized status message. |
| `AllowSecureActionsWithNoAuthenticators` | Allows actions with an authentication behavior but no authenticators that apply. |

#### Behavior Configuration

| Configuration | Description |
| --- | ----------- |
| `ConfigureBehaviors` | Allows you to configure behaviors. |
| `WithBehaviorChainInvoker` | Specifies the behavior chain invoker to use. |
| `WithBehaviorChain<T>` | Specifies the behavior chain to use. |
| `WithDefaultBehavior<T>` | Specifies the last behavior in the chain. |

#### Binding Configuration

| Configuration | Description |
| --- | ----------- |
| `ConfigureRequestBinders` | Allows you to configure request binders. |
| `FailIfNoMapperFound` | Indicates that the request should fail if no value mapper found for a request value. |
| `BindHeaders` | Binds header values to action parameters. |
| `BindHeadersByNamingConvention` | Binds header values to action parameters by convention. |
| `BindHeadersByAttribute` | Binds header values to action parameters by attribute. |
| `BindCookies` | Binds cookie values to action parameters. |
| `BindCookiesByNamingConvention` | Binds cookie values to action parameters by convention. |
| `BindCookiesByAttribute` | Binds cookie values to action parameters by attribute. |
| `BindRequestInfo` | Binds request info values to action parameters. |
| `BindRequestInfoByAttribute` | Binds request info values to action parameters by convention. |
| `BindContainer` | Binds container values to action parameters. |
| `BindContainerByAttribute` | Binds container values to action parameters by attribute. |
| `BindComplexTypeProperties` | Binds request parameters to the first level of properties of a complex action parameter type. |
| `IgnoreEmptyRequestParameterValues` | Specifies that the parameter of an empty request should be left null and not set to a new instance. |

#### Container Configuration

| Configuration | Description |
| --- | ----------- |
| `UseContainer` | Specifies the IoC container to use. |
| `ConfigureRegistry` | Configures the IoC container. |

#### Diagnostics Configuration

| Configuration | Description |
| --- | ----------- |
| `EnableDiagnostics` | Enables the diagnostics page. |
| `EnableDiagnosticsInDebugMode<T>` | Enables the diagnostics page when the specified type's assembly is in debug mode. |
| `WithDiagnosticsAtUrl` | Sets the url of the diagnostics page. |
| `ExcludeDiagnosticsFromAuthentication` | Excludes the diagnostics pages from authentication. |
| `WithDiagnosticsProvider` | Set the diagnostics provider. |
| `ConfigureDiagnosticsSections` | Configures diagnostic page sections. |

#### Host Configuration

| Configuration | Description |
| --- | ----------- |
| `WithPathProvider` | Specifies the path provider to use. |
| `WithRequestPropertyProvider` | Specifies the request properties provider to use. |

#### HTTP Configuration

| Configuration | Description |
| --- | ----------- |
| `ConfigureHttpMethods` | Allows you to configure http methods. |
| `ConfigureQuerystringParameterDelimiters` | Configure querystring parameter delimiter conventions. |
| `WithAttachmentFilenameQuoting` | Determines how attachment filenames are quoted. |
| `PreserveAttachmentFilenameInnerQuotes` | Preserves attachment filename inner quotes. |

#### Response Configuration

| Configuration | Description |
| --- | ----------- |
| `ConfigureResponseWriters` | Allows you to configure response writers. |
| `ConfigureResponseStatus` | Configures response status. |
| `ConfigureResponseHeaders` | Configures response headers. |
| `WithDefaultBufferSizeOf` | Specifies the default buffer size in bytes. The default is 1MB. |
| `WithDefaultEncoding<T>` | Specifies the default encoding. |
| `WithDefaultBindingFailureStatus` | Specifies the default status code when binding fails. |
| `WithDefaultBindingFailureReasonPhrase` |  Specifies the default status text when binding fails. |
| `WithDefaultNoReaderStatus` | Specifies the default status code when no reader applies. |
| `WithDefaultNoReaderStatus` | Specifies the default status code and text when no reader applies. |
| `WithDefaultResponseStatus` | Specifies the default response status code and text. |
| `WithDefaultNoResponseStatus` | Specifies the default no response status code and text. |
| `WithDefaultNoWriterStatus` | Specifies the default status code when no writer applies. |
| `WithDefaultNoWriterStatus` | Specifies the default status code and text when no writer applies. |

#### Routing Configuration

| Configuration | Description |
| --- | ----------- |
| `ConfigureHttpRouteDecorators` | Allows you to configure HTTP route decorators. |
| `ConfigureRouteConventions` | Allows you to configure route conventions. |
| `ConfigureUrlConventions` | Allows you to configure url conventions. |
| `WithUrlAlias` | Adds url aliases. |
| `WithUrlPrefix` | Adds a prefix to all urls. Regex matching the namespace to map. |
| `ConfigureNamespaceUrlMapping` | Enables you to configure the namespace to url mappings. |
| `ConfigureNamespaceUrlMapping` --> `Add` | Adds a namespace url mapping. Supports substitutions e.g. `$1` or `${capturegroup}`. |
| `ConfigureNamespaceUrlMapping` --> `MapNamespaceAfter` | Maps the namespace starting after the namespace. |
| `WithActionSegmentsConvention` | Gets action segments. |
| `WithHttpMethodConvention` | Gets the http method of the action. |
| `WithHandlerNameConvention` | Specifies the regex used to identify handlers e.g. "Handler$". |
| `WithActionNameConvention` | Specifies the regex used to identify actions and parse action names. By default the http method is pulled from the "method" capture group, the segements from from the "segment" capture group e.g. "^(?&lt;method&gt;{methods}?)(?&lt;segments&gt;.*)". |
| `WithHttpRouteMapper` | Specifies the route mapper to use. |
| `AutomaticallyConstrainUrlParameterByType` | Automatically constrain url parameters by type. |
| `WithInlineConstraintResolver` | Specifies the inline constraint resolver to use. |
| `WithInlineConstraintBuilder` | Specifies the inline constraint builder to use. |

#### Serialization Configuration

| Configuration | Description |
| --- | ----------- |
| `ConfigureSerialization` | Allows you to configure serialization. |
| `ConfigureSerialization -> Json` | Allows you to configure Json.NET. |
| `ConfigureSerialization -> Xml` | Allows you to configure the XML serializer. |
| `ConfigureSerialization -> Xml -> Reader` | Allows you to configure the XML reader. |
| `ConfigureSerialization -> Xml -> Writer` | Allows you to configure the XML writer. |
| `ConfigureSerialization -> WithBufferSizeOf` | Specifies the serializer buffer size in bytes. |
| `ConfigureSerialization -> DisposeSerializedObjects` | Indicates that objects should be disposed after they have been serialized, if they implement IDisposable. |

#### Web API Configuration

| Configuration | Description |
| --- | ----------- |
| `ConfigureWebApi` | Allows you to configure Web Api through the Graphite DSL. |
| `ConfigureWebApi --> CreateBufferPolicy` | Creates and applies a new buffer policy selector. |
| `ConfigureWebApi --> CreateBufferPolicy --> DoNotBufferInput` | Prevents buffering of input. |
| `ConfigureWebApi --> CreateBufferPolicy --> DoNotBufferOutput` | Prevents buffering of output. |
| `ConfigureWebApi --> CreateBufferPolicy --> BufferOutputOf<T>` | Includes a specific type of content to buffer. |
| `ConfigureWebApi --> CreateBufferPolicy --> DoNotBufferOutputOf<T>` | Excludes a specific type of content from buffering. |
| `ConfigureWebApi --> CreateBufferPolicy --> ApplyWebApiDefaults` | Applies the WebApi buffering defaults. Excludes StreamContent and PushStreamContent from buffering. |
| `ConfigureWebApi --> CreateBufferPolicy --> ApplyGraphiteDefaults` | Applies the Graphite buffering defaults. Excludes AsyncContent from buffering. |
| `ConfigureWebApi --> AddExceptionLogger<T>` | Adds a request scoped exception logger. |
| `ConfigureWebApi --> SetExceptionHandler<T>` | Sets a request scoped exception handler. |
| `ConfigureWebApi --> RouteExistingFiles` | Allows you to have a url that is the same as a physical path. |

