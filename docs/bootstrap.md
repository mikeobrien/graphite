---
layout: documentation
---

### Bootstrap

Bootstrapping Graphite can be done in a few different ways. Either way the `InitializeGraphite()` extension method must be called on `HttpConfiguration` at app startup. 

First, if you are hosting with ASP.NET, you will need to install the Web Api web host package:

```bash
PM> Install-Package Microsoft.AspNet.WebApi.WebHost
```

Next you will need to install Graphite. Graphite requires an IoC container to initialize. Currently the only supported container is [StructureMap](http://structuremap.github.io/) so that will need to be installed as well.

```bash
PM> Install-Package GraphiteWeb
PM> Install-Package GraphiteWeb.StructureMap
```

You will also need to configure StructureMap as the Graphite container by calling `UseStructureMapContainer()` on the Graphite configuration DSL (This is discussed in more detail under [dependency injection](dependency-inijection) and demonstrated below).

There are a couple of ways you can bootstrap Graphite for ASP.NET hosted sites.  One is to do it in the `Global.asax` as follows:

```csharp
public class Global : HttpApplication
{
    protected void Application_Start(object sender, EventArgs e)
    {
        var configuration = GlobalConfiguration.Configuration;

        configuration.InitializeGraphite(c => c
            .UseStructureMapContainer(configuration)
            ...);

        configuration.EnsureInitialized();
    }
}
```

Another is to use [WebActivator](https://www.nuget.org/packages/WebActivatorEx/) a la: 

```csharp
[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Bootstrap), nameof(Bootstrap.Start))]

public class Bootstrap
{
    public static void Start()
    {
        var configuration = GlobalConfiguration.Configuration;

        configuration.InitializeGraphite(c => c
            .UseStructureMapContainer(configuration)
            ...);

        configuration.EnsureInitialized();
    }
}
```

Self hosted is the same idea:

```csharp
public class Startup 
{ 
    public void Configuration(IAppBuilder appBuilder) 
    { 
        var configuration = new HttpConfiguration();
         
        configuration.InitializeGraphite(c => c
            .UseStructureMapContainer(configuration)
            ...);

        appBuilder.UseWebApi(config); 
    } 
} 
```

### Next: [Configuration](configuration)