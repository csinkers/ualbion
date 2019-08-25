using UAlbion.Api;

namespace UAlbion.Core.Events
{
    [Event("e:subscribed", "Emitted to an object immediately after it is subscribed.")]
    public class SubscribedEvent : EngineEvent { }
}