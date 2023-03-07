using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;

namespace UAlbion.Game.State;

#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public class SceneStack : Component
{
    readonly Stack<SceneId> _stack = new();
    public SceneStack()
    {
        On<PushSceneEvent>(e =>
        {
            var sceneManager = Resolve<ISceneManager>();
            _stack.Push(sceneManager.ActiveSceneId);
            Raise(new SetSceneEvent(e.SceneId));
        });
        On<PopSceneEvent>(e =>
        {
            if (_stack.Count > 0)
            {
                var newSceneId = _stack.Pop();
                Raise(new SetSceneEvent(newSceneId));
            }
        });
    }
}
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix