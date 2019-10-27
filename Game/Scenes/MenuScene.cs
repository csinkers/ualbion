using System;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes
{
    public interface IMenuScene : IScene { }
    public class MenuScene : GameScene, IMenuScene
    {
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(SpriteRenderer),
        };
        public MenuScene() : base(SceneId.MainMenu, new OrthographicCamera(), Renderers)
        { }

        protected override void Subscribed()
        {
            Raise(new SetCursorEvent(CoreSpriteId.Cursor));
            Raise(new SetMouseModeEvent(MouseMode.Normal));
            Raise(new SetInputModeEvent(InputMode.Dialog));
            base.Subscribed();
        }
    }
}