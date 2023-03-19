using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.Config;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes;

public interface IAutoMapScene : IScene { }
[Scene(SceneId.Automap)]
public class AutomapScene : Container, IAutoMapScene
{
    public ICamera Camera { get; }
    public AutomapScene() : base(nameof(SceneId.Automap))
    {
        Camera = AttachChild(new OrthographicCamera());
        AttachChild(new CameraMotion2D(Camera));
    }

    protected override void Subscribed()
    {
        Raise(new PushMouseModeEvent(MouseMode.Normal));
        Raise(new PushInputModeEvent(InputMode.Automap));
    }

    protected override void Unsubscribed()
    {
        Raise(new PopMouseModeEvent());
        Raise(new PopInputModeEvent());
    }
}