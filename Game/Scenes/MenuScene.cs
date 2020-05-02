using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes
{
    public interface IMenuScene : IScene { }
    public class MenuScene : GameScene, IMenuScene
    {
        /*
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(SpriteRenderer),
        };*/
        public MenuScene() : base(SceneId.MainMenu, new OrthographicCamera())
        { }

        protected override void Subscribed()
        {
            Raise(new PushMouseModeEvent(MouseMode.Normal));
            Raise(new PushInputModeEvent(InputMode.MainMenu));
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
