using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid.Events;

public class InputEvent : EngineEvent, IVerboseEvent
{
    public double DeltaSeconds { get; }
    public InputSnapshot Snapshot { get; }
    public Vector2 MouseDelta { get; }

    public InputEvent(double deltaSeconds, InputSnapshot snapshot, Vector2 mouseDelta)
    {
        DeltaSeconds = deltaSeconds;
        Snapshot = snapshot;
        MouseDelta = mouseDelta;
    }
}