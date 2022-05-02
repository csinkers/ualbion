using System;
using System.Collections.ObjectModel;
using UAlbion.Api.Eventing;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Text;

namespace UAlbion.Game.Debugging;

public class FormatTextEventBehaviour : IDebugBehaviour
{
    public ReadOnlyCollection<Type> HandledTypes { get; } = new(new[] { typeof(MapTextEvent) });
    public object Handle(DebugInspectorAction action, ReflectedObject reflected, EventExchange exchange)
    {
        if (exchange == null || reflected == null || action != DebugInspectorAction.Format)
            return null;

        if (reflected.Target is not MapTextEvent text)
            return null;

        IText textSource = exchange.Resolve<ITextFormatter>()?.Format(text.ToId());
        return textSource?.ToString();
    }
}