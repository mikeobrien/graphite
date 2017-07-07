using Graphite.Extensibility;

namespace Graphite.Views.Engines
{
    public interface IViewEngine : IConditional<ViewEngineContext>
    {
        string[] SupportedViewTypes { get; }
        void PreCompile(View view);
        string Render(View view, object model);
    }
}