using System;
using System.Collections.ObjectModel;
using UAlbion.Api.Eventing;
using UAlbion.Core;

namespace UAlbion.Game.Debugging;

public interface IDebugBehaviour
{
    ReadOnlyCollection<Type> HandledTypes { get; }
    object Handle(DebugInspectorAction action, ReflectedObject reflected, EventExchange exchange);
}