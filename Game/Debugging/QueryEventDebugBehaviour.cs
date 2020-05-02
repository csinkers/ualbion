﻿using System;
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

            var querier = Engine.GlobalExchange?.Resolve<IQuerier>();
            var eventManager = Engine.GlobalExchange?.Resolve<IEventManager>();
            if (querier == null || eventManager == null)
                return null;

            var context = eventManager.ActiveContexts.FirstOrDefault()?.Clone() ?? new EventContext(new EventSource.None());
            return querier.Query(context, query, true).ToString();
        }
    }
}
