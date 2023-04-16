using System;
using System.Collections.Generic;
using System.Numerics;
using UAlbion.Api.Eventing;
using UAlbion.Core.Events;
using Veldrid;

namespace UAlbion.Core.Veldrid.Events;

public class MouseInputEvent : EngineEvent, IVerboseEvent
{
    public double DeltaSeconds { get; set; }
    public Vector2 MouseDelta { get; set; }
    public float WheelDelta { get; set; }
    public Vector2 MousePosition { get; set; }
    public IReadOnlyList<MouseEvent> MouseEvents { get; set; }
    public Func<MouseButton, bool> IsMouseDown { get; set; }

    public bool CheckMouse(MouseButton button, bool pressed)
    {
        if (MouseEvents is List<MouseEvent> list) // Check for concrete type to avoid allocating an enumerator
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