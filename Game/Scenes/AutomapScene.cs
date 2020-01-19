using System;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes
{
    public interface IAutoMapScene : IScene { }
    public class AutomapScene : GameScene, IAutoMapScene
    {
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(SpriteRenderer),
        };

        public AutomapScene() : base(SceneId.Automap, new OrthographicCamera(), Renderers)
        {
            var cameraMotion = new CameraMotion2D((OrthographicCamera)Camera);
            Children.Add(cameraMotion);
        }

        public override void Subscribed()
        {
            Raise(new SetCursorEvent(CoreSpriteId.Cursor));
            Raise(new SetInputModeEvent(InputMode.Automap));
            Raise(new SetMouseModeEvent(MouseMode.Normal));
            base.Subscribed();
        }
    }
}
