using System;
using System.Threading.Tasks;

namespace Graphite.Extensions
{
    public static class ThreadingExtensions
    {
        public static Task<object> ConvertToObjectReturn(this Task task)
        {
            var completionSource = new TaskCompletionSource<object>();
            task.ContinueWith(t =>
            {
                if (t.IsFaulted) completionSource.TrySetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled) completionSource.TrySetCanceled();
                else completionSource.TrySetResult(null);
            }, TaskContinuationOptions.ExecuteSynchronously);
            return completionSource.Task;
        }

        public static Task<object> ConvertToObjectReturn<TResult>(this Task<TResult> task)
        {
            var completionSource = new TaskCompletionSource<object>();
            task.ContinueWith(t =>
            {
                if (t.IsFaulted) completionSource.TrySetException(t.Exception.InnerExceptions);
                else if (t.IsCanceled) completionSource.TrySetCanceled();
                else completionSource.TrySetResult(t.Result);
            }, TaskContinuationOptions.ExecuteSynchronously);
            return completionSource.Task;
        }

        public static bool IsTask(this Type type)
        {
            return type == typeof(Task);
        }

        public static bool IsTaskWithResult(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Task<>);
        }

        public static Type UnwrapTask(this Type type)
        {
            if (type == typeof(Task)) return typeof(void);
            if (type.IsTaskWithResult()) return type.GetGenericArguments()[0];
            return type;
        }

        public static Task<T> ToTaskResult<T>(this T result)
        {
            return Task.FromResult(result);
        }
    }
}
