using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid.Events;

public class MouseInputEvent : EngineEvent, IVerboseEvent
{
    public double DeltaSeconds { get; set; }
    public InputSnapshot Snapshot { get; set; }
    public Vector2 MouseDelta { get; set; }
}