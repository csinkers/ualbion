using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Scenes;

namespace UAlbion.Game.State
{
    public interface ISceneManager
    {
        EventExchange GetExchange(SceneId id);
        IScene ActiveScene { get; }
        SceneId ActiveSceneId { get; }
    }
}