using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Game.Events;
using UAlbion.Game.Scenes;

namespace UAlbion.Game.State
{
    public class SceneStack : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<SceneStack, PushSceneEvent>((x, e) =>
            {
                var sceneManager = x.Resolve<ISceneManager>();
                x._stack.Push(sceneManager.ActiveSceneId);
                x.Raise(new SetSceneEvent(e.SceneId));
            }),
            H<SceneStack, PopSceneEvent>((x, e) =>
            {
                if (x._stack.Count > 0)
                {
                    var newSceneId = x._stack.Pop();
                    x.Raise(new SetSceneEvent(newSceneId));
                }
            })
        );

        readonly Stack<SceneId> _stack = new Stack<SceneId>();
        public SceneStack() : base(Handlers) { }
    }
}
