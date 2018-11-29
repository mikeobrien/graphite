---
layout: documentation
---

### Serialization

By default, Graphite uses [Json.NET](https://www.newtonsoft.com/json) for JSON and the [BCL `XmlSerializer`](https://docs.microsoft.com/en-us/dotnet/api/system.xml.serialization.xmlserializer) for XML serialization. These can be configured right from the Graphite configuration, for example:

```csharp
.InitializeGraphite(g => g    .ConfigureSerialization(x => x        .WithBufferSizeOf(32 * 1024)        .DisposeSerializedObjects()        .Json(j => j            .IgnoreNullValues()            .IgnoreCircularReferences()            .ConfigureIsoDateTimeConverter(d =>            {                d.DateTimeStyles = DateTimeStyles.AdjustToUniversal |                                   DateTimeStyles.AssumeUniversal;                d.AdjustToLocalAfterDeserializing();            })            .SerializeIpAddresses()            .WriteNonNumericFloatsAsDefault()            .WriteEnumNames()            .FailOnUnmatchedElements())
        .Xml(x => x
            .Reader(r => r
                .IgnoreWhitespace = true)
            .Writer(w => w
                .Indent = true)));
```

The configuration is registered with the container and is available to take as a dependency. 

#### General Configuration

There are two options that apply to both Json.NET and the BCL `XmlSerializer`. These can also be used to configure other integrated serializers. 

##### Write Buffer Size

The first is the buffer size. This specifies the buffer size of the StreamWriter that both serializers write too.

```csharp
.InitializeGraphite(g => g    .ConfigureSerialization(x => x        .WithBufferSizeOf(32 * 1024));
```
##### Disposing Serialized Objects

The second option is disposal of serialized objects. This option instructs Graphite to automatically dispose objects right after they are serialized, if they implement `IDisposable`.

```csharp
.InitializeGraphite(g => g    .ConfigureSerialization(x => x        .DisposeSerializedObjects());
```

#### Json.NET Configuration

You can configure Json.NET as follows:

```csharp
.InitializeGraphite(g => g    .ConfigureSerialization(x => x        .Json(j => j            .ConfigureIsoDateTimeConverter(d =>            {                d.DateTimeStyles = DateTimeStyles.AdjustToUniversal |                                   DateTimeStyles.AssumeUniversal;                d.AdjustToLocalAfterDeserializing();            });
```

This continuation is passed a [`JsonSerializerSettings` object](https://www.newtonsoft.com/json/help/html/T_Newtonsoft_Json_JsonSerializerSettings.htm) that is registered with the IoC container and can be taken as a dependency. You can configure it directly or use some of the fluent Graphite convenience methods.


| Configuration | Description |
| --- | ----------- |
| `SerializeIpAddresses` | Adds an `IPAddress` converter. |
| `TryAddConverter` | Adds a converter if it does not already exist and allows you to configure it. |
| `AddConverter` | Adds a converter and allows you to configure it. |
| `RemoveConverters` | Removes all converters of the specified type. |
| `WriteEnumNames` | Serializes `enum` names instead of values. |
| `WriteNonNumericFloatsAsDefault` | Serializes non numeric floats (`NaN`) as the default value `0`. |
| `UseCamelCaseNaming` | Serializes with camelCase naming. |
| `IgnoreCircularReferences` | Prevents attempted serialization of circular references. |
| `IgnoreNullValues` | Prevents serialization of `null` values. |
| `FailOnUnmatchedElements` | Fails when deserializing a JSON field and no corresponding model member exists. |
| `PrettyPrintInDebugMode<T>` | Pretty prints JSON when the assembly of the specified type is in debug mode (i.e. not optimized). |

##### ISO Datetimes

You can add and or configure the IsoDateTimeConverter as follows:

```csharp
.InitializeGraphite(g => g    .ConfigureSerialization(x => x        .Json(j => j            .ConfigureIsoDateTimeConverter(d =>            {                d.DateTimeStyles = DateTimeStyles.AdjustToUniversal |                                   DateTimeStyles.AssumeUniversal;                d.AdjustToLocalAfterDeserializing();            });
```

Graphite adds the ability to adjust to local time after deserializing with the `d.AdjustToLocalAfterDeserializing()` method. 
	
##### Microsoft Datetime Format

You can serialize dates in the Microsoft format as follows:
	
```csharp
.InitializeGraphite(g => g    .ConfigureSerialization(x => x        .Json(j => j            .WriteMicrosoftJsonDateTime(d => d                .AdjustToLocalAfterDeserializing()
                .AdjustToUtcBeforeSerializing());
```

Graphite adds the ability to adjust to local time after deserializing with the `d.AdjustToLocalAfterDeserializing()` method. It also adds the ability to adjust to UTC before serializing with the `AdjustToUtcBeforeSerializing ` method.

### Custom Serialization

Graphite serialization is accomplished with response writers. You can read more about response writers in the Response Writers section but we'll cover it here briefly in the context of serialization. Lets take a look at the Json.NET writer:

```csharp
public class MyJsonWriter : SerializerWriterBase{    private readonly JsonSerializer _serializer;    private readonly Configuration _configuration;    public MyJsonWriter(JsonSerializer serializer,        HttpRequestMessage requestMessage,        HttpResponseMessage responseMessage,        Configuration configuration) :        base(requestMessage, responseMessage,            configuration, MimeTypes.ApplicationJson)    {        _serializer = serializer;        _configuration = configuration;    }
        public override bool AppliesTo(ResponseWriterContext context)    {        return base.AppliesTo(context);    }
        protected override void WriteToStream(ResponseWriterContext context, Stream output)    {        using (var streamWriter = output.CreateWriter(_configuration.DefaultEncoding,            _configuration.SerializerBufferSize, true))        using (var jsonWriter = new JsonTextWriter(streamWriter))        {            _serializer.Serialize(jsonWriter, context.Response);            jsonWriter.Flush();        }    }}
```

- Response writers must implement `IResponseWriter` but it's better to simply inherit from `SerializerWriterBase` as this handles a lot of things required for serialization in general. You can pass the mime type to the base class as demonstrated above as well as a few other dependencies. 
- If you want to only apply the writer to certain requests you can override `AppliesTo` and add your own logic. You will want to include the result of the base class `AppliesTo` as the base classes contain additional required logic. 
- Next you'll need to serialize the response object, `context.Response`, and write the serialized results to the output stream. 
- It's good to apply the configuration settings (i.e. encoding and buffer size) as demonstrated above so your writer can be configured rather than hardcoded. 
- You'll also want to configure your writer not to close the underlying stream.

Next you'll need to register your writer as follows:

```csharp
.InitializeGraphite(g => g    .ConfigureResponseWriters(x => x
        // Append after existing writers
        .Append<MyJsonWriter>()
        
        // Prepend to existing writers        .Prepend<MyJsonWriter>().Before<JsonWriter>().OrAppend()
        
        // Replace the existing json writer with your own        .Replace<JsonWriter>().With<MyJsonWriter>().OrAppend()));
```

There are a number of ways to register the writer, a few are shown above. The first one simply appends it to the existing writers. The problem with this is that the stock json writer will match before yours and be used instead. The second example prepends it before the stock writer so that it has a chance to run before the stock one but doesn't override writers before it. This would make sense if your custom writer only handles some requests but you want all the rest to be handled by the stock json writer. The last one replaces the stock json writer entirely.

### Custom Deserialization

Graphite deserialization is accomplished with request readers. You can read more about request readers in the Request Readers section but we'll cover it here briefly in the context of deserialization. Lets take a look at the Json.NET reader:

```csharp
public class MyJsonWriter : SerializerWriterBase{    private readonly JsonSerializer _serializer;    private readonly Configuration _configuration;    public MyJsonWriter(JsonSerializer serializer,        HttpRequestMessage requestMessage,        HttpResponseMessage responseMessage,        Configuration configuration) :        base(requestMessage, responseMessage,            configuration, MimeTypes.ApplicationJson)    {        _serializer = serializer;        _configuration = configuration;    }
        public override bool AppliesTo(ResponseWriterContext context)    {        return base.AppliesTo(context);    }
        protected override void WriteToStream(ResponseWriterContext context, Stream output)    {        using (var streamWriter = output.CreateWriter(_configuration.DefaultEncoding,            _configuration.SerializerBufferSize, true))        using (var jsonWriter = new JsonTextWriter(streamWriter))        {            _serializer.Serialize(jsonWriter, context.Response);            jsonWriter.Flush();        }    }}
```



