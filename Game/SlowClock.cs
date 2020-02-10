using UAlbion.Core;
using UAlbion.Game.Events;

namespace UAlbion.Game
{
    public class SlowClock : Component
    {
        const int TicksPerFrame = 8;

        static readonly HandlerSet Handlers = new HandlerSet(
            H<SlowClock, UpdateEvent>((x,e) => x.OnUpdate(e))
        );

        int _ticks;
        int _frameCount;

        void OnUpdate(UpdateEvent updateEvent)
        {
            _ticks += updateEvent.Frames;
            int delta = 0;
            while(_ticks >= TicksPerFrame)
            {
                _ticks -= TicksPerFrame;
                delta++;
            }

            _frameCount += delta;
            Raise(new SlowClockEvent(delta, _frameCount));
        }

        public SlowClock() : base(Handlers) { }
    }
}