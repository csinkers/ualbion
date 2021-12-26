using System;
using System.Diagnostics;

namespace UAlbion.Api
{
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
}