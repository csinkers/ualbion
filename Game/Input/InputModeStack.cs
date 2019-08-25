using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
{
    public class InputModeStack : Component
    {
        static readonly Handler[] Handlers =
        {
            new Handler<InputModeStack, PushInputModeEvent>((x, e) =>
            {
                x._stack.Push(e.Mode);
                x.Raise(new SetInputModeEvent((int)e.Mode));
            }),
            new Handler<InputModeStack, PopInputModeEvent>((x, e) =>
            {
                if(x._stack.Count > 0)
                    x.Raise(new SetInputModeEvent((int)x._stack.Pop()));
            }),
            new Handler<InputModeStack, SetInputModeEvent>((x, e) => x._stack.Clear())
        };

        readonly Stack<InputMode> _stack = new Stack<InputMode>();
        public InputModeStack() : base(Handlers) { }
    }
}