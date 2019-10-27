using UAlbion.Api;

namespace UAlbion.Game.Events
{
    [Event("push_scene", "Set the active scene, but save the previously active scene so it can be restored using pop_scene")]
    public class PushSceneEvent : GameEvent
    {
        public PushSceneEvent(SceneId sceneId)
        {
            SceneId = sceneId;
        }

        [EventPart("id", "The identifier of the scene to activate")]
        public SceneId SceneId { get; }
    }
}