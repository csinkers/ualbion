using System;
using UAlbion.Api;

namespace UAlbion.Core.Events
{
    //[Event("e:render")]
    public class RenderEvent : EngineEvent, IVerboseEvent
    {
        public RenderEvent(Action<IRenderable> add) { Add = add; }
        public Action<IRenderable> Add { get; }
    }
}
