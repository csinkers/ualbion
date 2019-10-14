using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
{
    public class ContextMenuInputMode : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<ContextMenuInputMode, SetInputModeEvent>((x,e) => x._isActive = e.Mode == InputMode.ContextMenu)
        );

        bool _isActive;

        public ContextMenuInputMode() : base(Handlers) { }
    }
}