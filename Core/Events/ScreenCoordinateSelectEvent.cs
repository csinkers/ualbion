using System.Numerics;
using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class ScreenCoordinateSelectEvent : EngineEvent, IVerboseEvent, IAsyncEvent<Selection>
    {
        public ScreenCoordinateSelectEvent(Vector2 position) => Position = position;
        public Vector2 Position { get; }
    }
}
