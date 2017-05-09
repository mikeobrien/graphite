using System;
using System.Collections;
using Graphite.Extensions;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Graphite.DependencyInjection;
using Graphite.Extensibility;
using Graphite.Monitoring;
using Graphite.Reflection;

namespace Graphite.Diagnostics
{
    public class DiagnosticsHandler
    {
        private static readonly Configuration DefaultConfiguration = new Configuration();
        private readonly RuntimeConfiguration _runtimeConfiguration;
        private readonly Configuration _configuration;
        private readonly Metrics _metrics;
        private readonly ITypeCache _typeCache;
        private readonly TrackingContainer _container;

        public DiagnosticsHandler(RuntimeConfiguration runtimeConfiguration,
            Configuration configuration, Metrics metrics, IContainer container,
            ITypeCache typeCache)
        {
            _runtimeConfiguration = runtimeConfiguration;
            _configuration = configuration;
            _metrics = metrics;
            _typeCache = typeCache;
            _container = container as TrackingContainer;
        }

        private string YesNo(bool value)
        {
            return value ? "<span class=\"green\">Yes</span>" : 
                "<span class=\"red\">No</span>";
        }

        private string SetNotSetDefault(object value, object @default)
        {
            return value == @default 
                ? "<span class=\"grey\">Default</span>"
                : value != null 
                    ? "<span class=\"green\">Custom</span>"
                    : "<span class=\"red\">Not set</span>";
        }

        private string HowManyConfigured(IEnumerable value)
        {
            var items = value.Cast<object>().ToList();
            return items.Any() 
                ? $"<span class=\"green\">{items.Count} configured</span>" 
                : "<span class=\"red\">None configured</span>";
        }

        private string RenderPlugin<T>(string name, PluginDefinition<T> definition)
        {
            return $@"<tbody>{RenderPluginRow(name, definition.Type)}</tbody>";
        }

