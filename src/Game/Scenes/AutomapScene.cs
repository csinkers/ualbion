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
    public AutomapScene() : base(nameof(SceneId.Automap))
    {
        var camera = AttachChild(new OrthographicCamera());
        AttachChild(new CameraMotion2D(camera));
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