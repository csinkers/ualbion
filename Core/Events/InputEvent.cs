using System.Numerics;
using UAlbion.Api;
using Veldrid;

namespace UAlbion.Core.Events
{
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
}