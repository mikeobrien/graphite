using System;

namespace Graphite.Cors
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class OverrideCorsAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CorsExposedHeadersAttribute : Attribute
    {
        public CorsExposedHeadersAttribute(params string[] headers)
        {
            Headers = headers;
        }

        public string[] Headers { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CorsAllowedHeadersAttribute : Attribute
    {
        public CorsAllowedHeadersAttribute(params string[] headers)
        {
            Headers = headers;
        }

        public string[] Headers { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CorsAllowedMethodsAttribute : Attribute
    {
        public CorsAllowedMethodsAttribute(params string[] methods)
        {
            Methods = methods;
        }

        public string[] Methods { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CorsAllowedOriginsAttribute : Attribute
    {
        public CorsAllowedOriginsAttribute(params string[] origins)
        {
            Origins = origins;
        }

        public string[] Origins { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CorsAttribute : Attribute
    {
        public CorsAttribute(bool allowAnyHeader = false, 
            bool allowAnyMethod = false, 
            bool allowAnyOrigin = false, 
            long preflightMaxAge = -1, 
            bool supportsCredentials = false,
            bool allowOptionRequestsToPassThrough = false,
            bool allowRequestsWithoutOriginHeader = true,
            bool allowRequestsThatFailCors = true)
        {
            AllowAnyHeader = allowAnyHeader;
            AllowAnyMethod = allowAnyMethod;
            AllowAnyOrigin = allowAnyOrigin;
            PreflightMaxAge = preflightMaxAge;
            SupportsCredentials = supportsCredentials;
            AllowOptionRequestsToPassThrough = allowOptionRequestsToPassThrough;
            AllowRequestsWithoutOriginHeader = allowRequestsWithoutOriginHeader;
            AllowRequestsThatFailCors = allowRequestsThatFailCors;
        }

        public bool AllowAnyHeader { get; }
        public bool AllowAnyMethod { get; }
        public bool AllowAnyOrigin { get; }
        public long PreflightMaxAge { get; }
        public bool SupportsCredentials { get; }
        public bool AllowOptionRequestsToPassThrough { get; }
        public bool AllowRequestsWithoutOriginHeader { get; }
        public bool AllowRequestsThatFailCors { get; }
    }
}