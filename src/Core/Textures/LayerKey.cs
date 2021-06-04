using System;

namespace UAlbion.Core.Textures
{
    public readonly struct LayerKey : IEquatable<LayerKey>
    {
        readonly int _id;
        readonly int _frame;
        public LayerKey(int id, int frame) { _id = id; _frame = frame; }
        public bool Equals(LayerKey other) => _id == other._id && _frame == other._frame;
        public override bool Equals(object obj) => obj is LayerKey other && Equals(other);
        public static bool operator ==(LayerKey left, LayerKey right) => left.Equals(right);
        public static bool operator !=(LayerKey left, LayerKey right) => !(left == right);
        public override int GetHashCode() { unchecked { return (_id * 397) ^ _frame; } }
        public override string ToString() => $"LK{_id}.{_frame}";
    }
}