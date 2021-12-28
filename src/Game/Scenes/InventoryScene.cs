using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.Config;
using UAlbion.Formats.ScriptEvents;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes;

public interface IInventoryScene : IScene { }

[Scene(SceneId.Inventory)]
public class InventoryScene : Container, IInventoryScene
{
    bool _clockWasRunning;

    public InventoryScene() : base("Inventory")
    {
        AttachChild(new OrthographicCamera());
    }

    protected override void Subscribed()
    {
        _clockWasRunning = Resolve<IClock>().IsRunning;
        if (_clockWasRunning)
            Raise(new StopClockEvent());

        Raise(new ShowMapEvent(false));
        Raise(new PushInputModeEvent(InputMode.Inventory));
        Raise(new PushMouseModeEvent(MouseMode.Normal));
        Raise(new LoadPaletteEvent(Base.Palette.Inventory));
    }

    protected override void Unsubscribed()
    {
        Raise(new PopMouseModeEvent());
        Raise(new PopInputModeEvent());
        Raise(new ShowMapEvent());

        if (_clockWasRunning)
            Raise(new StartClockEvent());
    }
}