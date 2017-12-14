using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Graphite.Extensibility;
using Graphite.Extensions;
using Graphite.Http;
using Graphite.Linq;
using Tests.Common.Fakes;

namespace Tests.Common
{
    public class List<TKey, TValue> : List<KeyValuePair<TKey, TValue>>
    {
        public ILookup<TKey, TValue> ToLookup()
        {
            return this.ToLookup(x => x.Key, x => x.Value);
        }
    }

    public static class Extensions
    {
        public static DateTime SubtractUtcOffset(this DateTime date)
        {
            return date.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(date).Hours);
        }

        public static bool IsType<T>(this Type type)
        {
            return type == typeof(T);
        }

        public static void Add<TKey, TValue>(
            this List<KeyValuePair<TKey, TValue>> source, 
            TKey key, TValue value)
        {
            source.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public static string ReadAllText(this Stream stream)
        {
            using (stream)
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static void Clear(this HttpConfiguration configuration)
        {
            configuration.Routes.Clear();
            configuration.DependencyResolver = new EmptyResolver();
        }

        public static TimeSpan Elapsed(this Action action)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            action();
            stopwatch.Stop();
            return stopwatch.Elapsed;
        }

        public static long TicksSinceEpoch(this DateTime datetime)
        {
            return (datetime.Ticks - 621355968000000000 - 
                TimeZone.CurrentTimeZone.GetUtcOffset(datetime).Ticks) / 10000;
        }

        public static void Times(this int iterations, Action action)
        {
            for (var i = 0; i < iterations; i++)
            {
                action();
            }
        }

        public static void TimesParallel(this int iterations, Action action)
        {
            1.To(iterations).AsParallel().ForEach(x => action());
        }

        public static T Second<T>(this IEnumerable<T> source)
        {
            return source.Skip(1).FirstOrDefault();
        }

        public static T Third<T>(this IEnumerable<T> source)
        {
            return source.Skip(2).FirstOrDefault();
        }

        public static T Fourth<T>(this IEnumerable<T> source)
        {
            return source.Skip(3).FirstOrDefault();
        }

        public static T Fifth<T>(this IEnumerable<T> source)
        {
            return source.Skip(4).FirstOrDefault();
        }

        public static Task<HttpResponseMessage> SendAsync(this HttpMessageHandler handler,
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return (Task<HttpResponseMessage>) handler.GetType()
                .GetMethod("SendAsync", BindingFlags.Instance | 
                        BindingFlags.NonPublic).Invoke(handler, 
                    new object[] { request, cancellationToken });
        }

        public static T WaitForResult2<T>(this Task<T> task)
        {
            task.Wait();
            return task.Result;
        }

        public static IEnumerable<T> Generate<T>(this T seed, Func<T, T> generator)
        {
            var item = seed;
            while (true)
            {
                yield return item;
                var nextItem = generator(item);
                if (nextItem == null) yield break;
                item = nextItem;
            }
        }

        public static IEnumerable<Exception> GetChain(this Exception exception)
        {
            return exception.Generate(x => x.InnerException);
        }

        public static void AddRange<T>(this List<T> source, params T[] items)
        {
            source.AddRange(items);
        }

        public static ConditionalPluginsDsl<TPlugin, TContext> Append<TPlugin, TContext>(this
            ConditionalPluginsDsl<TPlugin, TContext> definitions, Type type,
            Func<TContext, bool> predicate = null, bool @default = false)
        {
            Type<ConditionalPluginsDsl<TPlugin, TContext>>
                .Method(x => x.Append<TPlugin>(y => false, false))?
                .GetGenericMethodDefinition().MakeGenericMethod(type)
                .Invoke(definitions, new object[] { predicate, @default });
            return definitions;
        }

        public static void WriteAllText(this Stream stream, string text)
        {
            var writer = new StreamWriter(stream);
            writer.Write(text);
            writer.Flush();
        }

        public static ILookup<string, string> GetHeaderValues(this string value)
        {
            return value.Split(';')
                .Select(x => x.Split('='))
                .ToLookup(x => x[0].Trim(),
                    x => x.Length > 1 ? x[1].Trim(' ', '"') : "");
        }

        public static HttpResponseMessage CreateTextResponse(this string content)
        {
            return content.CreateResponseTask("text/plain");
        }

        public static HttpResponseMessage CreateHtmlResponse(this string content)
        {
            return content.CreateResponseTask("text/html");
        }

        public static HttpResponseMessage CreateJsonResponse(this string content)
        {
            return content.CreateResponseTask("application/json");
        }

        public static HttpResponseMessage CreateResponseTask(
            this string content, string contentType)
        {
            return new HttpResponseMessage
            {
                Content = new StringContent(content, Encoding.UTF8, contentType)
            };
        }

        public static bool IsRedirect(this HttpStatusCode status)
        {
            return status == HttpStatusCode.MultipleChoices ||
                   status == HttpStatusCode.MovedPermanently || status == HttpStatusCode.Found ||
                   status == HttpStatusCode.SeeOther || status == HttpStatusCode.TemporaryRedirect;
        }

        public static TValue TryGet<TKey, TValue>(this IDictionary<TKey, TValue> source, TKey key)
        {
            return source.ContainsKey(key) ? source[key] : default(TValue);
        }

        public static MultipartFormDataContent AddTextFormData(this MultipartFormDataContent form,
            string name, string value, string filename = null, string contentType = null)
        {
            return form.AddFormData(name, new StringContent(value), filename, contentType);
        }

        public static MultipartFormDataContent AddStreamFormData(this MultipartFormDataContent form,
            string name, Stream stream, string filename = null, string contentType = null)
        {
            return form.AddFormData(name, new StreamContent(stream, 1.MB()), 
                filename, contentType, stream.Length);
        }

        public static MultipartFormDataContent AddFormData(this MultipartFormDataContent form,
            string name, HttpContent content, string filename = null, string contentType = null,
            long? contentLength = null)
        {
            if (contentType != null)
                content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            var disposition = content.Headers.ContentDisposition = 
                new ContentDispositionHeaderValue("form-data")
            {
                Name = name
            };
            if (filename.IsNotNullOrEmpty())  disposition.FileName = filename;
            content.Headers.ContentLength = contentLength;
            form.Add(content);
            return form;
        }
    }
}
