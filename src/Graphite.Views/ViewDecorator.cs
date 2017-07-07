using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Graphite.Actions;
using Graphite.Views.Engines;
using Graphite.Views.ViewSource;
using Graphite.Writers;

namespace Graphite.Views
{
    public class ViewDecorator : IActionDecorator
    {
        private readonly List<IViewSource> _viewSources;
        private readonly ViewConfiguration _viewConfiguration;
        private readonly List<IViewEngine> _viewEngines;
        private readonly Configuration _configuration;
        private readonly HttpConfiguration _httpConfiguration;

        public ViewDecorator(List<IViewSource> viewSources,
            ViewConfiguration viewConfiguration, 
            List<IViewEngine> viewEngines,
            Configuration configuration,
            HttpConfiguration httpConfiguration)
        {
            _viewSources = viewSources;
            _viewConfiguration = viewConfiguration;
            _viewEngines = viewEngines;
            _configuration = configuration;
            _httpConfiguration = httpConfiguration;
        }

        public virtual bool AppliesTo(ActionDecoratorContext context)
        {
            return context.ActionDescriptor.Route.HasResponse;
        }

        public virtual void Decorate(ActionDecoratorContext context)
        {
            var engines = _viewEngines
                .ThatApplyTo(context.ActionDescriptor, _configuration, 
                    _viewConfiguration, _httpConfiguration)
                .SelectMany(x => x
                .SupportedViewTypes.Select(t => new
                {
                    Engine = x,
                    Type = t
                })).ToList();

            var supportedTypes = engines.SelectMany(x => 
                x.Engine.SupportedViewTypes).ToArray();
            var viewSourceContext = new ViewSourceContext(
                context.ActionDescriptor, supportedTypes);

            var result = _viewSources
                .ThatApplyTo(supportedTypes, context.ActionDescriptor, 
                    _configuration, _viewConfiguration, _httpConfiguration)
                .SelectMany(x => x.GetViews(viewSourceContext))
                .Join(engines, x => x.Type, x => x.Type, 
                    (v, e) => new
                    {
                        View = v,
                        e.Engine
                    })
                .FirstOrDefault();

            if (result == null) return;

            result.Engine.PreCompile(result.View);

            context
                .ConfigureRegistry(x => x
                    .Register(new ViewContent(result.View, result.Engine))
                    .Register<IResponseWriter, ViewWriter>())
                .ConfigureResponseWriters(x =>
                {
                    if (_viewConfiguration.ClearOtherWriters) x.Clear();
                    x.Append<ViewWriter>();
                });
        }
    }
}
