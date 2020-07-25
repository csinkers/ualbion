using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes
{
    public interface IFlatScene : IScene { }
    public class FlatScene : GameScene, IFlatScene
    {
        public FlatScene() : base(SceneId.World2D, new OrthographicCamera()) 
            => AttachChild(new CameraMotion2D((OrthographicCamera)Camera));

        protected override void Subscribed() => Raise(new PushInputModeEvent(InputMode.World2D));
        protected override void Unsubscribed() => Raise(new PopInputModeEvent());
    }
}
