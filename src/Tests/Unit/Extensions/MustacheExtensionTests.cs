using System;
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

        [TestCase("{{value}}", "fark")]
        [TestCase("{{value}}{{#list}}{{value}}{{/list}}", "farknested1nested2")]
        [TestCase("{{value}}{{#list}}{{value}}\r\n{{/list}}", "farknested1\r\nnested2\r\n")]
        [TestCase("{{#simpleValues}}{{.}}{{/simpleValues}}", "abc")]
        [TestCase("{{#isTrue}}yay {{value}}{{/isTrue}}{{#isFalse}}aw {{value}}{{/isFalse}}", "yay fark")]
        [TestCase("{{value}}" +
                  "{{#list}}" +
                        "{{value}}" +
                        "{{#nestedValues}}" +
                            "{{value}}" +
                        "{{/nestedValues}}" +
                  "{{/list}}" +
                  "{{#model}}{{value}}{{/model}}", 
            "farknested1nestedNested1nestedNested2nested2nestedNested3nestedNested4nested")]
        public void Should_render_nested_template(string template, string expected)
        {
            template.RenderMustache(new
                {
                    value = "fark",
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
                    } as object,
                    model = new
                    {
                        value = "nested"
                    },
                    simpleValues = new ArrayList { "a", "b", "c" } as object,
                    isTrue = true,
                    isFalse = false
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
