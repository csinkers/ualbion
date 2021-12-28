using System;
using System.Diagnostics;
using System.Linq;
using UAlbion.Api;

namespace UAlbion.Core;

public class FrameCounter
{
    readonly Stopwatch _frameTimer;
    readonly int _a;
    readonly short _b;
    readonly short _c;
    long _previousFrameTicks;

    public long FrameCount { get; private set; }

    public FrameCounter()
    {
        _frameTimer = Stopwatch.StartNew();
        var correlationBytes = Guid.NewGuid().ToByteArray();
        _a = BitConverter.ToInt32(correlationBytes, 0);
        _b = BitConverter.ToInt16(correlationBytes, 4);
        _c = BitConverter.ToInt16(correlationBytes, 6);
    }

    public double StartFrame()
    {
        long currentFrameTicks = _frameTimer.ElapsedTicks;
        double deltaSeconds = (currentFrameTicks - _previousFrameTicks) / (double)Stopwatch.Frequency;
        _previousFrameTicks = currentFrameTicks;

        FrameCount++;
        var correlationId = new Guid(_a, _b, _c, BitConverter.GetBytes(FrameCount).Reverse().ToArray());
        CoreTrace.SetCorrelationId(correlationId);
        CoreTrace.Log.StartFrame(FrameCount, deltaSeconds * 1.0e6);
        return deltaSeconds;
    }
}