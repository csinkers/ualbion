using System;
using System.Numerics;
using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class ScreenCoordinateSelectEvent : EngineEvent, IVerboseEvent
    {
        public ScreenCoordinateSelectEvent(Vector2 position, Action<float, Selection> registerHit)
        {
            Position = position;
            RegisterHit = registerHit;
        }

        public Vector2 Position { get; }
        public Action<float, Selection> RegisterHit { get; }
    }
}