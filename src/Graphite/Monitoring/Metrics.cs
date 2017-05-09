using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Graphite.Actions;

namespace Graphite.Monitoring
{
    public class Metrics
    {
        private DateTime _startTime;
        private int _totalRequests = 0;

        private ConcurrentDictionary<ActionDescriptor, ActionMetrics> Actions { get; } =
            new ConcurrentDictionary<ActionDescriptor, ActionMetrics>();

        public TimeSpan RunningTime => DateTime.Now - _startTime;
        public TimeSpan StartupTime { get; private set; }
        public int TotalRequests => _totalRequests;

        public void BeginStartup()
        {
            _startTime = DateTime.Now;
        }

        public void StartupComplete()
        {
            StartupTime = RunningTime;
        }

        public void IncrementRequests()
        {
            Interlocked.Increment(ref _totalRequests);
        }

        public ActionMetrics AddAction(ActionDescriptor action)
        {
            var actionMetrics = new ActionMetrics();
            Actions.TryAdd(action, actionMetrics);
            return actionMetrics;
        }

        public TimeSpan GetAverageRequestTime(ActionDescriptor action)
        {
            ActionMetrics actionMetrics;
            return Actions.TryGetValue(action, out actionMetrics) ?
                actionMetrics.GetAverageRequestTime() : TimeSpan.Zero;
        }
    }

    public class ActionMetrics
    {
        private readonly ConcurrentBag<ActionRequest> _requests = 
            new ConcurrentBag<ActionRequest>();

        public void AddRequestTime(TimeSpan duration)
        {
            _requests.Add(new ActionRequest(DateTime.Now, duration));
        }

        public TimeSpan GetAverageRequestTime()
        {
            return !_requests.Any() ? TimeSpan.Zero : TimeSpan.FromTicks(
                (long)_requests.Average(x => x.Duration.Ticks));
        }
    }

    public class ActionRequest
    {
        public ActionRequest(DateTime timestamp, TimeSpan duration)
        {
            Timestamp = timestamp;
            Duration = duration;
        }

        public DateTime Timestamp { get; }
        public TimeSpan Duration { get; }
    }
}
