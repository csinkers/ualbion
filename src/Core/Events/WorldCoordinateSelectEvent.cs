using System.Numerics;
using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class WorldCoordinateSelectEvent : EngineEvent, IVerboseEvent, IAsyncEvent<Selection>
    {
        public WorldCoordinateSelectEvent(Vector3 origin, Vector3 direction)
        {
            Origin = origin;
            Direction = direction;
        }

        public Vector3 Origin { get; }
        public Vector3 Direction { get; }
    }
}
