using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
{
    public class ContextMenuMouseMode : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<ContextMenuMouseMode, SetMouseModeEvent>((x,e) => x._isActive = e.Mode == MouseModeId.ContextMenu),
        };

        bool _isActive;

        public ContextMenuMouseMode() : base(Handlers) { }
    }
}