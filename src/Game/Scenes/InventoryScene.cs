using UAlbion.Api.Eventing;
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
    public ICamera Camera { get; }

    public InventoryScene() : base("Inventory")
    {
        Camera = AttachChild(new OrthographicCamera());
    }

    protected override void Subscribed()
    {
        _clockWasRunning = Resolve<IClock>().IsRunning;
        if (_clockWasRunning)
            Raise(new StopClockEvent());

        Raise(new PushInputModeEvent(InputMode.Inventory));
        Raise(new PushMouseModeEvent(MouseMode.Normal));
        Raise(new LoadPaletteEvent(Base.Palette.Inventory));
    }

    protected override void Unsubscribed()
    {
        Raise(new PopMouseModeEvent());
        Raise(new PopInputModeEvent());

        if (_clockWasRunning)
            Raise(new StartClockEvent());
    }
}