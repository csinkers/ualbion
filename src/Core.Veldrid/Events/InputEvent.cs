using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using Veldrid.Sdl2;

namespace UAlbion.Core.Veldrid.Events;

public class InputEvent : EngineEvent, IVerboseEvent
{
    public double DeltaSeconds { get; set; }
    public Vector2 MouseDelta { get; set; }
    public InputSnapshot Snapshot { get; set; }
}