using UAlbion.Core;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Scenes
{
    public interface IIsometricBakeScene : IScene { }

    [Scene(SceneId.IsometricBake)]
    public class IsometricBakeScene : Scene, IIsometricBakeScene
    {
        public IsometricBakeScene() : base(nameof(SceneId.IsometricBake), new OrthographicCamera()) { }
        // public IsometricBakeScene() : base(nameof(SceneId.IsometricBake), new PerspectiveCamera()) { }
        protected override void Subscribed() { }
        protected override void Unsubscribed() { }
    }
}