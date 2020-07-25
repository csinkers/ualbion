using UAlbion.Core;

namespace UAlbion.Game.Scenes
{
    public class GameScene : Scene
    {
        public SceneId Id { get; }

        protected GameScene(SceneId sceneId, ICamera camera)
            : base(sceneId.ToString(), camera)
        {
            Id = sceneId;
        }
    }
    // Interfaces just for resolving specific scenes in dependent components
}
