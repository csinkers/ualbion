using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes
{
    public interface IAutoMapScene : IScene { }
    public class AutomapScene : GameScene, IAutoMapScene
    {
        /*
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(SpriteRenderer),
        }; */

        public AutomapScene() : base(SceneId.Automap, new OrthographicCamera())
        {
            AttachChild(new CameraMotion2D((OrthographicCamera)Camera));
        }

        protected override void Subscribed()
        {
            Raise(new PushMouseModeEvent(MouseMode.Normal));
            Raise(new PushInputModeEvent(InputMode.Automap));
            base.Subscribed();
        }

        public override void Detach()
        {
            Raise(new PopMouseModeEvent());
            Raise(new PopInputModeEvent());
            base.Detach();
        }
    }
}
