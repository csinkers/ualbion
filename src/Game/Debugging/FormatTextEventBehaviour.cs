using System;
using System.Collections.ObjectModel;
using UAlbion.Core;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Text;

namespace UAlbion.Game.Debugging
{
    public class FormatTextEventBehaviour : IDebugBehaviour
    {
        public ReadOnlyCollection<Type> HandledTypes { get; } = new ReadOnlyCollection<Type>(new[] { typeof(TextEvent) });
        public object Handle(DebugInspectorAction action, ReflectedObject reflected)
        {
            if (reflected == null || action != DebugInspectorAction.Format)
                return null;

            if (!(reflected.Target is TextEvent text))
                return null;

            IText textSource = Engine.GlobalExchange?.Resolve<ITextFormatter>()?.Format(text.ToId());
            return textSource?.ToString();
        }
    }
}
