﻿using System;
using System.Collections.Generic;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using UAlbion.Game.Settings;

namespace UAlbion.Game.Input
{
    public class InputManager : ServiceComponent<IInputManager>, IInputManager
    {
        readonly IDictionary<InputMode, IComponent> _inputModes = new Dictionary<InputMode, IComponent>();
        readonly IDictionary<MouseMode, IComponent> _mouseModes = new Dictionary<MouseMode, IComponent>();
        readonly Stack<MouseMode> _mouseModeStack = new Stack<MouseMode>();
        readonly Stack<InputMode> _inputModeStack = new Stack<InputMode>();
        InputMode _inputMode = InputMode.Global;
        MouseMode _mouseMode = MouseMode.Normal;

        public InputMode InputMode => (Resolve<ISettings>().Debug.DebugFlags & DebugFlags.ShowConsole) != 0 ? InputMode.TextEntry : _inputMode;
        public MouseMode MouseMode => (Resolve<ISettings>().Debug.DebugFlags & DebugFlags.ShowConsole) != 0 ? MouseMode.Normal :_mouseMode;
        public IEnumerable<InputMode> InputModeStack => _inputModeStack;
        public IEnumerable<MouseMode> MouseModeStack => _mouseModeStack;

        public InputManager()
        {
            On<InputModeEvent>(SetInputMode);
            On<MouseModeEvent>(SetMouseMode);
            On<PushMouseModeEvent>(e =>
            {
                _mouseModeStack.Push(_mouseMode);
                var setEvent = new MouseModeEvent(e.Mode);
                SetMouseMode(setEvent);
                Raise(setEvent);
            });
            On<PopMouseModeEvent>(e =>
            {
                if (_mouseModeStack.Count == 0) 
                    return;

                var setEvent = new MouseModeEvent(_mouseModeStack.Pop());
                SetMouseMode(setEvent);
                Raise(setEvent);
            });
            On<PushInputModeEvent>(e =>
            {
                _inputModeStack.Push(_inputMode);
                var setEvent = new InputModeEvent(e.Mode);
                SetInputMode(setEvent);
                Raise(setEvent);
            });
            On<PopInputModeEvent>(e =>
            {
                if (_inputModeStack.Count == 0) 
                    return;

                var setEvent = new InputModeEvent(_inputModeStack.Pop());
                SetInputMode(setEvent);
                Raise(setEvent);
            });
        }

        public InputManager RegisterMouseMode(MouseMode mouseMode, IComponent implementation)
        {
            if (implementation == null) throw new ArgumentNullException(nameof(implementation));
            _mouseModes.Add(mouseMode, implementation);
            implementation.IsActive = false;
            AttachChild(implementation);
            return this;
        }

        public InputManager RegisterInputMode(InputMode inputMode, IComponent implementation)
        {
            if (implementation == null) throw new ArgumentNullException(nameof(implementation));
            _inputModes.Add(inputMode, implementation);
            implementation.IsActive = false;
            AttachChild(implementation);
            return this;
        }

        void SetMouseMode(MouseModeEvent e)
        {
            if (e.Mode == null)
            {
                Raise(new LogEvent(LogEvent.Level.Info,
                    $"MouseMode: {MouseMode} (Stack: {string.Join(", ", _mouseModeStack)})"));
                return;
            }

            _mouseModes.TryGetValue(e.Mode.Value, out var activeMode);
            if (_mouseMode == e.Mode && activeMode?.IsActive == true)
                return;

            foreach (var mode in _mouseModes.Values)
                mode.IsActive = false;

            if (activeMode != null)
                activeMode.IsActive = true;

            _mouseMode = e.Mode.Value;
        }

        void SetInputMode(InputModeEvent e)
        {
            if (e.Mode == null)
            {
                Raise(new LogEvent(LogEvent.Level.Info,
                    $"InputMode: {InputMode} (Stack: {string.Join(", ", _inputModeStack)})"));
                return;
            }

            _inputModes.TryGetValue(e.Mode.Value, out var activeMode);
            if (_inputMode == e.Mode && activeMode?.IsActive == true)
                return;

            foreach (var mode in _inputModes.Values)
                mode.IsActive = false;

            if (activeMode != null)
                activeMode.IsActive = true;

            _inputMode = e.Mode.Value;
        }
    }
}
