using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class SlowClock : Component
    {
        const int TicksPerFrame = 8;

        int _ticks;
        int _frameCount;

        public SlowClock()
        {
            On<FastClockEvent>(OnUpdate);
        }

        void OnUpdate(FastClockEvent updateEvent)
        {
            _ticks += updateEvent.Frames;
            int delta = 0;
            while(_ticks >= TicksPerFrame)
            {
                _ticks -= TicksPerFrame;
                delta++;
            }

            if (delta <= 0)
                return;

            _frameCount += delta;
            Raise(new SlowClockEvent(delta, _frameCount));
        }
    }

    [Event("slow_clock")]
    public class SlowClockEvent : GameEvent, IVerboseEvent
    {
        [EventPart("delta")] public int Delta { get; }
        [EventPart("frame_count")] public int FrameCount { get; }

        public SlowClockEvent(int delta, int frameCount)
        {
            Delta = delta;
            FrameCount = frameCount;
        }
    }
}
