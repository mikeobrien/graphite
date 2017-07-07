using RazorEngine.Templating;

namespace Graphite.Views.Engines
{
    public class RazorEngine : ViewEngineBase
    {
        public static readonly string Type = "cshtml";
        private readonly IRazorEngineService _razor;
        
        public RazorEngine(IRazorEngineService razor) : base(Type)
        {
            _razor = razor;
        }

        public override void PreCompile(View view)
        {
            if (!_razor.IsTemplateCached(view.Hash, view.ModelType.Type))
                _razor.Compile(view.Source, view.Hash, view.ModelType.Type);
        }

        public override string Render(View view, object model)
        {
            return _razor.Run(view.Hash, view.ModelType.Type, model);
        }
    }
}