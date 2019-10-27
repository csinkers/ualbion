using System;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Scenes
{
    public interface IMapScene : IScene { }
    public class MapScene : GameScene, IMapScene
    {
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(SpriteRenderer),
        };

        public MapScene() : base(SceneId.Automap, new OrthographicCamera(), Renderers)
        {
            var cameraMotion = new CameraMotion2D((OrthographicCamera)Camera);
            Children.Add(cameraMotion);
        }
    }
}