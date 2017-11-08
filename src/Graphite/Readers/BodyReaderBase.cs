using System.Threading.Tasks;
using Graphite.Binding;

namespace Graphite.Readers
{
    public abstract class BodyReaderBase<T, TWrapper> : IRequestReader 
        where TWrapper : InputBody<T>
    {
        public bool AppliesTo(ReaderContext context)
        {
            return context.ReadType != null &&
                (context.ReadType.Type == typeof(T) || 
                context.ReadType.Type == typeof(TWrapper));
        }

        protected abstract Task<T> GetData(ReaderContext content);
        protected abstract TWrapper GetResult(ReaderContext context, object data);

        public async Task<ReadResult> Read(ReaderContext context)
        {
            var data = await GetData(context);
            if (context.ReadType.Type == typeof(T)) return ReadResult.Success(data);
            return ReadResult.Success(GetResult(context, data));
        }
    }
}