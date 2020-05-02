using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes
{
    public interface IFlatScene : IScene { }
    public class FlatScene : GameScene, IFlatScene
    {
        /*
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(SpriteRenderer),
        };*/

        public FlatScene() : base(SceneId.World2D, new OrthographicCamera())
        {
            AttachChild(new CameraMotion2D((OrthographicCamera)Camera));
        }

        protected override void Subscribed()
        {
            Raise(new PushInputModeEvent(InputMode.World2D));
            base.Subscribed();
        }

        public override void Detach()
        {
            Raise(new PopInputModeEvent());
            base.Detach();
        }
    }
}
