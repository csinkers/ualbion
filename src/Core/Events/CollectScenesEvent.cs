using System;
using UAlbion.Api;

namespace UAlbion.Core.Events
{
    public class CollectScenesEvent : EngineEvent, IVerboseEvent
    {
        public CollectScenesEvent(Action<Scene> register) { Register = register; }
        public Action<Scene> Register { get; }
    }
}
