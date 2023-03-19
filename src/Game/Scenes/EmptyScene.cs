using UAlbion.Api.Eventing;
using UAlbion.Core;
using UAlbion.Core.Visual;

namespace UAlbion.Game.Scenes;

public interface IEmptyScene : IScene { }

[Scene(SceneId.Empty)]
public class EmptyScene : Container, IEmptyScene
{
    public EmptyScene() : base("Empty") => Camera = AttachChild(new OrthographicCamera());
    public ICamera Camera { get; }
}