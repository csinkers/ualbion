using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
{
    public class InputModeStack : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<InputModeStack, PushInputModeEvent>((x, e) =>
            {
                var inputManager = x.Resolve<IInputManager>();
                x._stack.Push(inputManager.InputMode);
                x.Raise(new SetInputModeEvent(e.Mode));
            }),
            H<InputModeStack, PopInputModeEvent>((x, e) =>
            {
                if (x._stack.Count > 0)
                {
                    var newMode = x._stack.Pop();
                    x.Raise(new SetInputModeEvent(newMode));
                }
            })
        );

        readonly Stack<InputMode> _stack = new Stack<InputMode>();
        public InputModeStack() : base(Handlers) { }
    }
}
