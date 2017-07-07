using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Graphite.Extensions;
using Graphite.Reflection;

namespace Graphite.Views.ViewSource
{
    public abstract class ViewSourceBase : IViewSource
    {
        private static readonly View[] EmptyViews = {};
        private readonly ViewConfiguration _viewConfiguration;

        protected ViewSourceBase(ViewConfiguration viewConfiguration)
        {
            _viewConfiguration = viewConfiguration;
        }

        public virtual bool AppliesTo(ViewSourceContext context)
        {
            return context.ActionDescriptor.Route.HasResponse;
        }

        protected abstract ViewDescriptor[] GetViewDescriptors(
            ViewSourceContext context, string[] viewNames, 
            Encoding encoding);

        public virtual View[] GetViews(ViewSourceContext context)
        {
            var acceptTypes = 
                GetAttribute<ViewAcceptAttribute>(context)?.Accept ??
                _viewConfiguration.DefaultAcceptTypes.ToArray();
            var contentType = 
                GetAttribute<ViewContentTypeAttribute>(context)?.ContentType ??
                _viewConfiguration.DefaultContentType;
            var encoding = 
                GetAttribute<ViewEncodingAttribute>(context)?.Encoding ??
                _viewConfiguration.DefaultEncoding;
            
            var viewNames = GetAttribute<ViewAttribute>(context)?.Names ??
                (_viewConfiguration.ViewNameConvention ?? 
                    DefaultViewNameConvention).Invoke(context);

            return GetViewDescriptors(context, viewNames, encoding)?
                .Select(x => new View(x.Type, x.Source, acceptTypes, 
                    encoding, contentType, context.ActionDescriptor))
                .ToArray() ?? EmptyViews;
        }

        private TAttribute GetAttribute<TAttribute>(ViewSourceContext context)
            where TAttribute : Attribute
        {
            var responseType = context.ActionDescriptor.Route.ResponseType;
            var action = context.ActionDescriptor.Action;
            return responseType?.GetAttribute<TAttribute>() ??
                   action.GetActionOrHandlerAttribute<TAttribute>();
        }

        public static string[] DefaultViewNameConvention(ViewSourceContext context)
        {
            var viewNames = new List<string>();

            var respponseTypeName = context.ActionDescriptor.Route.ResponseType?.Type
                .GetNestedName().GetNonGenericName().NormalizeNestedTypeName();
            if (respponseTypeName.IsNotNullOrEmpty())
                viewNames.Add(respponseTypeName);

            viewNames.Add(context.ActionDescriptor.Action.HandlerTypeDescriptor.Type
                .GetNestedName().GetNonGenericName().NormalizeNestedTypeName());

            return viewNames.ToArray();
        }
    }
}