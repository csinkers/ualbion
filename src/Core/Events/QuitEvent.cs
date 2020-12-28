using UAlbion.Api;

namespace UAlbion.Core.Events
{
    [Event("quit", "Exit the game.", "exit")]
    public class QuitEvent : EngineEvent { }
}
