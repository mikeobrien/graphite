using System.Reflection;
using System.Text;
using Graphite.Extensions;

namespace Graphite.Reflection
{
    public class AssemblyResource
    {
        private readonly Assembly _assembly;

        public AssemblyResource(string name, Assembly assembly)
        {
            _assembly = assembly;
            Name = name;
        }

        public string Name { get; }

        public string GetString(Encoding encoding = null)
        {
            using (var stream = _assembly.GetManifestResourceStream(Name))
            {
                return stream.ReadToEnd(encoding);
            }
        }
    }
}