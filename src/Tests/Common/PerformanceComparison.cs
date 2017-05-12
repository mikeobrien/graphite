using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Graphite.Extensions;

namespace Tests.Common
{
    public class PerformanceComparison
    {
        private readonly int _iterations;
        private readonly int _warmupIterations;
        private readonly Func<TimeSpan, double> _averageBy;
        private readonly string _suffix;
        private readonly List<Case> _cases = new List<Case>();
        private readonly int _max;

        private PerformanceComparison(int iterations, int warmupIterations, 
            Func<TimeSpan, double> averageBy, string suffix, int max)
        {
            _iterations = iterations;
            _warmupIterations = warmupIterations;
            _averageBy = averageBy;
            _suffix = suffix;
            _max = max;
        }

        public static PerformanceComparison InTicks(int iterations, 
            int warmupIterations = 100, int max = int.MaxValue)
        {
            return new PerformanceComparison(iterations, 
                warmupIterations, x => x.Ticks, " ticks", max);
        }

        public static PerformanceComparison InMilliseconds(int iterations, 
            int warmupIterations = 100, int max = int.MaxValue)
        {
            return new PerformanceComparison(iterations,
                warmupIterations, x => x.TotalMilliseconds, "ms", max);
        }

        public Case AddCase(string name, Action action)
        {
            return _cases.AddItem(new Case(name, action, _averageBy, _max));
        }

        public class Case
        {
            private readonly Func<TimeSpan, double> _averageBy;
            private readonly int _max;

            public Case(string name, Action action,
                Func<TimeSpan, double> averageBy,
                int max)
            {
                _averageBy = averageBy;
                _max = max;
                Name = name;
                Action = action;
            }

            public string Name { get; }
            public Action Action { get; }
            public ConcurrentBag<DateTime> Timestamps { get; } = new ConcurrentBag<DateTime>();
            public ConcurrentBag<TimeSpan> Results { get; } = new ConcurrentBag<TimeSpan>();
            public double Average => Math.Round(Results.Average(x => Math.Min(_averageBy(x), _max)), 2);
            public double PerSecond => Math.Round(Timestamps.Count / 
                (Timestamps.Max() - Timestamps.Min()).TotalSeconds);
            public double Variance
            {
                get
                {
                    var results = Results.Select(x => Math.Min(_averageBy(x), _max)).ToList();
                    var average = results.Average();
                    return results.Select(x => Math.Pow(x - average, 2)).Sum() / results.Count;
                }
            }

            public double StandardDeviation => Math.Sqrt(Variance);
        }

        public void Run()
        {
            _cases.AsEnumerable().ForEach(x =>
                _warmupIterations.TimesParallel(x.Action));

            _cases.AsEnumerable().ForEach(x =>
                {
                    Thread.Sleep(1000);
                    _iterations.TimesParallel(() =>
                    {
                        x.Timestamps.Add(DateTime.Now);
                        x.Results.Add(x.Action.Elapsed());
                    });
                }
            );
            Print();
        }

        private void Print()
        {
            var maxLength = _cases.Max(x => x.Name.Length) + 1;
            _cases.ForEach(x => Console.WriteLine(x.Name + ":" +
                new string(' ', maxLength - x.Name.Length) + 
                    x.Average + _suffix + " " + x.PerSecond + " p/s " +
                    "Variance: " + Math.Round(x.Variance, 2) +  " " +
                    "Standard deviation: " + Math.Round(x.StandardDeviation, 2) + _suffix));
        }
    }
}
