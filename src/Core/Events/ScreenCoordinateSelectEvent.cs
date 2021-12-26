using System.Numerics;
using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class ScreenCoordinateSelectEvent : EngineEvent, IVerboseEvent, IAsyncEvent<Selection>
    {
        public ScreenCoordinateSelectEvent(Vector2 position, bool debug)
        {
            Position = position;
            Debug = debug;
        }

        public Vector2 Position { get; }
        public bool Debug { get; }
    }
}
