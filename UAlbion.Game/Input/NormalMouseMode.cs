using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
{
    public class NormalMouseMode : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<NormalMouseMode, SetMouseModeEvent>((x,e) => x._isActive = e.Mode == MouseModeId.Normal), 
        };

        bool _isActive;

        public NormalMouseMode() : base(Handlers) { }
    }
}