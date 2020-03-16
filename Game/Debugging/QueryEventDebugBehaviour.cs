using System;
using System.Linq;
using UAlbion.Core;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Debugging
{
    public class QueryEventDebugBehaviour : IDebugBehaviour
    {
        public Type[] HandledTypes { get; } = { typeof(QueryEvent), typeof(QueryItemEvent), typeof(QueryVerbEvent) };
        public object Handle(DebugInspectorAction action, Reflector.ReflectedObject reflected)
        {
            if (action != DebugInspectorAction.Format)
                return null;

            if (!(reflected.Object is IQueryEvent query))
                return null;

            var querier = Engine.Global?.Resolve<IQuerier>();
            var eventManager = Engine.Global?.Resolve<IEventManager>();
            if (querier == null || eventManager == null)
                return null;

            var context = eventManager.ActiveContexts.FirstOrDefault()?.Clone() ?? new EventContext();
            return querier.Query(context, query).ToString();
        }
    }
}
