using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes
{
    public interface IDungeonScene : IScene { }
    public class DungeonScene : GameScene, IDungeonScene
    {
        /*
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(ExtrudedTileMapRenderer),
            typeof(SpriteRenderer),
        }; */

        public DungeonScene() : base(SceneId.World3D, new PerspectiveCamera(true))
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
