using System;
using System.Collections.Generic;
using System.Linq;
using Graphite.Actions;
using Graphite.Extensions;

namespace Graphite.Routing
{
    public class DuplicateRouteException : Exception
    {
        public DuplicateRouteException(List<IGrouping<string, 
            ActionDescriptor>> duplicates) : base(GetMessage(duplicates)) { }

        private static string GetMessage(List<IGrouping
            <string, ActionDescriptor>> duplicates)
        {
            return "The following actions are resulting in the same url: \r\n" +
                duplicates.Select(x => $"\t{x.Key}: \r\n" +
                    x.Select(a => $"\t\t{a.Action.HandlerTypeDescriptor.Type.FullName}" +
                        $".{a.Action.MethodDescriptor.Name}\r\n").Join()).Join();
        }
    }
}
