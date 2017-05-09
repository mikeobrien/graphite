using System;
using System.Threading.Tasks;
using Graphite.Extensions;
using NUnit.Framework;
using Should;

namespace Tests.Unit.Extensions
{
    [TestFixture]
    public class TaskExtensionTests
    {
        [Test]
        public void Should_convert_task_to_object_result()
        {
            Task.CompletedTask.ConvertToObjectReturn().Result.ShouldBeNull();
        }

        [Test]
        public void Should_convert_task_result_to_object_result()
        {
            "fark".ToTaskResult().ConvertToObjectReturn().Result.ShouldEqual("fark");
        }

        [TestCase(typeof(Task), typeof(void))]
        [TestCase(typeof(Task<string>), typeof(string))]
        [TestCase(typeof(string), typeof(string))]
        public void Should_unwrap_task_types(Type type, Type expected)
        {
            type.UnwrapTask().ShouldEqual(expected);
        }

        [TestCase(typeof(Task), false)]
        [TestCase(typeof(string), false)]
        [TestCase(typeof(Task<string>), true)]
        public void Should_indicate_if_task_has_a_result(Type type, bool expected)
        {
            type.IsTaskWithResult().ShouldEqual(expected);
        }
    }
}
