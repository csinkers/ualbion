﻿using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api.Eventing;
using UAlbion.Config;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Formats.Config;
using UAlbion.Formats.Ids;
using UAlbion.Game.Events;
using UAlbion.Game.Input;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input;

public class InputBinder : GameServiceComponent<IInputBinder>, IInputBinder
{
    class BindingSet : Dictionary<InputMode, IDictionary<KeyBinding, string>> { }

    readonly BindingSet _bindings = new();
    readonly HashSet<Key> _pressedKeys = new();
    MapId _mapId = Base.Map.TestMapIskai;

    public bool IsAltPressed => _pressedKeys.Contains(Key.AltLeft) || _pressedKeys.Contains(Key.AltRight);
    public bool IsCtrlPressed => _pressedKeys.Contains(Key.ControlLeft) || _pressedKeys.Contains(Key.ControlRight);
    public bool IsShiftPressed => _pressedKeys.Contains(Key.ShiftLeft) || _pressedKeys.Contains(Key.ShiftRight);

    public InputBinder()
    {
        On<KeyboardInputEvent>(OnKeyboard);
        On<RebindInputEvent>(_ => Rebind());
        On<LoadMapEvent>(e => _mapId = e.MapId);
        On<AssetUpdatedEvent>(e =>
        {
            if (e.Id == AssetId.From(Base.Special.InputConfig))
                Rebind();
        });
    }

    protected override void Subscribed() => Rebind();

    void Rebind()
    {
        var config = Assets.LoadInputConfig();
        if (config == null)
            throw new InvalidOperationException("Input config could not be loaded");

        _bindings.Clear();

        foreach (var rawMode in config.Bindings)
        {
            if (!_bindings.ContainsKey(rawMode.Key))
                _bindings.Add(rawMode.Key, new Dictionary<KeyBinding, string>());

            var mode = _bindings[rawMode.Key];
            foreach (var rawBinding in rawMode.Value)
            {
                var parts = rawBinding.Key.Split('+').Select(x => x.Trim().ToUpperInvariant()).ToArray();
                Key key = Key.LastKey;
                var modifiers = ModifierKeys.None;
                for (int i = 0; i < parts.Length; i++)
                {
                    if (i == parts.Length - 1)
                    {
                        if (int.TryParse(parts[i], out var numeric))
                            key = Key.Number0 + numeric;
                        else
                            key = Enum.Parse<Key>(parts[i], true);
                    }
                    else
                        modifiers |= Enum.Parse<ModifierKeys>(parts[i], true);
                }

                if (key != Key.LastKey)
                    mode[new KeyBinding(key, modifiers)] = rawBinding.Value;
            }
        }
    }

    public IEnumerable<(InputMode, IEnumerable<(string, string)>)> Bindings
    {
        get
        {
            foreach (var mode in _bindings)
                yield return (mode.Key, mode.Value.Select(x => (x.Key.ToString(), x.Value)));
        }
    }

    ModifierKeys Modifiers
    {
        get
        {
            ModifierKeys m = ModifierKeys.None;
            if (IsShiftPressed) m |= ModifierKeys.Shift;
            if (IsCtrlPressed) m |= ModifierKeys.Control;
            if (IsAltPressed) m |= ModifierKeys.Alt;
            return m;
        }
    }

    static bool IsModifier(Key key)
    {
        switch (key)
        {
            case Key.LControl:
            case Key.RControl:
            case Key.LShift:
            case Key.RShift:
            case Key.LAlt:
            case Key.RAlt:
            case Key.LWin:
            case Key.RWin:
                return true;
            default:
                return false;
        }
    }

    void OnKeyboard(KeyboardInputEvent e)
    {
        var inputManager = Resolve<IInputManager>();

        // e.KeyEvents is IReadOnlyList, so using foreach will allocate an enumerator
        // ReSharper disable once ForCanBeConvertedToForeach
        for (var index = 0; index < e.KeyEvents.Count; index++)
        {
            var keyEvent = e.KeyEvents[index];
            if (!keyEvent.Down)
            {
                _pressedKeys.Remove(keyEvent.Key);
                continue;
            }

            _pressedKeys.Add(keyEvent.Key);

            if (IsModifier(keyEvent.Key))
                continue;

            var binding = new KeyBinding(keyEvent.Key, keyEvent.Modifiers);
            var mode = _bindings.ContainsKey(inputManager.InputMode) ? inputManager.InputMode : InputMode.Global;
            if (mode == InputMode.TextEntry)
                continue;

            if (!_bindings[mode].TryGetValue(binding, out var action))
                if (!_bindings[InputMode.Global].TryGetValue(binding, out action))
                    continue;

            action = action.Trim();
            if (action.StartsWith('+')) // Continuous actions are handled later
                continue;

            if (action == "!loadprevmap")
            {
                _mapId = MapId.FromUInt32(_mapId.ToUInt32() - 1);
                Raise(new LoadMapEvent(_mapId));
                continue;
            }

            if (action == "!loadnextmap")
            {
                _mapId = MapId.FromUInt32(_mapId.ToUInt32() + 1);
                Raise(new LoadMapEvent(_mapId));
                continue;
            }

            var actionEvent = Event.Parse(action, out var error);
            Raise(actionEvent ?? new LogEvent(LogLevel.Error, $"The action \"{action}\" could not be parsed: {error}"));
        }

        // Handle continuous bindings
        foreach (var key in _pressedKeys)
        {
            var binding = new KeyBinding(key, Modifiers);
            var mode = _bindings.ContainsKey(inputManager.InputMode) ? inputManager.InputMode : InputMode.Global;
            if (!_bindings[mode].TryGetValue(binding, out var action))
                if (!_bindings[InputMode.Global].TryGetValue(binding, out action))
                    continue;

            action = action.Trim();
            if (!action.StartsWith('+'))
                continue;

            var actionEvent = Event.Parse(action[1..], out _);
            if (actionEvent != null)
                Raise(actionEvent);
        }
    }

    /*
            void OnUpdate(FastClockEvent fastClockEvent)
            {
                // TODO: Re-emit any held events
            }


            public bool GetKey(Key key) { return CurrentlyPressedKeys.Contains(key); }
            public bool GetKeyDown(Key key) { return NewKeysThisFrame.Contains(key); }
            public bool GetMouseButton(MouseButton button) { return CurrentlyPressedMouseButtons.Contains(button); }
            public bool GetMouseButtonDown(MouseButton button) { return NewMouseButtonsThisFrame.Contains(button); }
            */
}
