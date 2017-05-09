using System.Collections;
using System.Collections.Generic;

namespace Tests.Common
{
    public class Logger : IEnumerable<object>
    {
        private readonly List<object> _entries = new List<object>();

        public void Write(object value)
        {
            _entries.Add(value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerator<object> GetEnumerator()
        {
            return _entries.GetEnumerator();
        }
    }
}
