using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UAlbion.Core;
using UAlbion.Core.Events;
using UAlbion.Game.Events;
using Veldrid;

namespace UAlbion.Game
{
    public enum InputMode
    {
        Global,
        Console,
        Menu,
        World2D,
        World3D,
        Map,
        Inventory,
        Combat,
        Conversation
    }

    public enum CursorMode
    {
        Normal,
        Examine,
        Interact,
        PathFinding
    }

    public struct KeyBinding : IEquatable<KeyBinding>
    {
        public Key Key { get; }
        public ModifierKeys Modifiers { get; }
        public KeyBinding(Key key, ModifierKeys modifiers) { Key = key; Modifiers = modifiers; }
        public bool Equals(KeyBinding other) { return Key == other.Key && Modifiers == other.Modifiers; }
        public override bool Equals(object obj) { return obj is KeyBinding other && Equals(other); }
        public override int GetHashCode() { unchecked { return ((int)Key * 397) ^ (int)Modifiers; } }
    }

    public class InputBinder : Component
    {
        static readonly Handler[] Handlers = {
            new Handler<InputBinder, ChangeInputModeEvent>((x, e) => x.OnInputModeChanged(e)),
            new Handler<InputBinder, InputEvent>((x, e) => x.OnInput(e)),
        };

        static KeyBinding Bind(Key key, ModifierKeys modifiers = ModifierKeys.None) => new KeyBinding(key, modifiers);

        // TODO: Load bindings from config
        IDictionary<InputMode, IDictionary<KeyBinding, string>> _bindings = new Dictionary<InputMode, IDictionary<KeyBinding, string>>
            {
                { InputMode.Global, new Dictionary<KeyBinding, string>
                    {
                        { Bind(Key.Escape), "toggle_menu" },
                        { Bind(Key.F5), "quicksave" },
                        { Bind(Key.F7), "quickload" },
                        { Bind(Key.Tilde), "toggle_console" },
                        { Bind(Key.F4, ModifierKeys.Alt), "quit" },
                        { Bind(Key.BracketLeft), "e:mag -1" },
                        { Bind(Key.BracketRight), "e:mag 1" },
                    }
                },

                { InputMode.World2D, new Dictionary<KeyBinding, string> {
                    { Bind(Key.Keypad4), "+e:camera_move -64  0" },
                    { Bind(Key.Keypad6), "+e:camera_move  64  0" },
                    { Bind(Key.Keypad8), "+e:camera_move  0 -64" },
                    { Bind(Key.Keypad2), "+e:camera_move  0  64" },

                    { Bind(Key.W), "+e:camera_move  0 -64" },
                    { Bind(Key.A), "+e:camera_move -64  0" },
                    { Bind(Key.S), "+e:camera_move  0  64" },
                    { Bind(Key.D), "+e:camera_move  64  0" },

                    /*
                    { Bind(Key.W), "+party_move  0 -1" },
                    { Bind(Key.A), "+party_move -1  0" },
                    { Bind(Key.S), "+party_move  0  1" },
                    { Bind(Key.D), "+party_move  1  0" },
                    //*/

                    { Bind(Key.Up),    "+party_move  0 -1" },
                    { Bind(Key.Down),  "+party_move  0  1" },
                    { Bind(Key.Left),  "+party_move -1  0" },
                    { Bind(Key.Right), "+party_move  1  0" },

                    { Bind(Key.Tab), "open_inventory" },

                    { Bind(Key.Number1), "set_active_member 0" },
                    { Bind(Key.Number2), "set_active_member 1" },
                    { Bind(Key.Number3), "set_active_member 2" },
                    { Bind(Key.Number4), "set_active_member 3" },
                    { Bind(Key.Number5), "set_active_member 4" },

                    { Bind(Key.Q), "cursor_mode look" },
                    { Bind(Key.E), "cursor_mode manipulate" },
                } },

                { InputMode.World3D, new Dictionary<KeyBinding, string> {
                    { Bind(Key.W), "+party_move  0 -1" },
                    { Bind(Key.S), "+party_move  0  1" },
                    { Bind(Key.A), "+party_move -1  0" },
                    { Bind(Key.D), "+party_move  1  0" },

                    { Bind(Key.Up),    "+party_move  0 -1" },
                    { Bind(Key.Down),  "+party_move  0  1" },
                    { Bind(Key.Left),  "+party_move -1  0" },
                    { Bind(Key.Right), "+party_move  1  0" },

                    { Bind(Key.Tab), "open_inventory" },

                    { Bind(Key.Number1), "set_active_member 0" },
                    { Bind(Key.Number2), "set_active_member 1" },
                    { Bind(Key.Number3), "set_active_member 2" },
                    { Bind(Key.Number4), "set_active_member 3" },
                    { Bind(Key.Number5), "set_active_member 4" },

                    { Bind(Key.Q), "+cursor_mode examine" },
                    { Bind(Key.E), "+cursor_mode interact" },
                    { Bind(Key.M), "open_map" },
                    { Bind(Key.T), "wait" },
                } },
                { InputMode.Inventory, new Dictionary<KeyBinding, string> {
                    { Bind(Key.A, ModifierKeys.Control), "take_all" },
                    { Bind(Key.Escape), "close_inventory" },
                }},
                { InputMode.Conversation, new Dictionary<KeyBinding, string> {
                    { Bind(Key.Number1), "respond 1" },
                    { Bind(Key.Number2), "respond 2" },
                    { Bind(Key.Number3), "respond 3" },
                    { Bind(Key.Number4), "respond 4" },
                    { Bind(Key.Number5), "respond 5" },
                    { Bind(Key.Number6), "respond 6" },
                    { Bind(Key.Number7), "respond 7" },
                    { Bind(Key.Escape), "end_conversation" },
                }},
        };

