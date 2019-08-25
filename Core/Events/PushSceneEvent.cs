using UAlbion.Api;

namespace UAlbion.Core.Events
{
    [Event("push_scene", "Set the active scene, but save the previously active scene so it can be restored using pop_scene")]
    public class PushSceneEvent : EngineEvent
    {
        public PushSceneEvent(int sceneId)
        {
            SceneId = sceneId;
        }

        [EventPart("id", "The identifier of the scene to activate")]
        public int SceneId { get; }
    }
}