using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
{
    public class MouseModeStack : Component
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<MouseModeStack, PushMouseModeEvent>((x, e) =>
            {
                var inputManager = x.Exchange.Resolve<IInputManager>();
                x._stack.Push(inputManager.MouseMode);
                x.Raise(new SetMouseModeEvent(e.Mode));
            }),
            H<MouseModeStack, PopMouseModeEvent>((x, e) =>
            {
                if (x._stack.Count > 0)
                {
                    var newMode = x._stack.Pop();
                    x.Raise(new SetMouseModeEvent(newMode));
                }
            }),
            H<MouseModeStack, SetMouseModeEvent>((x, e) =>
            {
                x._stack.Clear();
            })
        );

        readonly Stack<MouseMode> _stack = new Stack<MouseMode>();
        public MouseModeStack() : base(Handlers) { }
    }
}