using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Graphite.Reflection;

namespace Graphite.Extensions
{
    // VERY simple/naive mustache implementation for diagnostics, but 
    // thats all we need and didnt want to add a Nustache dependency.
    
    // Supports:
    // - Values: {{someValue}}
    // - Blocks: {{#user}}Hello {{username}}{{/user}}
    // - Boolean blocks: {{#enabled}}This is enabled{{/enabled}}
    // - Complex object lists: {{#users}}Usernname: {{username}}{{/users}}
    // - Simple object lists: {{#timestamps}}Timestamp: {{.}}{{/timestamps}}
    // - Partials: {{> user}}

    public static class MustacheExtensions
    {
        private static readonly Func<string, object, ITypeCache, Block> ParsedTemplates = 
            Memoize.Func<string, object, ITypeCache, Block>((t, p, c) => t.ParseTemplate(p, c));

        public static string RenderMustache(this string template, object model, 
            ITypeCache typeCache, object partials = null)
        {
            return ParsedTemplates(template, partials, typeCache).ReplaceTokens(model, typeCache);
        }
        
        private static string ReplaceTokens(this Block block, object model, ITypeCache typeCache)
        {
            var result = typeCache.GetTypeDescriptor(model.GetType()).Properties
                .Where(x => block.Tokens.Any(t => t == x.Name))
                .Aggregate(block.Template, (a, i) => a.ReplaceToken(block, i, model, typeCache));
            return block.Tokens.Aggregate(result, (a, i) => a.Replace($"{{{{{i}}}}}", ""));
        }

        private static string ReplaceToken(this string template, Block block, 
            PropertyDescriptor property, object model, ITypeCache typeCache)
        {
            var value = property.GetValue(model);
            var type = value?.GetType() ?? property.PropertyType.Type;
            return template.Replace($"{{{{{property.Name}}}}}", 
                !block.Blocks.ContainsKey(property.Name)
                    ? value?.ToString()
                    : (type.Is<bool>()
                        ? ((bool)value
                            ? block.Blocks[property.Name].ReplaceTokens(model, typeCache)
                            : "")
                        : block.Blocks[property.Name].ReplaceBlock(property, value, typeCache)));
        }

        private static string ReplaceBlock(this Block block, PropertyDescriptor property, 
            object model, ITypeCache typeCache)
        {
            var type = model != null
                ? typeCache.GetTypeDescriptor(model.GetType())
                : property.PropertyType;
            return model != null
                ? (type.IsEnumerable
                    ? block.ReplaceLoop(model, typeCache)
                    : block.ReplaceTokens(model, typeCache))
                : "";
        }

        private static string ReplaceLoop(this Block block, object model, ITypeCache typeCache)
        {
            return ((IEnumerable) model).Cast<object>()
                .Aggregate("", (a, i) => a + (
                    typeCache.GetTypeDescriptor(i.GetType()).IsSimpleType
                        ? block.Template.Replace("{{.}}", i.ToString())
                        : block.ReplaceTokens(i, typeCache)));
        }
        
        private class Block
        {
            public string Template { get; set; }
            public List<string> Tokens { get; } = new List<string>(); 
            public Dictionary<string, Block> Blocks { get; } = new Dictionary<string, Block>();
        }

        private static readonly Regex BlockOpenTokenRegex = new Regex(@"\{\{\#([\w\d_]+)\}\}");
        private static readonly Regex BlockTokenRegex = new Regex(@"\{\{([\w\d_]+)\}\}");

        private static Block ParseTemplate(this string template, object partials = null, ITypeCache typeCache = null)
        {
            var block = new Block
            {
                Template = template.MergePartials(partials, typeCache)
            };

            while (true)
            {
                var token = BlockOpenTokenRegex.Match(block.Template).Groups[1].Value;

                if (token.IsNullOrEmpty()) break;
                
                var blockMatch = new Regex(
                        $@"(.*)\{{\{{#{token}\}}\}}(.*?)\{{\{{\/{token}\}}\}}(.*)", 
                        RegexOptions.Singleline)
                    .Match(block.Template);

                var beforeBlock = blockMatch.Groups[1].Value;
                var blockBody = blockMatch.Groups[2].Value;
                var afterBlock = blockMatch.Groups[3].Value;
                
                block.Template = beforeBlock + $"{{{{{token}}}}}" + afterBlock;
                block.Blocks[token] = blockBody.ParseTemplate();
            }

            block.Tokens.AddRange(BlockTokenRegex.Matches(block.Template).Cast<Match>()
                .Select(x => x.Groups[1].Value).Distinct());
            
            return block;
        }

        private static readonly Regex PartialTokenRegex = new Regex(@"\{\{\> [\w\d_]+\}\}");

        private static string MergePartials(this string template, object partials, ITypeCache typeCache)
        {
            if (partials == null) return template;
            var fullTemplate = typeCache.GetTypeDescriptor(partials.GetType())
                .Properties.Where(x => x.PropertyType.Type.Is<string>())
                .Aggregate(template, (a, i) => new Regex($@"\{{\{{\> {i.Name}\}}\}}")
                    .Replace(a, (string)i.GetValue(partials)));
            return PartialTokenRegex.Replace(fullTemplate, "");
        }
    }
}
