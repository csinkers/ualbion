using System;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes
{
    public interface IDungeonScene : IScene { }
    public class DungeonScene : GameScene, IDungeonScene
    {
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(ExtrudedTileMapRenderer),
            typeof(SpriteRenderer),
        };

        public DungeonScene() : base(SceneId.World3D, new PerspectiveCamera(), Renderers)
        {
            var cameraMotion = new CameraMotion3D((PerspectiveCamera)Camera);
            Children.Add(cameraMotion);
        }

        protected override void Subscribed()
        {
            Raise(new SetCursorEvent(CoreSpriteId.Cursor));
            Raise(new SetInputModeEvent(InputMode.World3D));
            Raise(new SetMouseModeEvent(MouseMode.MouseLook));
            base.Subscribed();
        }
    }
}