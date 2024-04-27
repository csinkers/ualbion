using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;

namespace UAlbion.Game.Combat;

[Scene(SceneId.Combat)]
public class CombatScene : Container, IScene
{
    bool _clockWasRunning;
    public ICamera Camera { get; }
    public CombatScene() : base(nameof(SceneId.Combat))
    {
        Camera = AttachChild(new PerspectiveCamera());
    }

    protected override void Subscribed()
    {
        _clockWasRunning = Resolve<IClock>().IsRunning;
        if (_clockWasRunning)
            Raise(new StopClockEvent());

        // Raise(new ShowMapEvent(false));
        Raise(new PushMouseModeEvent(MouseMode.Normal));
        Raise(new PushInputModeEvent(InputMode.Combat));
    }

    protected override void Unsubscribed()
    {
        Raise(new PopMouseModeEvent());
        Raise(new PopInputModeEvent());
        // Raise(new ShowMapEvent());

        if (_clockWasRunning)
            Raise(new StartClockEvent());
    }
}