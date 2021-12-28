using System.Collections.Generic;
using UAlbion.Formats.Config;

namespace UAlbion.Game.Input;

public interface IInputBinder
{
    IEnumerable<(InputMode, IEnumerable<(string, string)>)> Bindings { get; }

    bool IsAltPressed { get; }
    bool IsCtrlPressed { get; }
    bool IsShiftPressed { get; }
}