using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes
{
    public interface IAutoMapScene : IScene { }
    public class AutomapScene : GameScene, IAutoMapScene
    {
        public AutomapScene() : base(SceneId.Automap, new OrthographicCamera())
        {
            AttachChild(new CameraMotion2D((OrthographicCamera)Camera));
        }

        protected override void Subscribed()
        {
            Raise(new PushMouseModeEvent(MouseMode.Normal));
            Raise(new PushInputModeEvent(InputMode.Automap));
        }

        protected override void Unsubscribed()
        {
            Raise(new PopMouseModeEvent());
            Raise(new PopInputModeEvent());
        }
    }
}
