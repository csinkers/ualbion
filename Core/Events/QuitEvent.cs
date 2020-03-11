using UAlbion.Api;

namespace UAlbion.Core.Events
{
    [Event("quit", "Exit the game.", new[] { "exit" })]
    public class QuitEvent : EngineEvent { }
}
