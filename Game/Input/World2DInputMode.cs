using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
{
    public class World2DInputMode : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<World2DInputMode, SetInputModeEvent>((x,e) =>
            {
                var activating = e.Mode == InputMode.World2D && !x._isActive;
                var deactivating = e.Mode != InputMode.World2D && x._isActive;

                if (activating)
                {
                    x._isActive = true;
                    x.Raise(new SetCursorEvent(CoreSpriteId.Cursor));
                }

                if (deactivating)
                {
                    x._isActive = false;
                }
            }),
            new Handler<World2DInputMode, InputEvent>((x,e) => x.OnInput(e)), 
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

        public World2DInputMode() : base(Handlers) { }
    }
}