using System.Collections;
using System.Collections.Generic;
using Graphite.Extensions;
using Graphite.Reflection;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Extensions
{
    [TestFixture]
    public class MustacheExtensionTests
    {
        [TestCase("{{value}}", "fark")]
        [TestCase("farker {{value}}", "farker fark")]
        [TestCase("{{value}} farker", "fark farker")]
        [TestCase("-{{value}} farker", "-fark farker")]
        [TestCase("farker {{value}}-", "farker fark-")]
        public void Should_render_simple_template(string template, string expected)
        {
            template.RenderMustache(new { value = "fark" }, new TypeCache())
                .ShouldEqual(expected);
        }
        
        [TestCase("{{#model}}{{value}}{{/model}}", 
            "nested")]
        public void Should_render_complex_block(string template, string expected)
        {
            template.RenderMustache(new
                {
                    model = new
                    {
                        value = "nested"
                    }
                }, new TypeCache())
                .ShouldEqual(expected);
        }
        
        [TestCase("{{#isTrue}}yay {{value}}{{/isTrue}}{{#isFalse}}aw {{value}}{{/isFalse}}", "yay ")]
        public void Should_render_boolean_block(string template, string expected)
        {
            template.RenderMustache(new
                {
                    isTrue = true,
                    isFalse = false
                }, new TypeCache())
                .ShouldEqual(expected);
        }
        
        [TestCase("{{#list}}{{value}}{{/list}}", "nested1nested2")]
        [TestCase("{{#list}}{{value}}\r\n{{/list}}", "nested1nested2")]
        [TestCase("{{#list}}" +
                        "{{value}}" +
                        "{{#nestedValues}}" +
                            "{{value}}" +
                        "{{/nestedValues}}" +
                  "{{/list}}",
            "nested1nestedNested1nestedNested2nested2nestedNested3nestedNested4")]
        public void Should_render_complex_list(string template, string expected)
        {
            template.RenderMustache(new
                {
                    list = new ArrayList
                    {
                        new
                        {
                            value = "nested1",
                            nestedValues = new ArrayList
                            {
                                new { value = "nestedNested1" },
                                new { value = "nestedNested2" }
                            }
                        },
                        new
                        {
                            value = "nested2",
                            nestedValues = new ArrayList
                            {
                                new { value = "nestedNested3" },
                                new { value = "nestedNested4" }
                            }
                        }
                    } as object
                }, new TypeCache())
                .ShouldEqual(expected);
        }
        
        [TestCase("{{#simpleValues}}{{.}}{{/simpleValues}}", "abc")]
        public void Should_render_simple_list(string template, string expected)
        {
            template.RenderMustache(new
                {
                    simpleValues = new ArrayList { "a", "b", "c" } as object
                }, new TypeCache())
                .ShouldEqual(expected);
        }

        [TestCase(null, "")]
        [TestCase(5, "5")]
        public void Should_conditionally_render_nullable_types(int? value, string expected)
        {
            "{{#value}}{{.}}{{/value}}".RenderMustache(new
                {
                    value
                }, new TypeCache())
                .ShouldEqual(expected);
        }

        public class Model
        {
            public string Value { get; set; }
        }

        [Test]
        public void Should_not_render_null_block()
        {
            "{{#nullModel}}{{Value}}{{/nullModel}}{{#nullList}}{{Value}}{{/nullList}}".RenderMustache(new
            {
                nullModel = (Model)null,
                nullList = (IEnumerable<Model>)null
            }, new TypeCache()).ShouldEqual("");
        }

        [Test]
        public void Should_remove_unmatched_tokens()
        {
            "{{Value}}".RenderMustache(new
                { }, new TypeCache()).ShouldEqual("");
        }

        [Test]
        public void Should_include_partial()
        {
            "{{value}}{{> partial1}}{{> partial2}}".RenderMustache(new
            {
                value = "fark"
            }, new TypeCache(), 
            new
            {
                partial1 = "-{{value}}"
            }).ShouldEqual("fark-fark");
        }
    }
}
