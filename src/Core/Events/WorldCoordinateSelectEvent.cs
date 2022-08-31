using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;

namespace UAlbion.Core.Events;

#pragma warning disable CA2227 // Collection properties should be read only - we want to avoid per-frame allocations
public class WorldCoordinateSelectEvent : EngineEvent, IVerboseEvent
{
    public Vector3 Origin { get; set; }
    public Vector3 Direction { get; set; }
    public bool Debug { get; set; }
    public List<Selection> Selections { get; set; }
}
#pragma warning restore CA2227 // Collection properties should be read only