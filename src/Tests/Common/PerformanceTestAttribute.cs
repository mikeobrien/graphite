using NUnit.Framework;

namespace Tests.Common
{
    public class PerformanceTestAttribute : CategoryAttribute
    {
        public PerformanceTestAttribute() : base("Performance") { }
    }
}
