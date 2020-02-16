using System;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.Config;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes
{
    public interface IFlatScene : IScene { }
    public class FlatScene : GameScene, IFlatScene
    {
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(SpriteRenderer),
        };

        public FlatScene() : base(SceneId.World2D, new OrthographicCamera(), Renderers)
        {
            var cameraMotion = new CameraMotion2D((OrthographicCamera)Camera);
            Children.Add(cameraMotion);
        }

        public override void Subscribed()
        {
            Raise(new PushInputModeEvent(InputMode.World2D));
            Raise(new RefreshMapSubscribersEvent()); // TODO: Is this still needed?
            base.Subscribed();
        }

        protected override void Unsubscribed()
        {
            Raise(new PopInputModeEvent());
            base.Unsubscribed();
        }
    }

    public class RefreshMapSubscribersEvent : IEvent { }
}
