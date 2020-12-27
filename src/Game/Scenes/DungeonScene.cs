using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.Config;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes
{
    public interface IDungeonScene : IScene { }
    [Scene(SceneId.World3D)]
    public class DungeonScene : Scene, IDungeonScene
    {
        public DungeonScene() : base(nameof(SceneId.World3D), new PerspectiveCamera(true))
        {
            AttachChild(new CameraMotion3D((PerspectiveCamera)Camera));
        }

        protected override void Subscribed()
        {
            Raise(new PushMouseModeEvent(MouseMode.MouseLook));
            Raise(new PushInputModeEvent(InputMode.World3D));
        }

        protected override void Unsubscribed()
        {
            Raise(new PopMouseModeEvent());
            Raise(new PopInputModeEvent());
        }
    }
}
