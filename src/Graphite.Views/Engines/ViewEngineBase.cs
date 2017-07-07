namespace Graphite.Views.Engines
{
    public abstract class ViewEngineBase : IViewEngine
    {
        protected ViewEngineBase(params string[] types)
        {
            SupportedViewTypes = types;
        }

        public string[] SupportedViewTypes { get; }

        public virtual void PreCompile(View view) { }
        public abstract string Render(View view, object model);

        public virtual bool AppliesTo(ViewEngineContext context)
        {
            return true;
        }
    }
}