using System;
using System.Collections.ObjectModel;

namespace UAlbion.Game.Debugging
{
    public interface IDebugBehaviour
    {
        ReadOnlyCollection<Type> HandledTypes { get; }
        object Handle(DebugInspectorAction action, ReflectedObject reflected);
    }
}
