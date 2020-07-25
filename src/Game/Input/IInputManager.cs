using System.Collections.Generic;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Input
{
    public interface IInputManager
    {
        InputMode InputMode { get; }
        MouseMode MouseMode { get; }

        IEnumerable<InputMode> InputModeStack { get; }
        IEnumerable<MouseMode> MouseModeStack { get; }
    }
}
