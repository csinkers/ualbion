using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input
{
    public class InputManager : ServiceComponent<IInputManager>, IInputManager
    {
        readonly IDictionary<InputMode, IComponent> _inputModes = new Dictionary<InputMode, IComponent>();
        readonly IDictionary<MouseMode, IComponent> _mouseModes = new Dictionary<MouseMode, IComponent>();
        readonly Stack<MouseMode> _mouseModeStack = new Stack<MouseMode>();
        readonly Stack<InputMode> _inputModeStack = new Stack<InputMode>();

        public InputMode InputMode { get; private set; } = InputMode.Global;
        public MouseMode MouseMode { get; private set; } = MouseMode.Normal; // (MouseMode)(int)-1;
        public IEnumerable<InputMode> InputModeStack => _inputModeStack;
        public IEnumerable<MouseMode> MouseModeStack => _mouseModeStack;

        public InputManager()
        {
            On<SetInputModeEvent>(SetInputMode);
            On<SetMouseModeEvent>(SetMouseMode);
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
            implementation.IsActive = false;
            AttachChild(implementation);
            return this;
        }

        public InputManager RegisterInputMode(InputMode inputMode, IComponent implementation)
        {
            _inputModes.Add(inputMode, implementation);
            implementation.IsActive = false;
            AttachChild(implementation);
            return this;
        }

        void SetMouseMode(SetMouseModeEvent e)
        {
            _mouseModes.TryGetValue(e.Mode, out var activeMode);
            if (MouseMode == e.Mode && activeMode?.IsActive == true)
                return;

            foreach (var mode in _mouseModes.Values)
                mode.IsActive = false;

            if (activeMode != null)
                activeMode.IsActive = true;

            MouseMode = e.Mode;
        }

        void SetInputMode(SetInputModeEvent e)
        {
            _inputModes.TryGetValue(e.Mode, out var activeMode);
            if (InputMode == e.Mode && activeMode?.IsActive == true)
                return;

            foreach (var mode in _inputModes.Values)
                mode.IsActive = false;

            if (activeMode != null)
                activeMode.IsActive = true;

            InputMode = e.Mode;
        }
    }
}
