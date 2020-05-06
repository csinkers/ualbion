using UAlbion.Core;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes
{
    public interface IInventoryScene : IScene { }
    public class InventoryScene : GameScene, IInventoryScene
    {
        bool _clockWasRunning;
        public InventoryScene() : base(SceneId.Inventory, new OrthographicCamera())
        { }

        protected override void Subscribed()
        {
            _clockWasRunning = Resolve<IClock>().IsRunning;
            if (_clockWasRunning)
                Raise(new StopClockEvent());

            Raise(new PushInputModeEvent(InputMode.Inventory));
            Raise(new PushMouseModeEvent(MouseMode.Normal));
            Raise(new LoadPaletteEvent(PaletteId.Inventory));
        }

        protected override void Unsubscribed()
        {
            Raise(new PopMouseModeEvent());
            Raise(new PopInputModeEvent());
            if (_clockWasRunning)
                Raise(new StartClockEvent());
        }
    }
}
