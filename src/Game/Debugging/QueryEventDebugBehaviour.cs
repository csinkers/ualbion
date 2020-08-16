using System;
using System.Collections.ObjectModel;
using UAlbion.Core;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Debugging
{
    public class QueryEventDebugBehaviour : IDebugBehaviour
    {
        public ReadOnlyCollection<Type> HandledTypes { get; } = new ReadOnlyCollection<Type>(new[] { typeof(QueryEvent), typeof(QueryItemEvent), typeof(QueryVerbEvent) });
        public object Handle(DebugInspectorAction action, ReflectedObject reflected)
        {
            if (reflected == null || action != DebugInspectorAction.Format)
                return null;

            if (!(reflected.Target is IQueryEvent query))
                return null;

            var querier = Engine.GlobalExchange?.Resolve<IQuerier>();
            var eventManager = Engine.GlobalExchange?.Resolve<IEventManager>();
            if (querier == null || eventManager == null)
                return null;

            return querier.QueryDebug(query).ToString();
        }
    }
}
