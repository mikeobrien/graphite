using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Hosting;

namespace Graphite.Http
{
    public class BufferPolicySelector : IHostBufferPolicySelector
    {
        public bool BufferAllInput { get; set; } = true;
        public bool BufferAllOutput { get; set; } = true;

        public List<Type> ExcludeForOutput = new List<Type>();
        public List<Type> IncludeForOutput = new List<Type>();

        public bool UseBufferedInputStream(object hostContext)
        {
            return BufferAllInput;
        }

        public bool UseBufferedOutputStream(HttpResponseMessage response)
        {
            if (ExcludeForOutput.Contains(response.Content?.GetType())) return false;
            return BufferAllOutput || IncludeForOutput.Contains(response.Content?.GetType());
        }
    }
}