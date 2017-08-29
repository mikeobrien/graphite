using System.Threading.Tasks;
using Graphite.Extensibility;

namespace Graphite.Readers
{
    public interface IRequestReader : IConditional
    {
        Task<ReadResult> Read();
    }
}