using UAlbion.Formats.Config;

namespace UAlbion.Game.Events
{
    public interface ISetMouseModeEvent : IGameEvent
    {
        MouseMode Mode { get; }
    }
}