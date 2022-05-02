using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using UAlbion.Game.Settings;

namespace UAlbion.Game.Input;

public class InputManager : ServiceComponent<IInputManager>, IInputManager
{
    readonly IDictionary<InputMode, IComponent> _inputModes = new Dictionary<InputMode, IComponent>();
    readonly IDictionary<MouseMode, IComponent> _mouseModes = new Dictionary<MouseMode, IComponent>();
    readonly Stack<MouseMode> _mouseModeStack = new();
    readonly Stack<InputMode> _inputModeStack = new();
    InputMode _inputMode = InputMode.Global;
    MouseMode _mouseMode = MouseMode.Normal;

    public InputMode InputMode => (Resolve<ISettings>().Debug.DebugFlags & DebugFlags.ShowConsole) != 0 ? InputMode.TextEntry : _inputMode;
    public MouseMode MouseMode => (Resolve<ISettings>().Debug.DebugFlags & DebugFlags.ShowConsole) != 0 ? MouseMode.Normal :_mouseMode;
    public IEnumerable<InputMode> InputModeStack => _inputModeStack;
    public IEnumerable<MouseMode> MouseModeStack => _mouseModeStack;

    public InputManager()
    {
        On<InputModeEvent>(SetInputMode);
        On<MouseModeEvent>(e => SetMouseMode(e.Mode));
        On<ToggleMouseLookEvent>(_ =>
        {
            switch (_mouseMode)
            {
                case MouseMode.Normal: SetMouseMode(MouseMode.MouseLook); break;
                case MouseMode.MouseLook: SetMouseMode(MouseMode.Normal); break;
            }
        });
        On<PushMouseModeEvent>(e =>
        {
            _mouseModeStack.Push(_mouseMode);
            var setEvent = new MouseModeEvent(e.Mode);
            SetMouseMode(setEvent.Mode);
            Raise(setEvent);
        });
        On<PopMouseModeEvent>(e =>
        {
            if (_mouseModeStack.Count == 0) 
                return;

            var setEvent = new MouseModeEvent(_mouseModeStack.Pop());
            SetMouseMode(setEvent.Mode);
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

    void SetMouseMode(MouseMode? mode)
    {
        if (mode == null)
        {
            Info($"MouseMode: {MouseMode} (Stack: {string.Join(", ", _mouseModeStack)})");
            return;
        }

        _mouseModes.TryGetValue(mode.Value, out var activeMode);
        if (_mouseMode == mode && activeMode?.IsActive == true)
            return;

        foreach (var otherMode in _mouseModes.Values)
            otherMode.IsActive = false;

        if (activeMode != null)
            activeMode.IsActive = true;

        _mouseMode = mode.Value;
    }

    void SetInputMode(InputModeEvent e)
    {
        if (e.Mode == null)
        {
            Info($"InputMode: {InputMode} (Stack: {string.Join(", ", _inputModeStack)})");
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