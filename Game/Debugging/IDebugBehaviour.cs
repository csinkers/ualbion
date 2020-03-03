using System;

namespace UAlbion.Game.Debugging
{
    public interface IDebugBehaviour
    {
        Type[] HandledTypes { get; }
        object Handle(DebugInspectorAction action, Reflector.ReflectedObject reflected);
    }
}