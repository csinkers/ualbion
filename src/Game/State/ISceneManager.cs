using UAlbion.Core;
using UAlbion.Core.Visual;
using UAlbion.Game.Scenes;

namespace UAlbion.Game.State;

public interface ISceneManager : ICameraProvider
{
    IScene ActiveScene { get; }
    SceneId ActiveSceneId { get; }
    IScene GetScene(SceneId sceneId);
}