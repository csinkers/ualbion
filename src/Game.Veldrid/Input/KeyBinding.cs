using System;
using Veldrid;

namespace UAlbion.Game.Veldrid.Input;

public struct KeyBinding : IEquatable<KeyBinding>
{
    public Key Key { get; }
    public ModifierKeys Modifiers { get; }
    public KeyBinding(Key key, ModifierKeys modifiers) { Key = key; Modifiers = modifiers; }

    public bool Equals(KeyBinding other) { return Key == other.Key && Modifiers == other.Modifiers; }
    public override bool Equals(object obj) { return obj is KeyBinding other && Equals(other); }
    public static bool operator ==(KeyBinding left, KeyBinding right) => left.Equals(right);
    public static bool operator !=(KeyBinding left, KeyBinding right) => !(left == right);
    public override int GetHashCode() { unchecked { return ((int)Key * 397) ^ (int)Modifiers; } }
    public override string ToString() => $"{Modifiers} + {Key}";
}