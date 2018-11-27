---
layout: documentation
---

## Dependency Injection

Graphite is built from the ground up with dependency injection. Currently the only container supported is Structure but other containers can be integrated. To use StructureMap, install the Graphite StructureMap Nuget as follows:

```bash
PM> Install-Package GraphiteWeb.StructureMap
```

Next you will need to tell Graphite that you want to use the StructureMap container. 

```csharp
.InitializeGraphite(g => g    .UseStructureMapContainer());
```

This will set both the Web API and Graphite container. You can also specify a registry as follows:

```csharp
.InitializeGraphite(g => g    .UseStructureMapContainer<MyRegistry>());
```

Or you can configure StructureMap explicitly:

```csharp
.InitializeGraphite(g => g    .UseStructureMapContainer(x => x
        .AddRegistry<MyRegistry>(), false);
```

The last parameter indicates whether or not to set the Web API container. Lastly, you can pass in the container:

```csharp
.InitializeGraphite(g => g    .UseStructureMapContainer(new Container(), false));
```

Again, the last parameter indicates whether or not to set the Web API container.