using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Scenes
{
    public interface IIsometricBakeScene : IScene { }

    [Scene(SceneId.IsometricBake)]
    public class IsometricBakeScene : Scene, IIsometricBakeScene
    {
        public IsometricBakeScene() : base(nameof(SceneId.IsometricBake), new OrthographicCamera())
            => AttachChild(new CameraMotion2D((OrthographicCamera)Camera));
        // public IsometricBakeScene() : base(nameof(SceneId.IsometricBake), new PerspectiveCamera()) { }
        protected override void Subscribed() { }
        protected override void Unsubscribed() { }
    }
}