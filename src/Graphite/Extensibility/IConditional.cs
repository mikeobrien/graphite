namespace Graphite.Extensibility
{
    public interface IConditional<TContext>
    {
        bool AppliesTo(TContext context);
    }
}
