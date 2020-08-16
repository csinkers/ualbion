using System;
using System.Collections.ObjectModel;
using UAlbion.Core;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Text;

namespace UAlbion.Game.Debugging
{
    public class FormatTextEventBehaviour : IDebugBehaviour
    {
        public ReadOnlyCollection<Type> HandledTypes { get; } = new ReadOnlyCollection<Type>(new[] { typeof(BaseTextEvent), typeof(MapTextEvent), typeof(EventTextEvent) });
        public object Handle(DebugInspectorAction action, ReflectedObject reflected)
        {
            if (reflected == null || action != DebugInspectorAction.Format)
                return null;

            if (!(reflected.Target is BaseTextEvent text))
                return null;

            IText textSource = Engine.GlobalExchange?.Resolve<ITextFormatter>()?.Format(text.ToId());
            return textSource?.ToString();
        }
    }
}
