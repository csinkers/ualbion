using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Game.Entities;

namespace UAlbion.Game.Scenes;

public interface IIsometricBakeScene : IScene { }

[Scene(SceneId.IsometricBake)]
public class IsometricBakeScene : Container, IIsometricBakeScene
{
    public IsometricBakeScene() : base(nameof(SceneId.IsometricBake))
    {
        Camera = AttachChild(new OrthographicCamera(false));
        AttachChild(new CameraMotion2D(Camera));
    }

    public ICamera Camera { get; }

    protected override void Subscribed() { }
    protected override void Unsubscribed() { }
}