﻿using Graphite.Http;
using Newtonsoft.Json;

namespace Graphite.Readers
{
    public class JsonReader : StringReaderBase
    {
        private readonly JsonSerializerSettings _settings;

        public JsonReader(JsonSerializerSettings settings) : 
            base(MimeTypes.ApplicationJson)
        {
            _settings = settings;
        }

        protected override object GetRequest(string data, 
            RequestReaderContext context)
        {
            return JsonConvert.DeserializeObject(data, context.RequestContext
                .Route.RequestParameter.ParameterType.Type, _settings);
        }
    }
}