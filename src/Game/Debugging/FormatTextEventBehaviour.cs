using System;
using System.Collections.ObjectModel;
using UAlbion.Api.Eventing;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Debugging;

public class FormatTextEventBehaviour : Component, IDebugBehaviour
{
    public ReadOnlyCollection<Type> HandledTypes { get; } = new(new[] { typeof(TextEvent) });
    public object Handle(DebugInspectorAction action, ReflectedObject reflected)
    {
        if (reflected == null || action != DebugInspectorAction.Format)
            return null;

        if (reflected.Target is not TextEvent text)
            return null;

        // IText textSource = Resolve<ITextFormatter>()?.Format(text.ToId());
        //return textSource?.ToString();
        return text.ToString();
    }
}