        readonly HashSet<Key> _pressedKeys = new HashSet<Key>();
        readonly HashSet<MouseButton> _pressedMouseButtons = new HashSet<MouseButton>();
        InputMode _activeMode = InputMode.World2D;
        Vector2 _mousePosition;
        Vector2 _mouseDelta;

        ModifierKeys _modifiers
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

        public InputBinder() : base(Handlers) { }

        void OnInputModeChanged(ChangeInputModeEvent e)
        {
            _activeMode = e.Mode;
        }

        void OnInput(InputEvent e)
        {
            _mousePosition = e.Snapshot.MousePosition;
            _mouseDelta = e.MouseDelta;

            foreach (var keyEvent in e.Snapshot.KeyEvents)
            {
                if (keyEvent.Down) _pressedKeys.Add(keyEvent.Key);
                else _pressedKeys.Remove(keyEvent.Key);

                var binding = new KeyBinding(keyEvent.Key, keyEvent.Modifiers);
                if (!_bindings[_activeMode].TryGetValue(binding, out var action))
                    if (!_bindings[InputMode.Global].TryGetValue(binding, out action))
                        continue;

                action = action.Trim();
                if (action.StartsWith('+')) // Continuous actions are handled later
                    continue;

                var actionEvent = Event.Parse(action);
                if(actionEvent != null)
                    Raise(actionEvent);
            }

            // Handle continuous bindings
            foreach(var key in _pressedKeys)
            {
                var binding = new KeyBinding(key, _modifiers);
                if (!_bindings[_activeMode].TryGetValue(binding, out var action))
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

    //[Event("change_input_mode", "Changes the currently active input mode / keybindings")]
    class ChangeInputModeEvent : GameEvent
    {
        //[EventPart("mode")]
        public InputMode Mode { get; }
        public ChangeInputModeEvent(InputMode mode)
        {
            Mode = mode;
        }
    }

    /* SceneId:
        MainMenu
        Inventory
        Combat
        2D:10
        3D:5
     */
}
