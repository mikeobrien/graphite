using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Extensions;
using Swank.Description;
using IApiExplorer = Swank.Description.IApiExplorer;

namespace Graphite.Swank
{
    public class GraphiteApiExplorer : IApiExplorer
    {
        private readonly Lazy<List<IApiDescription>> _descriptions;

        public GraphiteApiExplorer(RuntimeConfiguration runtimeConfiguration)
        {
            _descriptions = runtimeConfiguration.ToLazy(x => x.Actions
                .Select(d => new GraphiteApiDescription(d))
                .Cast<IApiDescription>()
                .ToList());
        }

        public IEnumerable<IApiDescription> ApiDescriptions => _descriptions.Value;
    }
}