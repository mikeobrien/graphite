using System.Net.Http;
using Graphite.Actions;
using Graphite.Binding;
using Graphite.Routing;

namespace TestHarness.Binding
{
    public class BindingTestHandler
    {
        public class ParameterModel
        {
            public string Value { get; set; }
        }

        public ParameterModel PostToParameters(string value)
        {
            return new ParameterModel
            {
                Value = value
            };
        }

        public class FormModel
        {
            public string Form1 { get; set; }
            public int Form2 { get; set; }
        }

        public FormModel PostFormToModel(FormModel request)
        {
            return request;
        }
        
        public FormModel PostFormToParams(string form1, int form2)
        {
            return new FormModel
            {
                Form1 = form1,
                Form2 = form2
            };
        }

        public class MultiParamFormModel
        {
            public string[] Form1 { get; set; }
            public int[] Form2 { get; set; }
        }

        public MultiParamFormModel PostFormToMultiParams(string[] form1, int[] form2)
        {
            return new MultiParamFormModel
            {
                Form1 = form1,
                Form2 = form2
            };
        }

        public MultiParamFormModel PostFormToModelWithMultiParams(MultiParamFormModel request)
        {
            return request;
        }

        public class BindingModel
        {
            public string Param { get; set; }
            public string ParamByName { get; set; }
            public string ParamByAttribute { get; set; }
            public bool ParamByType { get; set; }
        }

        public BindingModel GetWithCookies(string param, string cookie1,
            [FromCookies("cookie2")] string someCookie)
        {
            return new BindingModel
            {
                Param = param,
                ParamByName = cookie1,
                ParamByAttribute = someCookie
            };
        }

        public BindingModel GetWithHeaders(string param, string header1,
            [FromHeaders("header2")] string someHeader)
        {
            return new BindingModel
            {
                Param = param,
                ParamByName = header1,
                ParamByAttribute = someHeader
            };
        }

        public BindingModel GetWithRequestInfo(string param, string remoteAddress,
            [FromRequestProperties("remotePort")] int someInfo, 
            HttpRequestMessage requestMessage)
        {
            return new BindingModel
            {
                Param = param,
                ParamByName = remoteAddress,
                ParamByAttribute = someInfo.ToString(),
                ParamByType = requestMessage != null
            };
        }

        public class ContainerBindingModel
        {
            public string Url { get; set; }
        }

        public ContainerBindingModel GetContainer(RouteDescriptor route)
        {
            return new ContainerBindingModel
            {
                Url = route.Url
            };
        }
    }
}