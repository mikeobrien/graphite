---
layout: documentation
---

### Configuration

Graphite is configured by the configuration DSL during initialization. For example:

```csharp
public class Global : HttpApplication
{
    protected void Application_Start(object sender, EventArgs e)
    {
        var configuration = GlobalConfiguration.Configuration;

        configuration.InitializeGraphite(c => c
            .EnableDiagnosticsInDebugMode()
            .UseStructureMapContainer<Registry>(configuration)
            .ExcludeTypeNamespaceFromUrl<Global>());

        configuration.EnsureInitialized();
    }
}
```
The only required configuration is specifying the IoC container to use.

The configuration DSL will be covered briefly below but in more detail in following sections. 

#### Handler/Action Assemblies

At startup, Graphite scans assemblies for handlers and actions. The currently executing assembly is automatically configured as an assembly to scan. You can add more assemblies as follows:

```csharp
configuration.InitializeGraphite(c => c
    .IncludeTypeAssembly<SomeType>()
    .IncludeTypeAssembly(typeof(SomeType))
    .IncludeAssemblies(typeof(SomeType).Assembly, ...));
```
Or you can clear the default assembly and then add others:

```csharp
configuration.InitializeGraphite(c => c
    .ClearAssemblies()
    .IncludeTypeAssembly<SomeType>());
```

#### Json.NET

You can configure Json.NET with the `ConfigureJsonNet()` DSL method as follows:

```csharp
configuration.InitializeGraphite(c => c
    .ConfigureJsonNet(s => s...));
```

The configuration is registered in the IoC container so can be taken as a dependency.

#### Diagnostics

The diagnostics page can be enabled in a few different ways. One way is to enable it when an assembly is in debug mode:

```csharp
configuration.InitializeGraphite(c => c
    .EnableDiagnosticsInDebugMode()
    .EnableDiagnosticsInDebugMode<SomeType>());
```

The first enables the diagnostics page when the calling assembly is in debug mode and the second when the specified type's assembly is.

The diagnostics page url can also be overridden as follows (the default is `_graphite`):

```csharp
configuration.InitializeGraphite(c => c
    .WithDiagnosticsAtUrl("some/other/url"));
```

#### Http Methods

By default Graphite supports the following HTTP methods: `GET`, `POST`, `PUT`, `PATCH`, `DELETE`, `OPTIONS`, `HEAD`, `TRACE` and `CONNECT`. When Graphite scans handler types for actions, it looks for methods matching the naming convention specified by the HTTP method. The default convention is the action method name starting with the pascal cased HTTP method (e.g. `^Get`, `^Post`, etc).

**In progress...**

- WithDownloadBufferSizeOf
- DisableMetrics
- WithUnhandledExceptionStatusText
- DisableDefaultErrorHandler
- UseContainer
- UseContainer<T>
- WithInitializer<T>
- WithTypeCache<T>
- WithActionInvoker<T>
- WithBehaviorChainInvoker<T>
- WithInvokerBehavior<T>

### Next: [Dependency Injection](dependency-injection)