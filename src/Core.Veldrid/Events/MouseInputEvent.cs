using System.Collections.Generic;
using System.Numerics;
using Veldrid.Sdl2;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;

namespace UAlbion.Core.Veldrid.Events;

public class MouseInputEvent : EngineEvent, IVerboseEvent
{
    public double DeltaSeconds { get; set; }
    public Vector2 MouseDelta { get; set; }
    public Vector2 WheelDelta { get; set; }
    public Vector2 MousePosition { get; set; }
    public IReadOnlyList<MouseButtonEvent> MouseEvents { get; set; }
    public bool IsMouseDown(MouseButton button) => (Snapshot.MouseDown & button) != 0;
    public InputSnapshot Snapshot { get; set; } // Only used for IsMouseDown

    public bool CheckMouse(MouseButton button, bool pressed)
    {
        if (MouseEvents is List<MouseButtonEvent> list) // Check for concrete type to avoid allocating an enumerator
        {
            foreach (var x in list)
                if (x.MouseButton == button && x.Down == pressed)
                    return true;
            return false;
        }

        foreach (var x in MouseEvents)
            if (x.MouseButton == button && x.Down == pressed)
                return true;

        return false;
    }
}
