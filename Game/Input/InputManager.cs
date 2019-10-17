using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
{
    public class InputManager : Component, IInputManager
    {
        static readonly HandlerSet Handlers = new HandlerSet(
            H<InputManager, SetInputModeEvent>((x,e) => x.SetInputMode(e)),
            H<InputManager, ISetMouseModeEvent>((x,e) => x.SetMouseMode(e))
        );

        readonly IDictionary<InputMode, IComponent> _inputModes = new Dictionary<InputMode, IComponent>();
        readonly IDictionary<MouseMode, IComponent> _mouseModes = new Dictionary<MouseMode, IComponent>();

        public InputMode InputMode { get; private set; } = InputMode.Global;
        public MouseMode MouseMode { get; private set; } = (MouseMode)(-1);

        public InputManager() : base(Handlers) { }

        public InputManager RegisterMouseMode(MouseMode mouseMode, IComponent implementation)
        {
            _mouseModes.Add(mouseMode, implementation);
            return this;
        }

        public InputManager RegisterInputMode(InputMode inputMode, IComponent implementation)
        {
            _inputModes.Add(inputMode, implementation);
            return this;
        }

        void SetMouseMode(ISetMouseModeEvent e)
        {
            if (MouseMode == e.Mode) return;

            foreach (var mode in _mouseModes)
            {
                if (mode.Key == e.Mode)
                    Exchange.Attach(mode.Value);
                else
                    mode.Value.Detach();
            }

            MouseMode = e.Mode;
            Raise(e); // Re-raise after changing in case the new mouse mode needs to access event properties / other controls need to refresh state.
        }

        void SetInputMode(SetInputModeEvent e)
        {
            if (InputMode == e.Mode) return;

            foreach (var mode in _inputModes)
            {
                if (mode.Key == e.Mode)
                    Exchange.Attach(mode.Value);
                else
                    mode.Value.Detach();
            }

            InputMode = e.Mode;
        }
    }
}
