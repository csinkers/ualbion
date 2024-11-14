using System;
using System.Collections.ObjectModel;
using UAlbion.Api.Eventing;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Diag;

public class FormatTextEventBehaviour : Component, IDebugBehaviour
{
    public ReadOnlyCollection<Type> HandledTypes { get; } = new([typeof(TextEvent)]);
    /*
    public object Handle(DebugInspectorAction action, in ReflectorState state)
    {
        if (action != DebugInspectorAction.Format)
            return null;

        return state.Target is TextEvent text 
            ? text.ToString() 
            : null;
    }
    */
}