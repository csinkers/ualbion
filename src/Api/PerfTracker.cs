using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using static System.FormattableString;

namespace UAlbion.Api;

public static class PerfTracker
{
    const int MillisecondsToTicks = 10000;
    // TODO: Enqueue console writes in debug mode to a queue with an output
    // task / thread, to ensure that writing to the console doesn't affect
    // the perf stats

    class Stats
    {
        public long Count { get; private set; }
        public long Total { get; private set; }
        public long Min { get; private set; } = long.MaxValue;
        public long Max { get; private set; } = long.MinValue;
        public float Fast { get; set; }

        public void AddTicks(long ticks)
        {
            Count++;
            Total += ticks;
            Fast = (ticks + 8 * Fast) / 9.0f;
            if (Min > ticks) Min = ticks;
            if (Max < ticks) Max = ticks;
        }

        public void AddMs(long ms) => AddTicks(ms * MillisecondsToTicks);
    }

    static readonly Stopwatch StartupStopwatch = Stopwatch.StartNew();
    static readonly IDictionary<string, Stats> FrameTimes = new Dictionary<string, Stats>();
    static readonly IDictionary<string, int> FrameCounters = new Dictionary<string, int>();
    static readonly List<KeyValuePair<string, int>> CountersTemp = new();
    static readonly object SyncRoot = new();
    static int _frameCount;

    public static void BeginFrame()
    {
        if (_frameCount == 1)
            StartupEvent("First frame finished"); // Last startup event to be emitted

        _frameCount++;

        CountersTemp.Clear();
        foreach (var kvp in FrameCounters)
            CountersTemp.Add(kvp);

        foreach (var kvp in CountersTemp)
        {
            var count = kvp.Value;
            if (!FrameTimes.TryGetValue(kvp.Key, out var stats))
            {
                stats = new Stats { Fast = count * MillisecondsToTicks };
                FrameTimes[kvp.Key] = stats;
            }

            stats.AddMs(count);
            FrameCounters[kvp.Key] = 0;
        }
    }

    public static void EndFrameEvent(long startTicks, string name)
    {
        lock (SyncRoot)
        {
            long ticks = Stopwatch.GetTimestamp() - startTicks;
            if (!FrameTimes.TryGetValue(name, out var stats))
            {
                stats = new Stats { Fast = ticks };
                FrameTimes[name] = stats;
            }

            stats.AddTicks(ticks);
        }
    }

    public static void StartupEvent(string name)
    {
        if (_frameCount > 1) return;
        //#if DEBUG
        var tid = Thread.CurrentThread.ManagedThreadId;
        Console.WriteLine($"[{tid}] at {StartupStopwatch.ElapsedMilliseconds}: {name}");
        //#endif
        CoreTrace.Log.StartupEvent(name);
    }

    public static InfrequentTracker InfrequentEvent(string name) => new(name, StartupStopwatch);
    public static FrameTimeTracker FrameEvent(string name) => new(name);

    public static void Clear()
    {
        lock (SyncRoot)
        {
            FrameTimes.Clear();
            _frameCount = 0;
        }
    }

    public static (IList<string>, IList<string>) GetFrameStats()
    {
        var sb = new StringBuilder();
        var descriptions = new List<string>();
        var results = new List<string>();
        lock (SyncRoot)
        {
            foreach (var kvp in FrameTimes.OrderBy(x => x.Key))
            {
                sb.Append(Invariant($"Avg/frame: {(float)kvp.Value.Total / (MillisecondsToTicks * _frameCount):F3}"));
                sb.Append(Invariant($" Min: {(float)kvp.Value.Min / MillisecondsToTicks:F3}"));
                sb.Append(Invariant($" Max: {(float)kvp.Value.Max / MillisecondsToTicks:F3}"));
                sb.Append(Invariant($" F:{kvp.Value.Fast / MillisecondsToTicks:F3}"));
                sb.Append(Invariant($" Avg/call: {(float)kvp.Value.Total / (MillisecondsToTicks * kvp.Value.Count):F3}"));
                sb.Append(Invariant($" Calls/Frame: {(float)kvp.Value.Count / _frameCount:F3}"));
                sb.Append(Invariant($" Total: {kvp.Value.Total / MillisecondsToTicks}"));
                sb.Append(Invariant($" Count: {kvp.Value.Count}"));
                descriptions.Add(kvp.Key);
                results.Add(sb.ToString());
                sb.Clear();
            }
        }

        return (descriptions, results);
    }

    public static void IncrementFrameCounter(string name)
    {
        lock (SyncRoot)
        {
            FrameCounters.TryGetValue(name, out var count);
            FrameCounters[name] = count + 1;
        }
    }
}