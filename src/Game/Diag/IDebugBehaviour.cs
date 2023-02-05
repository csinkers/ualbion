using System;
using System.Collections.ObjectModel;
using UAlbion.Api.Eventing;

namespace UAlbion.Game.Diag;

public interface IDebugBehaviour : IComponent
{
    ReadOnlyCollection<Type> HandledTypes { get; }
    object Handle(DebugInspectorAction action, in ReflectorState state);
}