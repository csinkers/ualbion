using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
{
    public class NormalMouseMode : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<NormalMouseMode, SetMouseModeEvent>((x,e) =>
            {
                var activating = e.Mode == MouseModeId.Normal && !x._isActive;
                if (activating)
                {
                    x._isActive = true;
                    x.Raise(new SetCursorEvent(CoreSpriteId.Cursor));
                }
            }),
            new Handler<NormalMouseMode, InputEvent>((x,e) => x.OnInput(e)), 
        };

        void OnInput(InputEvent e)
        {
            if (!_isActive)
                return;

            if(e.Snapshot.WheelDelta < 0)
                Raise(new MagnifyEvent(-1));
            if(e.Snapshot.WheelDelta > 0)
                Raise(new MagnifyEvent(1));
        }

        bool _isActive;

        public NormalMouseMode() : base(Handlers) { }
    }
}