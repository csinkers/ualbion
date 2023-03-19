using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes;

public interface IMenuScene : IScene { }
[Scene(SceneId.MainMenu)]
public class MenuScene : Container, IMenuScene
{
    bool _clockWasRunning;
    public ICamera Camera { get; }

    public MenuScene() : base(nameof(SceneId.MainMenu))
    {
        Camera = AttachChild(new OrthographicCamera());
    }

    protected override void Subscribed()
    {
        _clockWasRunning = Resolve<IClock>().IsRunning;
        if (_clockWasRunning)
            Raise(new StopClockEvent());

        Raise(new PushMouseModeEvent(MouseMode.Normal));
        Raise(new PushInputModeEvent(InputMode.MainMenu));
    }

    protected override void Unsubscribed()
    {
        Raise(new PopMouseModeEvent());
        Raise(new PopInputModeEvent());
        if (_clockWasRunning)
            Raise(new StartClockEvent());
    }
}