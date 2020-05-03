using UAlbion.Core;
using UAlbion.Game.Scenes;

namespace UAlbion.Game.State
{
    public interface ISceneManager
    {
        IScene ActiveScene { get; }
        SceneId ActiveSceneId { get; }
        IScene GetScene(SceneId inventory);
    }
}
