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
                x._stack.Push(x._currentMode);
                x.Raise(new SetInputModeEvent(e.Mode));
                x._currentMode = e.Mode;
            }),
            H<InputModeStack, PopInputModeEvent>((x, e) =>
            {
                if (x._stack.Count > 0)
                {
                    var newMode = x._stack.Pop();
                    x.Raise(new SetInputModeEvent(newMode));
                    x._currentMode = newMode;
                }
            }),
            H<InputModeStack, SetInputModeEvent>((x, e) =>
            {
                x._stack.Clear();
                x._currentMode = e.Mode;
            })
        );

        readonly Stack<InputMode> _stack = new Stack<InputMode>();
        InputMode _currentMode = InputMode.Global;
        public InputModeStack() : base(Handlers) { }
    }
}