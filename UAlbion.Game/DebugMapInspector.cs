using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Events;

namespace UAlbion.Game
{
    public class MouseInputEvent
    {
        public int ScreenX { get; }
        public int ScreenY { get; }
        public int WorldX { get; }
        public int WorldY { get; }
    }

    public class DebugMapInspector : Component
    {
        static readonly IList<Handler> Handlers = new Handler[]
        {
            new Handler<DebugMapInspector, MouseInputEvent>((x, e) => x.Input(e)),
            new Handler<DebugMapInspector, EngineUpdateEvent>((x, _) => x.RenderDialog())
        };

        int _cursorX;
        int _cursorY;

        void Input(MouseInputEvent inputEvent)
        {
        }

        void RenderDialog()
        {
        }

        public DebugMapInspector() : base(Handlers)
        {
        }
    }
}