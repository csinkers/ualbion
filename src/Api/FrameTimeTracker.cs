using System;
using System.Diagnostics;

namespace UAlbion.Api
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
    public readonly struct FrameTimeTracker : IDisposable
    {
        readonly long _ticks;
        readonly string _name;

        public FrameTimeTracker(string name)
        {
            _name = name;
            _ticks = Stopwatch.GetTimestamp();
        }

        public void Dispose() => PerfTracker.EndFrameEvent(_ticks, _name);
    }
#pragma warning restore CA1815 // Override equals and operator equals on value types
}