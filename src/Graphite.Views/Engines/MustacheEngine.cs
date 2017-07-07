using System;
using System.IO;
using Graphite.Extensions;
using Nustache.Compilation;
using Nustache.Core;

namespace Graphite.Views.Engines
{
    public class MustacheEngine : ViewEngineBase
    {
        public static readonly string Type = "mustache";

        private static readonly Func<string, string, Type, Func<object, string>> TemplateCache =
            Memoize.Func<string, string, Type, Func<object, string>>(CompileTemplate);

        public MustacheEngine() : base(Type) { }

        public override void PreCompile(View view)
        {
            TemplateCache(GetTemplateKey(view), view.Source, view.ModelType.Type);
        }

        public override string Render(View view, object model)
        {
            return TemplateCache(GetTemplateKey(view), view.Source, 
                view.ModelType.Type)(model);
        }

        private string GetTemplateKey(View view)
        {
            return $"{view.Hash}-{view.ModelType.Type.FullName}";
        }

        private static Func<object, string> CompileTemplate(
            string source, Type modelType)
        {
            var template = new Template();
            template.Load(new StringReader(source));
            return template.Compile(modelType, null);
        }
    }
}