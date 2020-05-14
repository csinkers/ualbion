using System;
using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.MapEvents;
using UAlbion.Game.Entities;
using UAlbion.Game.Text;

namespace UAlbion.Game.Debugging
{
    public class FormatTextEventBehaviour : IDebugBehaviour
    {
        public Type[] HandledTypes { get; } = { typeof(BaseTextEvent), typeof(MapTextEvent), typeof(EventTextEvent) };
        public object Handle(DebugInspectorAction action, Reflector.ReflectedObject reflected)
        {
            if (action != DebugInspectorAction.Format)
                return null;

            if (!(reflected.Object is BaseTextEvent text))
                return null;

            IText textSource = Engine.GlobalExchange?.Resolve<ITextManager>()?.FormatTextEvent(text, FontColor.White);
            return textSource?.ToString();
        }
    }
}
