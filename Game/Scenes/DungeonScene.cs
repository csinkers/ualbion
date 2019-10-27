using System;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Scenes
{
    public interface IDungeonScene : IScene { }
    public class DungeonScene : GameScene, IDungeonScene
    {
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(ExtrudedTileMapRenderer),
            typeof(SpriteRenderer),
        };

        public DungeonScene() : base(SceneId.World3D, new PerspectiveCamera(), Renderers)
        {
            var cameraMotion = new CameraMotion3D((PerspectiveCamera)Camera);
            Children.Add(cameraMotion);
        }
    }
}