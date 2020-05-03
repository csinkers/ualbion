﻿using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Veldrid.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using UAlbion.Game.Input;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input
{
    public class InputBinder : ServiceComponent<IInputBinder>, IInputBinder
    {
        class BindingSet : Dictionary<InputMode, IDictionary<KeyBinding, string>> { }

        readonly BindingSet _bindings = new BindingSet();
        readonly HashSet<Key> _pressedKeys = new HashSet<Key>();
        // InputMode _activeMode = InputMode.Global;
        MapDataId _mapId = (MapDataId)100;

        public InputBinder(InputConfig config)
        {
            On<InputEvent>(OnInput);
            On<LoadMapEvent>(e => _mapId = e.MapId);

            foreach (var rawMode in config.Bindings)
            {
                if (!_bindings.ContainsKey(rawMode.Key))
                    _bindings.Add(rawMode.Key, new Dictionary<KeyBinding, string>());

                var mode = _bindings[rawMode.Key];
                foreach (var rawBinding in rawMode.Value)
                {
                    var parts = rawBinding.Key.Split('+').Select(x => x.Trim().ToLower()).ToArray();
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

                    if(key != Key.LastKey)
                        mode[new KeyBinding(key, modifiers)] = rawBinding.Value;
                }
            }
        }

        public IEnumerable<(InputMode, IEnumerable<(string, string)>)> Bindings
        {
            get
            {
                foreach (var mode in _bindings)
                    yield return(mode.Key, mode.Value.Select(x => (x.Key.ToString(), x.Value)));
            }
        }

        ModifierKeys Modifiers
        {
            get
            {
                ModifierKeys m = ModifierKeys.None;
                if (_pressedKeys.Overlaps(new[] { Key.ShiftLeft, Key.ShiftRight, Key.LShift, Key.RShift }))
                    m |= ModifierKeys.Shift;

                if (_pressedKeys.Overlaps(new[] { Key.ControlLeft, Key.ControlRight, Key.LControl, Key.RControl }))
                    m |= ModifierKeys.Control;

                if (_pressedKeys.Overlaps(new[] { Key.AltLeft, Key.AltRight, Key.LAlt, Key.RAlt }))
                    m |= ModifierKeys.Alt;

                return m;
            }
        }

        static bool IsModifier(Key key)
        {
            switch(key)
            {
                case Key.LControl: case Key.RControl:
                case Key.LShift:   case Key.RShift:
                case Key.LAlt:     case Key.RAlt:
                case Key.LWin:     case Key.RWin:
                    return true;
                default:
                    return false;
            }
        }

        void OnInput(InputEvent e)
        {
            var inputManager = Resolve<IInputManager>();
            foreach (var keyEvent in e.Snapshot.KeyEvents)
            {
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
                if (!_bindings[mode].TryGetValue(binding, out var action))
                    if (!_bindings[InputMode.Global].TryGetValue(binding, out action))
                        continue;

                action = action.Trim();
                if (action.StartsWith('+')) // Continuous actions are handled later
                    continue;

                if (action == "!loadprevmap")
                {
                    _mapId--;
                    Raise(new LoadMapEvent(_mapId));
                    continue;
                }
                if (action == "!loadnextmap")
                {
                    _mapId++;
                    Raise(new LoadMapEvent(_mapId));
                    continue;
                }

                var actionEvent = Event.Parse(action);
                Raise(actionEvent ?? new LogEvent(LogEvent.Level.Error, $"The action \"{action}\" could not be parsed."));
            }

            // Handle continuous bindings
            foreach(var key in _pressedKeys)
            {
                var binding = new KeyBinding(key, Modifiers);
                var mode = _bindings.ContainsKey(inputManager.InputMode) ? inputManager.InputMode : InputMode.Global;
                if (!_bindings[mode].TryGetValue(binding, out var action))
                    if (!_bindings[InputMode.Global].TryGetValue(binding, out action))
                        continue;

                action = action.Trim();
                if (!action.StartsWith('+'))
                    continue;

                var actionEvent = Event.Parse(action.Substring(1));
                if(actionEvent != null)
                    Raise(actionEvent);
            }
        }

        void OnUpdate(FastClockEvent fastClockEvent)
        {
            // TODO: Re-emit any held events
        }

        /*
        public bool GetKey(Key key) { return CurrentlyPressedKeys.Contains(key); }
        public bool GetKeyDown(Key key) { return NewKeysThisFrame.Contains(key); }
        public bool GetMouseButton(MouseButton button) { return CurrentlyPressedMouseButtons.Contains(button); }
        public bool GetMouseButtonDown(MouseButton button) { return NewMouseButtonsThisFrame.Contains(button); }
        */
    }
}
