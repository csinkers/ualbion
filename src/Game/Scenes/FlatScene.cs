using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.Config;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes;

public interface IFlatScene : IScene { }
[Scene(SceneId.World2D)]
public class FlatScene : Container, IFlatScene
{
    public FlatScene() : base(nameof(SceneId.World2D))
    {
        var camera = AttachChild(new OrthographicCamera());
        AttachChild(new CameraMotion2D(camera));
    }

    protected override void Subscribed() => Raise(new PushInputModeEvent(InputMode.World2D));
    protected override void Unsubscribed() => Raise(new PopInputModeEvent());
}