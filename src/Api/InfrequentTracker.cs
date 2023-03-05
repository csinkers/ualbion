using System;
using System.Diagnostics;

namespace UAlbion.Api;
#pragma warning disable CA1815 // Override equals and operator equals on value types
public readonly struct InfrequentTracker : IDisposable
{
    readonly Stopwatch _stopwatch;
    readonly string _name;
    readonly long _initialTicks;

    public InfrequentTracker(string name, Stopwatch stopwatch)
    {
        _name = name;
        _stopwatch = stopwatch ?? throw new ArgumentNullException(nameof(stopwatch));
        _initialTicks = Stopwatch.GetTimestamp();
#if DEBUG
        var tid = Environment.CurrentManagedThreadId;
        Console.WriteLine($"[{tid}] at {stopwatch.ElapsedMilliseconds}: Starting {_name}");
#endif
        CoreTrace.Log.StartupEvent(name);
    }

    public void Dispose()
    {
#if DEBUG
        var tid = Environment.CurrentManagedThreadId;
        var elapsedMs = (Stopwatch.GetTimestamp() - _initialTicks) * 1000 / Stopwatch.Frequency;
        Console.WriteLine($"[{tid}] at {_stopwatch.ElapsedMilliseconds}: Finished {_name} in {elapsedMs}");
#endif
        CoreTrace.Log.StartupEvent(_name);
    }
}
#pragma warning restore CA1815 // Override equals and operator equals on value types