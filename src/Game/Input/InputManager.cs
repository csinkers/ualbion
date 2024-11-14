using System;
using System.Collections.Generic;
using UAlbion.Api.Eventing;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;

namespace UAlbion.Game.Input;

public class InputManager : ServiceComponent<IInputManager>, IInputManager
{
    readonly Dictionary<InputMode, IComponent> _inputModes = [];
    readonly Dictionary<MouseMode, IComponent> _mouseModes = [];
    readonly Stack<MouseMode> _mouseModeStack = new();
    readonly Stack<InputMode> _inputModeStack = new();

    public InputMode InputMode { get; private set; } = InputMode.Global;
    public MouseMode MouseMode { get; private set; } = MouseMode.Normal;
    public IEnumerable<InputMode> InputModeStack => _inputModeStack;
    public IEnumerable<MouseMode> MouseModeStack => _mouseModeStack;

    public InputManager()
    {
        On<InputModeEvent>(SetInputMode);
        On<MouseModeEvent>(e => SetMouseMode(e.Mode));
        On<ToggleMouseLookEvent>(_ =>
        {
            switch (MouseMode)
            {
                case MouseMode.Normal: SetMouseMode(MouseMode.MouseLook); break;
                case MouseMode.MouseLook: SetMouseMode(MouseMode.Normal); break;
            }
        });
        On<PushMouseModeEvent>(e =>
        {
            _mouseModeStack.Push(MouseMode);
            var setEvent = new MouseModeEvent(e.Mode);
            SetMouseMode(setEvent.Mode);
            Raise(setEvent);
        });
        On<PopMouseModeEvent>(_ =>
        {
            if (_mouseModeStack.Count == 0) 
                return;

            var setEvent = new MouseModeEvent(_mouseModeStack.Pop());
            SetMouseMode(setEvent.Mode);
            Raise(setEvent);
        });
        On<PushInputModeEvent>(e =>
        {
            _inputModeStack.Push(InputMode);
            var setEvent = new InputModeEvent(e.Mode);
            SetInputMode(setEvent);
            Raise(setEvent);
        });
        On<PopInputModeEvent>(_ =>
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
        ArgumentNullException.ThrowIfNull(implementation);
        _mouseModes.Add(mouseMode, implementation);
        implementation.IsActive = false;
        AttachChild(implementation);
        return this;
    }

    public InputManager RegisterInputMode(InputMode inputMode, IComponent implementation)
    {
        ArgumentNullException.ThrowIfNull(implementation);
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
        if (MouseMode == mode && activeMode?.IsActive == true)
            return;

        foreach (var otherMode in _mouseModes.Values)
            otherMode.IsActive = false;

        if (activeMode != null)
            activeMode.IsActive = true;

        MouseMode = mode.Value;
    }

    void SetInputMode(InputModeEvent e)
    {
        if (e.Mode == null)
        {
            Info($"InputMode: {InputMode} (Stack: {string.Join(", ", _inputModeStack)})");
            return;
        }

        _inputModes.TryGetValue(e.Mode.Value, out var activeMode);
        if (InputMode == e.Mode && activeMode?.IsActive == true)
            return;

        foreach (var mode in _inputModes.Values)
            mode.IsActive = false;

        if (activeMode != null)
            activeMode.IsActive = true;

        InputMode = e.Mode.Value;
    }
}