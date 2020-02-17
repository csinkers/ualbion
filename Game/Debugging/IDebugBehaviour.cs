using System;

namespace UAlbion.Game.Debugging
{
    public interface IDebugBehaviour
    {
        Type HandledType { get; }
        object Handle(DebugInspectorAction action, Reflector.ReflectedObject reflected);
    }
}