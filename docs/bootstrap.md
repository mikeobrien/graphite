---
layout: documentation
---

### Bootstrap

Bootstrapping Graphite can be done in a few different ways. Either way the `InitializeGraphite()` extension method must be called on `HttpConfiguration` at app startup. 

First, if you are hosting with ASP.NET, you will need to install the Web Api web host package:

```bash
PM> Install-Package Microsoft.AspNet.WebApi.WebHost
```

If you are self hosting with OWIN:

```bash
PM> Install-Package Microsoft.AspNet.WebApi.OwinSelfHost
```

Next you will need to install Graphite. Graphite requires an IoC container to initialize. Currently the only supported container is [StructureMap](http://structuremap.github.io/) so that will need to be installed as well.

```bash
PM> Install-Package GraphiteWeb
PM> Install-Package GraphiteWeb.StructureMap
```

You will also need to configure StructureMap as the Graphite and Web Api container by calling `UseStructureMapContainer()` on the Graphite configuration DSL (This is discussed in more detail under [dependency injection](dependency-inijection) and demonstrated below).

There are a couple of ways you can bootstrap Graphite for ASP.NET hosted sites.  One is to do it in the `Global.asax` as follows:

```csharp
public class Global : HttpApplication
{
    protected void Application_Start(object sender, EventArgs e)
    {
        GlobalConfiguration.Configure(c => c            .InitializeGraphite(g => g                .UseStructureMapContainer()
                ...));
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
        GlobalConfiguration.Configure(c => c            .InitializeGraphite(g => g                .UseStructureMapContainer()
                ...));
    }
}
```

OWIN bootstrapping can be done in a startup class, a la:

```csharp
public class Program{    static void Main(string[] args)    {        WebApp.Start<Startup>(args[0]);        Console.WriteLine($"Server running at {args[0]}, press enter to exit.");        Console.ReadLine();    }}public class Startup{    public void Configuration(IAppBuilder appBuilder)    {        appBuilder.InitializeGraphite(g => g            .UseStructureMapContainer()            ...;
    }}
```

### Next: [Configuration](configuration)