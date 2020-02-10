using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace UAlbion.Api
{
    public static class PerfTracker
    {
        // TODO: Enqueue console writes in debug mode to a queue with an output
        // task / thread, so ensure that writing to the console doesn't affect
        // the perf stats
        class FrameTimeTracker : IDisposable
        {
            readonly Stopwatch _stopwatch = Stopwatch.StartNew();
            readonly string _name;

            public FrameTimeTracker(string name) { _name = name; }

            public void Dispose()
            {
                lock (_syncRoot)
                {
                    long ticks = _stopwatch.ElapsedTicks;
                    if (!_frameTimes.ContainsKey(_name))
                        _frameTimes[_name] = new Stats { Fast = ticks };

                    var stats = _frameTimes[_name];
                    stats.AddTicks(ticks);
                }
            }
        }

        class InfrequentTracker : IDisposable
        {
            readonly Stopwatch _stopwatch = Stopwatch.StartNew();
            readonly string _name;

            public InfrequentTracker(string name)
            {
                _name = name;
#if DEBUG
                Console.WriteLine($"Starting {name}");
#endif
                CoreTrace.Log.StartupEvent(name);
            }

            public void Dispose()
            {
#if DEBUG
                Console.WriteLine($"Finished {_name} in {_stopwatch.ElapsedMilliseconds} ms");
#endif
                CoreTrace.Log.StartupEvent(_name);
            }
        }

        class Stats
        {
            public long Count { get; set; }
            public long Total { get; set; }
            public long Min { get; set; } = long.MaxValue;
            public long Max { get; set; } = long.MinValue;
            public float Fast { get; set; }

            public void AddTicks(long ticks)
            {
                Count++;
                Total += ticks;
                Fast = (ticks + 8 * Fast) / 9.0f;
                if (Min > ticks) Min = ticks;
                if (Max < ticks) Max = ticks;
            }

            public void AddMs(long ms) => AddTicks(ms * 10000);
        }

        static readonly Stopwatch _startupStopwatch = Stopwatch.StartNew();
        static readonly IDictionary<string, Stats> _frameTimes = new Dictionary<string, Stats>();
        static readonly IDictionary<string, int> _frameCounters = new Dictionary<string, int>();
        static readonly object _syncRoot = new object();
        static int _frameCount;

        public static void BeginFrame()
        {
            _frameCount++;
            foreach (var key in _frameCounters.Keys.ToList())
            {
                var count = _frameCounters[key];
                if (!_frameTimes.ContainsKey(key))
                    _frameTimes[key] = new Stats { Fast = count * 10000 };

                var stats = _frameTimes[key];
                stats.AddMs(count);

                _frameCounters[key] = 0;
            }
        }

        public static void StartupEvent(string name)
        {
            if (_frameCount == 0)
            {
//#if DEBUG
                Console.WriteLine($"at {_startupStopwatch.ElapsedMilliseconds}: {name}");
//#endif
                CoreTrace.Log.StartupEvent(name);
            }
        }

        public static IDisposable InfrequentEvent(string name) => new InfrequentTracker(name);

        public static IDisposable FrameEvent(string name) => new FrameTimeTracker(name);

        public static void Clear()
        {
            lock (_syncRoot)
            {
                _frameTimes.Clear();
                _frameCount = 0;
            }
        }

        public static (IList<string>, IList<string>) GetFrameStats()
        {
            var sb = new StringBuilder();
            var descriptions = new List<string>();
            var results = new List<string>();
            lock(_syncRoot)
            {
                foreach (var kvp in _frameTimes.OrderBy(x => x.Key))
                {
                    sb.Append($"Avg/frame: {(float)kvp.Value.Total / (10000 * _frameCount):F3}");
                    sb.Append($" Min: {(float)kvp.Value.Min / 10000:F3}");
                    sb.Append($" Max: {(float)kvp.Value.Max / 10000:F3}");
                    sb.Append($" F:{kvp.Value.Fast / 10000:F3}");
                    sb.Append($" Avg/call: {(float)kvp.Value.Total / (10000 * kvp.Value.Count):F3}");
                    sb.Append($" Calls/Frame: {(float)kvp.Value.Count / _frameCount:F3}");
                    sb.Append($" Total: {kvp.Value.Total / 10000}");
                    sb.Append($" Count: {kvp.Value.Count}");
                    descriptions.Add(kvp.Key);
                    results.Add(sb.ToString());
                    sb.Clear();
                }
            }

            return (descriptions, results);
        }

        public static void IncrementFrameCounter(string name)
        {
            lock(_syncRoot)
            {
                _frameCounters.TryGetValue(name, out var count);
                _frameCounters[name] = count + 1;
            }
        }
    }
}
