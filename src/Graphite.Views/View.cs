using System;
using System.Text;
using Graphite.Actions;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.Views
{
    public class View
    {
        private readonly Lazy<string> _source;
        private readonly Lazy<string> _hash;

        public View(string type, Func<string> source, string[] acceptTypes,
            Encoding encoding, string contentType, ActionDescriptor action)
        {
            _source = new Lazy<string>(source);
            Type = type;
            Action = action;
            AcceptTypes = acceptTypes;
            ModelType = action.Route.ResponseType;
            Encoding = encoding;
            ContentType = contentType;
            _hash = new Lazy<string>(() => _source.Value.Hash());
        }

        public string Source => _source.Value;
        public string Type { get; }
        public string[] AcceptTypes { get; }
        public Encoding Encoding { get; }
        public string ContentType { get; }
        public TypeDescriptor ModelType { get; }
        public string Hash => _hash.Value;
        public ActionDescriptor Action { get; }
    }
}