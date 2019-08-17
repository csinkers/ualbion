using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
{
    public class InventoryMoveMouseMode : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<InventoryMoveMouseMode, SetMouseModeEvent>((x,e) => x._isActive = e.Mode == MouseModeId.InventoryMove),
        };

        bool _isActive;

        public InventoryMoveMouseMode() : base(Handlers) { }
    }
}