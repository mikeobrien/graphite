using System.Text;
using Graphite.Views.Engines;

namespace Graphite.Views
{
    public class ViewContent
    {
        private readonly View _view;
        private readonly IViewEngine _engine;

        public ViewContent(View view, IViewEngine engine)
        {
            _view = view;
            _engine = engine;
        }

        public Encoding Encoding => _view.Encoding;
        public string[] AcceptTypes => _view.AcceptTypes;

        public string Render(object model) => 
            _engine.Render(_view, model);
    }
}