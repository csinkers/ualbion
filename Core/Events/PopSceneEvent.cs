using UAlbion.Api;

namespace UAlbion.Core.Events
{
    [Event("pop_scene", "Restore a previously active scene")]
    public class PopSceneEvent : EngineEvent { }
}