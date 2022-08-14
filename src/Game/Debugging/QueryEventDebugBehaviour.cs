/*
using System;
using System.Collections.ObjectModel;
using UAlbion.Core;
using UAlbion.Formats.MapEvents;

namespace UAlbion.Game.Debugging
{
    public class QueryEventDebugBehaviour : Component, IDebugBehaviour
    {
        public ReadOnlyCollection<Type> HandledTypes { get; } = new ReadOnlyCollection<Type>(new[] { typeof(QueryEvent) });
        public object Handle(DebugInspectorAction action, ReflectedObject reflected)
        {
            if (reflected == null || action != DebugInspectorAction.Format)
                return null;

            if (!(reflected.Target is QueryEvent query))
                return null;

            var querier = Resolve<IQuerier>();
            var eventManager = Resolve<IEventManager>();
            if (querier == null || eventManager == null)
                return null;

            return querier.QueryDebug(query).ToString();
        }
    }
}
*/
