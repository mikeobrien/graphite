using System;

namespace Graphite.Views.ViewSource
{
    public class ViewDescriptor
    {
        public ViewDescriptor(string type, Func<string> source)
        {
            Type = type;
            Source = source;
        }

        public string Type { get; }
        public Func<string> Source { get; }
    }
}