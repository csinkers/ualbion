using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UAlbion.Api;

namespace UAlbion.Core;

public class FrameCounter
{
    readonly Stopwatch _frameTimer;
    readonly int _a;
    readonly short _b;
    readonly short _c;
    readonly byte[] _d = new byte[8];
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

        long frameCount = FrameCount;
        var span = MemoryMarshal.CreateReadOnlySpan(ref frameCount, 1);
        var byteSpan = MemoryMarshal.Cast<long, byte>(span);
        for (int i = 0; i < 8; i++)
            _d[7 - i] = byteSpan[i];

        var correlationId = new Guid(_a, _b, _c, _d);
        CoreTrace.SetCorrelationId(correlationId);
        CoreTrace.Log.StartFrame(FrameCount, deltaSeconds * 1.0e6);
        return deltaSeconds;
    }
}