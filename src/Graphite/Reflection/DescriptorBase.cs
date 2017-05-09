using System;
using System.Linq;

namespace Graphite.Reflection
{
    public abstract class DescriptorBase
    {
        private readonly Lazy<Attribute[]> _attributes;

        protected DescriptorBase(string name, Lazy<Attribute[]> attributes)
        {
            _attributes = attributes;
            Name = name;
        }

        public string Name { get; }
        public Attribute[] Attributes => _attributes.Value;

        public bool HasAttribute<T>() where T : Attribute
        {
            return GetAttributes<T>().Any();
        }

        public bool HasAttributes<T1, T2>() where T1 : Attribute where T2 : Attribute
        {
            return GetAttributes<T1>().Any() || GetAttributes<T2>().Any();
        }

        public T GetAttribute<T>() where T : Attribute
        {
            return GetAttributes<T>().FirstOrDefault();
        }

        public T[] GetAttributes<T>() where T : Attribute
        {
            return Attributes.OfType<T>().ToArray();
        }
    }
}