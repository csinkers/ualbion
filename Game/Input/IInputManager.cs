using UAlbion.Formats.Config;

namespace UAlbion.Game.Input
{
    public interface IInputManager
    {
        InputMode InputMode { get; }
        MouseMode MouseMode { get; }
    }
}