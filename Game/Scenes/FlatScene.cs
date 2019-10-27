using System;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Scenes
{
    public interface IFlatScene : IScene { }
    public class FlatScene : GameScene, IFlatScene
    {
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(SpriteRenderer),
        };

        public FlatScene() : base(SceneId.World2D, new OrthographicCamera(), Renderers)
        {
            var cameraMotion = new CameraMotion2D((OrthographicCamera)Camera);
            Children.Add(cameraMotion);
        }
    }
}