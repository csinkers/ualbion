using UAlbion.Api;

namespace UAlbion.Core.Events
{
    [Event("set_scene", "Set the active scene")]
    public class SetSceneEvent : EngineEvent
    {
        public SetSceneEvent(int sceneId)
        {
            SceneId = sceneId;
        }

        [EventPart("id", "The identifier of the scene to activate")]
        public int SceneId { get; }
    }
}