        private string RenderPlugins<TPlugin, TContext>(string name, PluginDefinitions<TPlugin, TContext> definitions)
        {
            return $@"<tbody>{(definitions.Any()
                ? definitions.Select(x => RenderPluginRow(name, x.Type,
                    x.AppliesTo != null, definitions.Order(x), x.Singleton)).Join()
                : $"<tr><td>{name}</td><td colspan=\"5\"><span class=\"red\">None configured</span></td></tr>")}</tbody>";
        }

        private string RenderPluginRow(string name, Type type,
            bool? appliesTo = null, int? order = null, 
            bool? singleton = null)
        {
            return $@"
                <tr>
                    <td nowrap>{name}</td>
                    <td>{(order.HasValue ? $"<code>{order + 1}</code>" : "<span class=\"grey\">N/A</span>")}</td>
                    <td>{(singleton.HasValue ? YesNo(singleton.Value) : "<span class=\"grey\">N/A</span>")}</td>
                    <td>{(appliesTo.HasValue ? YesNo(appliesTo.Value) : "<span class=\"grey\">N/A</span>")}</td>
                    <td><code>{type.GetFriendlyTypeName(true).HtmlEncode()}</code></td> 
                    <td><code>{type.Assembly.GetFriendlyName().HtmlEncode()}</code></td>
                </tr>";
        }

        public string Get()
        {
            return $@"<html>
                <head>
                    <title>GRAPHITE</title>
                    <style>
                        body
                        {{ 
                            background-color: #101214;
                            font-family: Verdana, Arial, sans-serif;
                            padding: 24px 30px 30px 30px;
                        }}

                        .panels
                        {{
                            width: 100%;
                            border-spacing: 10px;
                        }}

                        .panel
                        {{
                            background: #2a2a2a;
                            color: #e2e2e2;
                            padding: 20px 30px;
                        }}

                        .title
                        {{ 
                            font-size: 18pt;
                            padding-top: 20px;
                            padding-bottom: 20px;
                        }}

                        td
                        {{
                            color: #e2e2e2;
                            font-size: 10pt;
                            vertical-align: top;
                        }}

                        .row-seperator tr:nth-child(n+2) td
                        {{
                            border-top: 1px #464646 solid;
                        }}

                        .body-seperator tbody:not(:last-child) tr:last-child td
                        {{
                            border-bottom: 1px #464646 solid;
                        }}

                        .panel table
                        {{
                            width: 100%;
                        }}

                        .panel th
                        {{
                            text-align: left;
                            color: #e2e2e2;
                            font-weight: normal;
                            font-size: 12pt;
                            border-bottom: 1px #464646 solid;
                            padding-bottom: 10px;
                        }}

                        .panel h3
                        {{
                            font-size: 14pt;
                        }}

                        .panel td
                        {{
                            padding-top: 5px;
                            padding-bottom: 5px;
                        }}

                        code
                        {{
                            color: #3eb4f9;
                        }}

                        .panel th, .panel td
                        {{
                            padding-right: 20px;
                        }}

                        .panel th, .panel td:first-child
                        {{
                            white-space: nowrap;
                        }}

                        .configuration td:last-child
                        {{
                            width: 100%;
                        }}

                        .assemblies
                        {{
                            padding-bottom: 15px;
                        }}

                        .actions > tbody > tr:first-child
                        {{ 
                            cursor: pointer; 
                            cursor: hand;
                        }}

                        .actions > tbody:not(:first-of-type) > tr:first-child > td
                        {{
                            border-top: 1px #464646 solid;
                        }}

                        .actions > tbody > tr:last-child > td
                        {{
                            padding-left: 15px;
                            padding-bottom: 15px;
                        }}

                        .actions > tbody > tr:last-child > td td:last-child
                        {{
                            width: 100%;
                        }}

                        .actions > tbody > tr:last-child > td td:first-child
                        {{
                            white-space: nowrap;
                        }}

                        .green
                        {{
                            color: #90c564;
                        }}

                        .red
                        {{
                            color: #e3524f;
                        }}

                        .grey
                        {{
                            color: #888888;
                        }}
                    </style>
                    <script>
                        function toggleVisibility(element) {{
                            var nextElement = element.nextElementSibling;
                            if (nextElement.style.display === 'none') {{
                                nextElement.style.display = '';
                            }} else {{
                                nextElement.style.display = 'none';
                            }}
                        }}
                    </script>
                </head>
                <body>

                    <table class=""panels"">

                        <tr><td colspan=""2"" class=""panel title"">Graphite v{Assembly.GetExecutingAssembly().GetName().Version}</td></tr>

                        <tr>
                            <td class=""panel"">

                                <table class=""configuration row-seperator"">
                                    <tr><td>Startup Time</td><td><code>{_metrics.StartupTime}</code></td></tr>
                                    <tr><td>Total Requests</td><td><code>{_metrics.TotalRequests}</code></td></tr>
                                    <tr><td>Default error handler enabled</td><td>{YesNo(_configuration.DefaultErrorHandlerEnabled)}</td></tr>
                                    <tr><td>Unhandled exception status text</td><td><code>{_configuration.UnhandledExceptionStatusText}</code></td></tr>
                                    <tr><td>Handler Name Filter Regex</td><td><code>{_configuration.HandlerNameFilterRegex}</code></td></tr>
                                    <tr><td>Handler Namespace Regex</td><td><code>{_configuration.HandlerNamespaceRegex}</code></td></tr>
                                    <tr><td>Url Aliases</td><td>{HowManyConfigured(_configuration.UrlAliases)}</td></tr>
                                    <tr><td>Handler Filter</td><td>{SetNotSetDefault(_configuration.HandlerFilter, DefaultConfiguration.HandlerFilter)}</td></tr>
                                    <tr><td>Action Regex</td><td>{SetNotSetDefault(_configuration.ActionRegex, DefaultConfiguration.ActionRegex)}</td></tr>
                                    <tr><td>Action Filter</td><td>{SetNotSetDefault(_configuration.ActionFilter, DefaultConfiguration.ActionFilter)}</td></tr>
                                    <tr><td>Get Handler Namespace</td><td>{SetNotSetDefault(_configuration.GetHandlerNamespace, DefaultConfiguration.GetHandlerNamespace)}</td></tr>
                                    <tr><td>Get Action Method Name</td><td>{SetNotSetDefault(_configuration.GetActionMethodName, DefaultConfiguration.GetActionMethodName)}</td></tr>
                                    <tr><td>Get Http Method</td><td>{SetNotSetDefault(_configuration.GetHttpMethod, DefaultConfiguration.GetHttpMethod)}</td></tr>
                                </table>

                            </td>
                            <td class=""panel"">
                                <h3>Enabled Http Methods</h3>

                                <table class=""http-methods row-seperator"">
                                    <thead>
                                        <tr>
                                            <th>Method</th>
                                            <th>Request</th>
                                            <th>Response</th>
                                            <th>Action Regex</th>
                                        </tr>
                                    </thead>
                                    <tbody>{_configuration.SupportedHttpMethods.Select(x =>
                                    $@"<tr>
                                            <td><code>{x.Method}</code></td>
                                            <td>{YesNo(x.AllowRequestBody)}</td>
                                            <td>{YesNo(x.AllowResponseBody)}</td>
                                            <td><code>{x.ActionRegex.HtmlEncode()}</code></td>
                                        </tr>").Join()}
                                    </tbody>
                                </table>

                            </td>
                        </tr>

                        <tr>
                            <td colspan=""2"" class=""panel"">
                                <h3>Plugins</h3>
                                <table class=""plugins body-seperator"">
                                    <thead>
                                        <tr>
                                            <th>Plugin Type</th>
                                            <th>Order</th>
                                            <th>Singleton</th>
                                            <th nowrap>Applies To</th>
                                            <th>Type</th>
                                            <th width=""100%"">Assembly</th>
                                        </tr>
                                    </thead>
                                    {RenderPlugin("Initializer", _configuration.Initializer)}
                                    {RenderPlugin("Type Cache", _configuration.TypeCache)}
                                    {RenderPlugins("Action Method Source", _configuration.ActionMethodSources)}
                                    {RenderPlugins("Action Source", _configuration.ActionSources)}
                                    {RenderPlugins("Route Convention", _configuration.RouteConventions)}
                                    {RenderPlugins("Url Convention", _configuration.UrlConventions)}
                                    {RenderPlugin("Behavior Chain Invoker", _configuration.BehaviorChainInvoker)}
                                    {RenderPlugins("Behavior", _configuration.Behaviors)}
                                    {RenderPlugin("Invoker Behavior", _configuration.InvokerBehavior)}
                                    {RenderPlugin("Action Invoker", _configuration.ActionInvoker)}
                                    {RenderPlugins("Request Binder", _configuration.RequestBinders)}
                                    {RenderPlugins("Request Reader", _configuration.RequestReaders)}
                                    {RenderPlugins("Parameter Mapper", _configuration.ValueMappers)}
                                    {RenderPlugins("Response Writer", _configuration.ResponseWriters)}
                                </table>

                            </td>
                        </tr>

                        <tr>
                            <td colspan=""2"" class=""panel"">
                                <h3>Container</h3>
                                <table class=""container row-seperator"">
                                    <thead>
                                        <tr>
                                            <th>Plugin Type</th>
                                            <th>Plugin Assembly</th>
                                            <th>Concrete Type</th>
                                            <th>Singleton</th>
                                            <th>Instance</th>
                                            <th width=""100%"">Concrete Assembly</th>
                                        </tr>
                                    </thead>
                                    <tbody>
                                    {_container?.Registry.OrderBy(x => x.PluginType.FullName).Select(x => 
                                        { 
                                            var pluginType = _typeCache.GetTypeDescriptor(x.PluginType);
                                            var concreteType = x.ConcreteType != null ? _typeCache.GetTypeDescriptor(x.ConcreteType) : null;
                                                                                                      
                                            return $@"
                                                <tr>
                                                    <td nowrap><code>{pluginType.FriendlyFullName}</code></td>
                                                    <td nowrap><code>{pluginType.Type.Assembly.GetFriendlyName().HtmlEncode()}</code></td>
                                                    <td><code>{concreteType?.FriendlyFullName}</code></td>
                                                    <td>{YesNo(x.Singleton)}</td> 
                                                    <td>{YesNo(x.IsInstance)}</td> 
                                                    <td nowrap><code>{concreteType?.Type.Assembly.GetFriendlyName().HtmlEncode()}</code></td>
                                                </tr>   
                                            ";
                                        }).Join()}
                                    </tbody>

                                </table>

                            </td>
                        </tr>

                        <tr>
                            <td colspan=""2"" class=""panel"">
                                <h3>Actions</h3>

                                <table class=""assemblies"">
                                    <tr>
                                        <th>Source Assembiles</th>
                                    </tr>
                                    {_configuration.Assemblies.Select(x => $"<tr><td><code>{x.FullName.HtmlEncode()}</code></td></tr>").Join()}
                                </table>

                                <table class=""actions"">
                                    <thead>
                                        <tr>
                                            <th colspan=""2"">Url</th>
                                            <th></th>
                                            <th>Action</th>
                                            <th width=""100%"">Assembly</th>
                                            <th nowrap>Average Time</th>
                                        </tr>
                                    </thead>
                                    {_runtimeConfiguration.Actions.OrderBy(x => x.Route.Url).Select(x => $@"
                                    <tbody>
                                        <tr onclick=""toggleVisibility(this)"">
                                            <td><code>{x.Route.Method}</code></td>
                                            <td><code>/{new Regex(@"(\{\w*\})").Replace(x.Route.Url.HtmlEncode(), "<span class=\"url-parameter\">$1</span>")}</code></td>
                                            <td>&rarr;</td>
                                            <td><code>{x.Action.FullName}</code></td>
                                            <td width=""100%""><code>{x.Action.Method.DeclaringType.Type.Assembly.GetFriendlyName().HtmlEncode()}</code></td>
                                            <td><code>{_metrics.GetAverageRequestTime(x)}</code></td>
                                        </tr>
                                        <tr style=""display: none"">
                                            <td colspan=""6"">
                                                <table>
                                                    <tr>
                                                        <td>Behaviors</td>
                                                        <td>{(!x.Behaviors.Any() ? "<span class=\"red\">None</span>" :
                                                            x.Behaviors.Select(b => $@"<code>{b.FriendlyFullName.HtmlEncode()}</code>")
                                                                .Join(" &rarr; "))}</td>
                                                    </tr>
                                                    <tr>
                                                        <td>Request Parameter</td>
                                                        <td>{(x.Route.RequestParameter == null ? "<span class=\"red\">None</span>" :
                                                            $@"<code class=""red"">{x.Route.RequestParameter.Name}</code>:<code>{x.Route.RequestParameter
                                                                .ParameterType.FriendlyFullName.HtmlEncode()}</code>")}</td>
                                                    </tr>
                                                    <tr>
                                                        <td>Querystring Parameters</td>
                                                        <td>{(!x.Route.QuerystringParameters.Any() ? "<span class=\"red\">None</span>" :
                                                            x.Route.QuerystringParameters.Select(p => $@"<code class=""red"">{p.Name}</code>:<code>{p.ParameterType
                                                            .FriendlyFullName.HtmlEncode()}</code>").Join(", "))}</td>
                                                    </tr>
                                                    <tr>
                                                        <td>Url Parameters</td>
                                                        <td>{(!x.Route.UrlParameters.Any() ? "<span class=\"red\">None</span>" :
                                                            x.Route.UrlParameters.Select(p => $@"<code class=""red"">{p.Name}</code>:<code>{p.ParameterType
                                                            .FriendlyFullName.HtmlEncode()}</code>").Join(", "))}</td>
                                                    </tr>
                                                    <tr>
                                                        <td>Response Type</td>
                                                        <td>{(x.Route.ResponseType == null ? "<span class=\"red\">None</span>" :
                                                            $"<code>{x.Route.ResponseType.FriendlyFullName.HtmlEncode()}</code>")}</td>
                                                    </tr>
                                                </table>
                                            </td>
                                        </tr>
                                    </tbody>").Join()}
                                </table>
     
                             </td>
                        </tr>
                    </table>

                </body>
            </html>";
        }
    }
}