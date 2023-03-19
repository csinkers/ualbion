using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Formats.Config;
using UAlbion.Game.Entities;
using UAlbion.Game.Events;

namespace UAlbion.Game.Scenes;

public interface IEditorScene : IScene { }
[Scene(SceneId.Editor)]
public class EditorScene : Container, IEditorScene
{
    public ICamera Camera { get; }
    public EditorScene() : base(nameof(SceneId.Editor))
    {
        Camera = AttachChild(new OrthographicCamera());
        AttachChild(new CameraMotion2D(Camera));
    }

    protected override void Subscribed() => Raise(new PushInputModeEvent(InputMode.Editor));
    protected override void Unsubscribed() => Raise(new PopInputModeEvent());
}