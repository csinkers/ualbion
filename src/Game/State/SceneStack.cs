using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;

namespace UAlbion.Game.State;

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