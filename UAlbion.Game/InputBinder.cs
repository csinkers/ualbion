using System;
using System.Collections.Generic;
using UAlbion.Core;
using UAlbion.Core.Events;
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
        public override int GetHashCode() { unchecked { return ((int) Key * 397) ^ (int) Modifiers; } }
    }

    public class InputBinder : Component
    {
        static readonly Handler[] Handlers = {
            new Handler<InputBinder, KeyDownEvent>((x, e) => x.OnKeyDown(e)),
            new Handler<InputBinder, SceneChangedEvent>((x, e) => x.OnSceneChanged(e)),
            new Handler<InputBinder, UpdateEvent>((x, e) => x.OnUpdate(e)),
        };

        static KeyBinding Bind(Key key, ModifierKeys modifiers = ModifierKeys.None) => new KeyBinding(key, modifiers);

        IDictionary<InputMode, IDictionary<KeyBinding, string>> Bindings = new Dictionary<InputMode, IDictionary<KeyBinding, string>>
            {
                { InputMode.Global, new Dictionary<KeyBinding, string>
                    {
                        { Bind(Key.Escape), "toggle_menu" },
                        { Bind(Key.F5), "quicksave" },
                        { Bind(Key.F7), "quickload" },
                        { Bind(Key.Tilde), "toggle_console" },
                        { Bind(Key.F4, ModifierKeys.Alt), "quit" }
                    }
                },

                { InputMode.World2D, new Dictionary<KeyBinding, string> {
                    { Bind(Key.W), "+party_move  0 -1" },
                    { Bind(Key.S), "+party_move  0  1" },
                    { Bind(Key.A), "+party_move -1  0" },
                    { Bind(Key.D), "+party_move  1  0" },

                    { Bind(Key.Up),    "+party_move  0 -1|" },
                    { Bind(Key.Down),  "+party_move  0  1|" },
                    { Bind(Key.Left),  "+party_move -1  0|" },
                    { Bind(Key.Right), "+party_move  1  0|" },

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

                    { Bind(Key.Up),    "+party_move  0 -1|" },
                    { Bind(Key.Down),  "+party_move  0  1|" },
                    { Bind(Key.Left),  "+party_move -1  0|" },
                    { Bind(Key.Right), "+party_move  1  0|" },

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

        public InputBinder() : base(Handlers) { }

        void OnSceneChanged(SceneChangedEvent e)
        {
            // TODO: Change input mode and release any held events
        }

        void OnKeyDown(KeyDownEvent @event)
        {
            // TODO: Set held events and emit momentary events
        }

        void OnKeyUp(KeyDownEvent @event)
        {
            // TODO: Release matching held events
        }

        void OnUpdate(UpdateEvent engineUpdateEvent)
        {
            // TODO: Re-emit any held events
        }
    }

    [Event("e:key_down")] public class KeyDownEvent : EngineEvent { }

    [Event("e:scene_changed")] public class SceneChangedEvent : EngineEvent {
        public SceneChangedEvent(string sceneId) { SceneId = sceneId; }
        [EventPart("scene_id")] public string SceneId { get; }
    }

    /* SceneId:
        MainMenu
        Inventory
        Combat
        2D:10
        3D:5
     */
}
