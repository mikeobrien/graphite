using System;
using System.Text;

namespace Graphite.Views.ViewSource
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ViewAttribute : Attribute
    {
        public ViewAttribute(params string[] names)
        {
            Names = names;
        }

        public string[] Names { get; }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ViewContentTypeAttribute : Attribute
    {
        public ViewContentTypeAttribute(string contentType)
        {
            ContentType = contentType;
        }
        
        public string ContentType { get; }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ViewEncodingAttribute : Attribute
    {
        public ViewEncodingAttribute(string encoding)
        {
            Encoding = Encoding.GetEncoding(encoding);
        }
        
        public Encoding Encoding { get; }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class ViewAcceptAttribute : Attribute
    {
        public ViewAcceptAttribute(params string[] accept)
        {
            Accept = accept;
        }

        public string[] Accept { get; }
    }
}