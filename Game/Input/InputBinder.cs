using System;
using System.Collections.Generic;
using System.Linq;
using UAlbion.Api;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Formats.AssetIds;
using UAlbion.Formats.Config;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game.Input
{
    public class InputBinder : Component
    {
        class Bindings : Dictionary<InputMode, IDictionary<KeyBinding, string>> { }

        static readonly HandlerSet Handlers = new HandlerSet(
            H<InputBinder, InputEvent>((x, e) => x.OnInput(e)),
            H<InputBinder, LoadMapEvent>((x, e) => x._mapId = e.MapId)
        );

        public InputBinder(InputConfig config) : base(Handlers)
        {
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
                            key = Enum.Parse<Key>(parts[i], true);
                        else
                            modifiers |= Enum.Parse<ModifierKeys>(parts[i], true);
                    }

                    if(key != Key.LastKey)
                        mode[new KeyBinding(key, modifiers)] = rawBinding.Value;
                }
            }
        }

        readonly Bindings _bindings = new Bindings();
        readonly HashSet<Key> _pressedKeys = new HashSet<Key>();
        // InputMode _activeMode = InputMode.Global;
        MapDataId _mapId = (MapDataId)100;

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

        void OnInput(InputEvent e)
        {
            var inputManager = Exchange.Resolve<IInputManager>();
            foreach (var keyEvent in e.Snapshot.KeyEvents)
            {
                if (!keyEvent.Down)
                {
                    _pressedKeys.Remove(keyEvent.Key);
                    continue;
                }

                _pressedKeys.Add(keyEvent.Key);

                var binding = new KeyBinding(keyEvent.Key, keyEvent.Modifiers);
                if (!_bindings[inputManager.InputMode].TryGetValue(binding, out var action))
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
                if (!_bindings[inputManager.InputMode].TryGetValue(binding, out var action))
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

        void OnUpdate(UpdateEvent engineUpdateEvent)
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
