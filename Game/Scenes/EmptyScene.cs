using System;
using UAlbion.Core;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Scenes
{
    public interface IEmptyScene : IScene { }
    public class EmptyScene : GameScene, IEmptyScene
    {
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(SpriteRenderer),
        };

        public EmptyScene() : base(SceneId.Empty, new OrthographicCamera(), Renderers) { }
    }
}