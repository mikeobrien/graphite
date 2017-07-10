using System;

namespace Graphite.Extensibility
{
    public class ConditionalPlugin<TPlugin, TContext> : Plugin<TPlugin>
    {
        private ConditionalPlugin(ConditionalPlugin<TPlugin, TContext> source) : base(source)
        {
            AppliesTo = source.AppliesTo;
        }

        private ConditionalPlugin(Type concrete, TPlugin instance, 
            Func<TContext, bool> appliesTo, bool dispose) : 
            base(concrete, instance, dispose)
        {
            AppliesTo = appliesTo;
        }

        private ConditionalPlugin(Type concrete, 
            Func<TContext, bool> appliesTo, bool singleton) : 
            base(concrete, singleton)
        {
            AppliesTo = appliesTo;
        }

        public static ConditionalPlugin<TPlugin, TContext> Create<TConcrete>(
            Func<TContext, bool> predicate, bool singleton = false)
            where TConcrete : TPlugin
        {
            return new ConditionalPlugin<TPlugin, TContext>( 
                typeof(TConcrete), predicate, singleton);
        }

        public static ConditionalPlugin<TPlugin, TContext> Create<TConcrete>(
            TConcrete instance, Func<TContext, bool> predicate, bool dispose = false)
            where TConcrete : TPlugin
        {
            return new ConditionalPlugin<TPlugin, TContext>(
                typeof(TConcrete), instance, predicate, dispose);
        }

        public Func<TContext, bool> AppliesTo { get; }

        public new ConditionalPlugin<TPlugin, TContext> Clone()
        {
            return new ConditionalPlugin<TPlugin, TContext>(this);
        }
    }
}