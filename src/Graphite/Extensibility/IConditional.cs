namespace Graphite.Extensibility
{
    public interface IConditional
    {
        bool Applies();
    }

    public interface IConditional<TContext>
    {
        bool AppliesTo(TContext context);
    }
}
