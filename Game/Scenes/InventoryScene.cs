using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes
{
    public interface IInventoryScene : IScene { }
    public class InventoryScene : GameScene, IInventoryScene
    {
        /*
        static readonly Type[] Renderers = {
            typeof(DebugGuiRenderer),
            typeof(FullScreenQuad),
            typeof(ScreenDuplicator),
            typeof(SpriteRenderer),
        }; */
        public InventoryScene() : base(SceneId.Inventory, new OrthographicCamera())
        { }

        protected override void Subscribed()
        {
            Raise(new PushMouseModeEvent(MouseMode.Normal));
            Raise(new PushInputModeEvent(InputMode.Inventory));
            Raise(new LoadPaletteEvent(PaletteId.Inventory));
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
