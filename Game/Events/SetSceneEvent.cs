using UAlbion.Api;
using UAlbion.Game.Scenes;

namespace UAlbion.Game.Events
{
    [Event("set_scene", "Set the active scene")]
    public class SetSceneEvent : GameEvent
    {
        public SetSceneEvent(SceneId sceneId)
        {
            SceneId = sceneId;
        }

        [EventPart("id", "The identifier of the scene to activate")]
        public SceneId SceneId { get; }
    }
}