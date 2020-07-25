using UAlbion.Core;

namespace UAlbion.Game.Scenes
{
    public interface IEmptyScene : IScene { }
    public class EmptyScene : GameScene, IEmptyScene
    {
        public EmptyScene() : base(SceneId.Empty, new OrthographicCamera()) { }
    }
}
