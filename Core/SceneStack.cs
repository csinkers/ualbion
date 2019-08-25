using System.Collections.Generic;
using UAlbion.Core.Events;

namespace UAlbion.Core
{
    public class SceneStack : Component
    {
        static readonly Handler[] Handlers =
        {
            new Handler<SceneStack, PushSceneEvent>((x, e) =>
            {
                x._stack.Push(e.SceneId);
                x.Raise(new SetSceneEvent(e.SceneId));
            }),
            new Handler<SceneStack, PopSceneEvent>((x, e) =>
            {
                if(x._stack.Count > 0)
                    x.Raise(new SetSceneEvent(x._stack.Pop()));
            }),
            new Handler<SceneStack, SetSceneEvent>((x, e) => x._stack.Clear())
        };

        readonly Stack<int> _stack = new Stack<int>();
        public SceneStack() : base(Handlers) { }
    }
}