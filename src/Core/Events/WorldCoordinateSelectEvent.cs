using System.Numerics;
using UAlbion.Api;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

public class WorldCoordinateSelectEvent : EngineEvent, IVerboseEvent, IAsyncEvent<Selection>
{
    public WorldCoordinateSelectEvent(Vector3 origin, Vector3 direction, bool debug)
    {
        Origin = origin;
        Direction = direction;
        Debug = debug;
    }

    public Vector3 Origin { get; }
    public Vector3 Direction { get; }
    public bool Debug { get; }
}