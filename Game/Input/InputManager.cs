using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
{
    public class InputManager : Component, IInputManager
    {
        readonly IDictionary<InputMode, IComponent> _inputModes = new Dictionary<InputMode, IComponent>();
        readonly IDictionary<MouseMode, IComponent> _mouseModes = new Dictionary<MouseMode, IComponent>();
        readonly Stack<MouseMode> _mouseModeStack = new Stack<MouseMode>();
        readonly Stack<InputMode> _inputModeStack = new Stack<InputMode>();

        public InputMode InputMode { get; private set; } = InputMode.Global;
        public MouseMode MouseMode { get; private set; } = MouseMode.Normal;
        public IEnumerable<InputMode> InputModeStack => _inputModeStack;
        public IEnumerable<MouseMode> MouseModeStack => _mouseModeStack;

        public InputManager()
        {
            On<SetInputModeEvent>(SetInputMode);
            On<SetMouseModeEvent>(SetMouseMode);
            On<SetExclusiveMouseModeEvent>(SetMouseMode);
            On<PushMouseModeEvent>(e =>
            {
                var inputManager = Resolve<IInputManager>();
                _mouseModeStack.Push(inputManager.MouseMode);
                var setEvent = new SetMouseModeEvent(e.Mode);
                SetMouseMode(setEvent);
                Raise(setEvent);
            });
            On<PopMouseModeEvent>(e =>
            {
                if (_mouseModeStack.Count == 0) 
                    return;

                var setEvent = new SetMouseModeEvent(_mouseModeStack.Pop());
                SetMouseMode(setEvent);
                Raise(setEvent);
            });
            On<PushInputModeEvent>(e =>
            {
                var inputManager = Resolve<IInputManager>();
                _inputModeStack.Push(inputManager.InputMode);
                var setEvent = new SetInputModeEvent(e.Mode);
                SetInputMode(setEvent);
                Raise(setEvent);
            });
            On<PopInputModeEvent>(e =>
            {
                if (_inputModeStack.Count == 0) 
                    return;

                var setEvent = new SetInputModeEvent(_inputModeStack.Pop());
                SetInputMode(setEvent);
                Raise(setEvent);
            });
        }

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
                if (mode.Key != e.Mode)
                    mode.Value.Detach();

            foreach (var mode in _mouseModes)
                if (mode.Key == e.Mode)
                    Exchange.Attach(mode.Value);

            MouseMode = e.Mode;
        }

        void SetInputMode(SetInputModeEvent e)
        {
            if (InputMode == e.Mode) return;

            foreach (var mode in _inputModes)
                if (mode.Key != e.Mode)
                    mode.Value.Detach();

            foreach (var mode in _inputModes)
                if (mode.Key == e.Mode)
                    Exchange.Attach(mode.Value);

            InputMode = e.Mode;
        }
    }
}
