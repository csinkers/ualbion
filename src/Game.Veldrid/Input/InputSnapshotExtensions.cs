using System;
using System.Collections.Generic;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input;

public static class InputSnapshotExtensions
{
    public static bool CheckMouse(this InputSnapshot snapshot, MouseButton button, bool pressed)
    {
        if (snapshot == null) throw new ArgumentNullException(nameof(snapshot));
        if (snapshot.MouseEvents is List<MouseEvent> list) // Check for concrete type to avoid allocating an enumerator
        {
            foreach (var x in list)
                if (x.MouseButton == button && x.Down == pressed)
                    return true;
            return false;
        }

        foreach (var x in snapshot.MouseEvents)
            if (x.MouseButton == button && x.Down == pressed)
                return true;

        return false;
    }
}