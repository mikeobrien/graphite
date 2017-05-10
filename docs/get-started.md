---
layout: documentation
---

### Get Started

You can either start out with a new empty web project or with an existing one as Graphite can coexist with your existing Web Api controllers.

If you are hosting with ASP.NET, install the following package if it is not already installed:

```
PM> Install-Package Microsoft.AspNet.WebApi.WebHost
```

Next, install Graphite and an IoC container. Currently Graphite only supports StructureMap but any IoC container can be plugged in.

```
PM> Install-Package GraphiteWeb
PM> Install-Package GraphiteWeb.StructureMap
```

Next we will bootstrap Graphite by adding the following to the `Global.asax`:

```
namespace MyWebApp{    public class Global : HttpApplication    {        protected void Application_Start(object sender, EventArgs e)        {            var configuration = GlobalConfiguration.Configuration;            configuration                .InitializeGraphite(c => c                    .EnableDiagnosticsInDebugMode()                    .UseStructureMapContainer()                    .ExcludeTypeNamespace<Global>());            configuration.EnsureInitialized();        }    }}
```

